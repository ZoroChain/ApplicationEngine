using System;
using System.Collections.Generic;
using System.Linq;

using AEServer.Service;

namespace AEServer.Session
{
    public struct AESessionManagerConfig
    {
        // expire time in seconds
        public ulong sessionExpireTime;

        public ulong checkSessionExipreInterval;
        public int checkSessionExipreCount;
    }

    public class AESessionManager: ISessionManager
    {
        protected ulong _lastCheckExpireTime = 0;

        protected ulong _sessionIDSeed = 1;
        protected object _sessionsLock = new Object();
        protected Dictionary<ulong, ISession> _sessionsByID = new Dictionary<ulong, ISession>();

        protected object _sessionServiceLock = new Object();
        protected Dictionary<ISession, IService> _sessionServicePair = new Dictionary<ISession, IService>();

        protected IListener[] _listeners = new IListener[(int)ListenerType.LT_Count];

        protected AESessionManagerConfig _conf;

        public AESessionManagerConfig conf
        {
            get
            {
                return _conf;
            }
        }

        public bool init(object config)
        {
            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Start Initialize Session Manager...");

            dynamic conf = config;

            // TO DO : init session manager by config

            dynamic listenerConf = conf.Listener;

            _conf.sessionExpireTime = (ulong)conf.sessionExpireTime;
            _conf.checkSessionExipreInterval = (ulong)conf.checkSessionExipreInterval;
            _conf.checkSessionExipreCount = (int)conf.checkSessionExipreCount;

            HttpListener htl = new HttpListener(this);
            htl.init(listenerConf.HTTP);
            _listeners[(int)ListenerType.LT_HTTP] = htl;
            
            if(AEHelper.HasProperty(listenerConf, "SignalR"))
            {
                SignalRListener srl = new SignalRListener(this);
                srl.init(listenerConf.SignalR);

                _listeners[(int)ListenerType.LT_SIGNALR] = srl;
            }

            if(AEHelper.HasProperty(listenerConf, "WebSocket"))
            {
                WebSocketListener wbsl = new WebSocketListener(this);
                wbsl.init(listenerConf.WebSocket);

                _listeners[(int)ListenerType.LT_WEBSOCKET] = wbsl;
            }

            // TO DO : init other listener by config

            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Initialize Session Finished");
            
            return true;
        }
        
        public bool fin()
        {
            // clear listeners
            if(_listeners != null)
            {
                for(int i=0; i<(int)ListenerType.LT_Count; ++i)
                {
                    IListener l = _listeners[i];
                    if(l == null)
                    {
                        continue;
                    }

                    l.fin();
                }
                _listeners = null;
            }

            lock(_sessionServiceLock)
            {
                if(_sessionServicePair != null)
                {
                    _sessionServicePair = null;
                }
            }

            lock(_sessionsLock)
            {
                if(_sessionsByID != null)
                {
                    foreach(var item in _sessionsByID.ToArray())
                    {
                        // TO DO : notify close & wait all close
                        item.Value.closing("shut down");
                    }
                }
            }

            // TO DO : wait all session close & clear _sessionsByID

            return true;
        }

        public ISession findSession(ListenerType type, object orgSessionID)
        {
            IListener l = _listeners[(int)type];
            if(l == null)
            {
                Debug.logger.log(LogType.LOG_ERR, "Finding session with not exist listener type ["+type+"]");
                return null;
            }
            
            return l.getSession(orgSessionID);
        }

        public ISession getSessionByID(ulong sessionID)
        {
            lock(_sessionsLock)
            {
                ISession ses = null;
                if(_sessionsByID.TryGetValue(sessionID, out ses))
                {
                    return ses;
                }
            }

            return null;
        }
        

        public IListener getListener(ListenerType type)
        {
            return _listeners[(int)type];
        }
        
        private IService _getSessionService(ISession s)
        {
            IService sv = null;
            lock(_sessionServiceLock)
            {
                _sessionServicePair.TryGetValue(s, out sv);
            }

            return sv;
        }

        public void changeSessionService(ISession s, IService sv)
        {
            lock (_sessionServiceLock)
            {
                // remove & add session inside _sessionServiceLock to prevent queue session task after remove session or before add session

                // remove session from old service
                IService oldSvr = null;
                if(_sessionServicePair.TryGetValue(s, out oldSvr))
                {
                    // start changing service
                    s.isChangingService = true;

                    AESessionRemoveTask rmvTsk = new AESessionRemoveTask(sv, s, "__session", "remove", "changing service");
                    // Notice : very careful with _sessionServiceLock, never lock it inside tasks, or will cause dead lock
                    oldSvr.queueTask(s, rmvTsk); 
                }
                else
                {
                    // no old session direct add session
                    // add session to new service
                    AESessionAddTask addTsk = new AESessionAddTask(s, "__session", "add", "changing service");
                    sv.queueTask(s, addTsk);
                }

                _sessionServicePair[s] = sv;

                // do add session after remove session is finish!!!
                //// add session to new service
                //AESessionAddTask addTsk = new AESessionAddTask(s, "__session", "add", "changing service");
                //sv.queueTask(s, addTsk);
            }
        }
        
