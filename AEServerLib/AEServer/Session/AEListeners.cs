using System;
using System.Collections;
using System.Collections.Generic;

namespace AEServer.Session
{
    public class AEListenerBase<OrgSessionIDType> : IListener
    {
        
        protected AESessionManager _sessionMgr;

        protected object _sessionsLock = new Object();
        protected Dictionary<OrgSessionIDType, ISession> _sessionsByOrgID = new Dictionary<OrgSessionIDType, ISession>();

        public AEListenerBase(AESessionManager mgr)
        {
            _sessionMgr = mgr;
        }

        virtual public ListenerType type
        {
            get
            {
                return ListenerType.LT_NONE;
            }
        }

        public int sessionCount
        {
            get
            {
                lock(_sessionsLock)
                {
                    return _sessionsByOrgID.Count;
                }
            }
        }
        
        public ISessionManager sessionManager
        {
            get
            {
                return _sessionMgr;
            }
        }

        virtual public bool init(object config)
        {
            return true;
        }

        virtual public bool fin()
        {
            lock(_sessionsLock)
            {
                if(_sessionsByOrgID != null)
                {
                    _sessionsByOrgID = null; // only dec ref, sessions is manage by session manager, not here
                }
            }
            _sessionMgr = null; // dec ref

            return true;
        }

        public ISession getSession(object orgSessionID)
        {
            lock(_sessionsLock)
            {
                ISession ses = null;
                if(_sessionsByOrgID.TryGetValue((OrgSessionIDType)orgSessionID, out ses))
                {
                    return ses;
                }
            }

            return null;
        }

        public int addSession(ISession s)
        {
            IAssociateSession ases = s.getAssociateSession(this.type);
            if(ases == null)
            {
                Debug.logger.log(LogType.LOG_ERR, "AEServer AEListener addSession session["+AESession.dumpSessionInfo(s)+"] without associate session type["+this.type+"]");
                return 0;
            }

            int ret = 0;
            lock(_sessionsLock)
            {
                _sessionsByOrgID[(OrgSessionIDType)ases.orgSessionID] = s;
                ret = _sessionsByOrgID.Count;
            }

            return ret;
        }

        public void removeSession(object orgSessionID)
        {
            lock(_sessionsLock)
            {
                _sessionsByOrgID.Remove((OrgSessionIDType)orgSessionID);
            }
        }

        public void tickPorcess(ulong timestamp)
        {

        }

    }
    public class HttpListener : AEListenerBase<string>
    {
        public HttpListener(AESessionManager mgr) : base(mgr)
        {
        }

        override public ListenerType type
        {
            get
            {
                return ListenerType.LT_HTTP;
            }
        }
        
        override public bool init(object config)
        {
            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Start Initialize HttpListener Manager...");


            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Initialize HttpListener Finished");

            return true;
        }
    }

    public class SignalRListener : AEListenerBase<string>
    {
        public SignalRListener(AESessionManager mgr) : base(mgr)
        {
        }

        override public ListenerType type
        {
            get
            {
                return ListenerType.LT_SIGNALR;
            }
        }
        
        override public bool init(object config)
        {
            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Start Initialize SignalRListener Manager...");


            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Initialize SignalRListener Finished");
            
            return true;
        }

    }

    public class WebSocketListener : AEListenerBase<int>
    {
        public WebSocketListener(AESessionManager mgr) : base(mgr)
        {
        }

        override public ListenerType type
        {
            get
            {
                return ListenerType.LT_WEBSOCKET;
            }
        }
        
        override public bool init(object config)
        {
            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Start Initialize WebSocketListener Manager...");


            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Initialize WebSocketListener Finished");
            
            return true;
        }

    }
}