        // return : =0 if failed, otherwise return service queued task count
        public ITask queueSessionTask(ISession session, ITask t)
        {
            if(session.isClosed || session.isClosing)
            {
                // session is closing or closed, discard message
                session.lastErrorCode = AEErrorCode.ERR_SESSION_QUEUE_TASK_ERROR;
                session.lastErrorMsg = "AEServer session manager queueSessionTask session[" + AESession.dumpSessionInfo(session) + "] is closing or closed";
                Debug.logger.log(LogType.LOG_ERR, session.lastErrorMsg);
                return null;
            }

            if (session.isChangingService && !(t is AESessionAddTask))
            {
                session.lastErrorCode = AEErrorCode.ERR_SESSION_QUEUE_TASK_ERROR;
                session.lastErrorMsg = "AEServer session manager queueSessionTask session[" + AESession.dumpSessionInfo(session) + "] is changing service";
                Debug.logger.log(LogType.LOG_WARNNING, session.lastErrorMsg); 
                // when session is changing service, only session add task is accept, any other task will be discard
                return null;
            }

            return _queueSessionTask(session, t);
        }

        protected ITask _queueSessionTask(ISession session, ITask t)
        {
            IService svr = _getSessionService(session);
            if(svr == null)
            {
                session.lastErrorCode = AEErrorCode.ERR_SERVICE_NOT_FOUND;
                session.lastErrorMsg = "AEServer session manager queueSessionTask session[" + AESession.dumpSessionInfo(session) + "] without service";
                Debug.logger.log(LogType.LOG_ERR, session.lastErrorMsg);
                return null;
            }

            return svr.queueTask(session, t);
        }

        public ISession newSession(IAssociateSession s)
        {
            // create session
            AESession ses = null;
            lock(_sessionsLock)
            {
                ulong sid = ++_sessionIDSeed;
                ses = new AESession(s, sid);

                _sessionsByID[sid] = ses;
            }
            
            // add to listener
            _listeners[(int)s.type].addSession(ses);

            return ses;
        }
        
        public void addAssociateSession(IAssociateSession ases, ISession s)
        {
            IListener l = _listeners[(int)ases.type];
            if(l == null)
            {
                Debug.logger.log(LogType.LOG_ERR, "add associate session with not exist listener type ["+ases.type+"]");
                return;
            }
            
            s.addAssociateSession(ases);
            l.addSession(s);
        }

        public void onSessionClose(ISession s)
        {
            if(s == null)
            {
                Debug.logger.log(LogType.LOG_ERR, "AEServer session manager on empty session close!");
                return;
            }

            // clear listener session record
            for(int i=0; i<(int)ListenerType.LT_Count; ++i)
            {
                IListener l = _listeners[i];
                if(l == null)
                {
                    continue;
                }

                IAssociateSession ases = s.getAssociateSession((ListenerType)i);
                if(ases == null)
                {
                    continue;
                }

                _listeners[i].removeSession(ases.orgSessionID);
            }

            // call session closed
            ((AESession)s).onClosed();

            // remove session
            lock(_sessionsLock)
            {
                _sessionsByID.Remove(s.sessionID);
            }

        }

        protected void _checkSessionExpire(ulong timestamp)
        {
            Queue<ISession> closingSession = new Queue<ISession>();
            lock(_sessionsLock)
            {
                // TO DO : Split to sevral segment to keep lock time short enough
                // TO DO : to list and check checkSessionExipreCount each time
                foreach(var item in _sessionsByID)
                {
                    // clear expire session
                    ulong idleTime = timestamp - item.Value.lastActiveTime;
                    if(idleTime >= _conf.sessionExpireTime)
                    {
                        item.Value.closing("expire");
                    }

                    if(item.Value.isClosing)
                    {
                        closingSession.Enqueue(item.Value);
                    }
                }
            }

            while(closingSession.Count > 0)
            {
                ISession cs = closingSession.Dequeue();

                // notify service thread session closing, wait service finish closing session
                AESessionCloseTask ct = new AESessionCloseTask(cs, "__session", "close", null);
                _queueSessionTask(cs, ct);
            }
        }
        
        public void tickPorcess(ulong timestamp)
        {
            for(int i=0; i<_listeners.Length; ++i)
            {
                if(_listeners[i] == null)
                {
                    continue;
                }
                _listeners[i].tickPorcess(timestamp);
            }

            ulong checkInterval = timestamp - _lastCheckExpireTime;
            if(checkInterval > _conf.checkSessionExipreInterval)
            {
                _checkSessionExpire(timestamp);
            }
        }
    }
}