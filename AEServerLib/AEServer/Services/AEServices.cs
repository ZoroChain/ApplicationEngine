using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

using AEServer.Session;

namespace AEServer.Service
{
    public class AEServiceBase : IService
    {
        protected AEServiceThreadBase<AEServiceBase>[] _threads = null;

        protected Dictionary<long, ITask> _tasks = new Dictionary<long, ITask>();
        protected object _taskLock = new Object();

        protected Dictionary<ulong, ISession> _sessions = new Dictionary<ulong, ISession>();
        protected object _sessionsLock = new Object();

        protected string _name = "default";
        protected object _gameConf = null;

        public int workerThreadCount
        {
            get
            {
                return _threads.Length;
            }
        }

        public int queuedTaskCount
        {
            get
            {
                lock(_taskLock)
                {
                    return _tasks.Count;
                }
            }
        }

        public int sessionCount
        {
            get
            {
                int ret = 0;
                lock(_sessionsLock)
                {
                    ret = _sessions.Count;
                }

                return ret;
            }
        }
        
        public string name
        {
            get
            {
                return _name;
            }
        }

        public object gameConf
        {
            get
            {
                return _gameConf;
            }
        }

        public bool init(object config)
        {
            dynamic conf = config;
            if(AEHelper.HasProperty(conf, "name"))
            {
                _name = conf.name;
            }

            _gameConf = conf.Game;
            
            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Start Initialize Service["+this.name+"]...");

            if(AEHelper.HasProperty(conf, "Threading"))
            {
                _threads = new AEServiceThreadBase<AEServiceBase>[conf.Threading.threads.Length]; // default 2
                
                for(int i =0; i< conf.Threading.threads.Length; ++i)
                {
                    _threads[i] = new AEServiceThreadBase<AEServiceBase>();
                    _threads[i].init(this, conf.Threading.threads[i], i);
                }
            }
            else
            {
                _threads = new AEServiceThreadBase<AEServiceBase>[2]; // default 2

                // TO DO : init threads by default config
                _threads[0].init(this, null, 0);
                _threads[1].init(this, null, 1);
            }
            
            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Initialize Service["+this.name+"] finished");

            return true;
        }
        
        public bool fin()
        {
            if(_threads != null)
            {
                for(int i=0; i< _threads.Length; ++i)
                {
                    _threads[i].fin();
                }
                _threads = null;
            }

            return true;
        }

        public ISession getSession(ulong sessionid)
        {
            ISession res = null;
            lock(_sessionsLock)
            {
                _sessions.TryGetValue(sessionid, out res);
            }

            return res;
        }
        
        public void addSession(ISession s)
        {
            lock(_sessionsLock)
            {
                _sessions[s.sessionID] = s;
            }
        }

        public void removeSession(ISession s)
        {
            lock(_sessionsLock)
            {
                _sessions.Remove(s.sessionID);
            }
        }

        public ITask getTask(long taskid)
        {
            ITask s = null;
            lock(_taskLock)
            {
                _tasks.TryGetValue(taskid, out s);
            }

            return s;
        }

        protected void _onQueuedTaskFin(ITask t)
        {
            lock(_taskLock)
            {
                // remove task
                _tasks.Remove(t.id);

                // remove task record in session 
                AESessionTask st = (AESessionTask)t;
                ((AESession)st.assocSession).removeTask(st);
            }
        }

        virtual protected int _getThreadIndexBySession(ISession session)
        {
            return (int)(session.sessionID % (ulong)_threads.Length);
        }

        public ITask queueTask(ISession session, ITask t)
        {
            if(!(session is AESession) || !(t is AESessionTask))
            {
                session.lastErrorCode = AEErrorCode.ERR_SESSION_QUEUE_TASK_ERROR;
                session.lastErrorMsg = "AEServer session manager queueSessionTask session[" + AESession.dumpSessionInfo(session) + "] is not AESession or task["+t+"] is not AESessionTask";
                Debug.logger.log(LogType.LOG_WARNNING, session.lastErrorMsg);
                return null;
            }
            
            AESession aeses = (AESession)session;
            AESessionTask aest = (AESessionTask)t;

            // new task
            t.addOnFinishCallback(_onQueuedTaskFin);

            int taskCount = 0;
            // record task
            lock(_taskLock)
            {
                _tasks[t.id] = t;

                taskCount = _tasks.Count;
            }

            // record task in session
            aeses.addTask(aest);

            // dispatch task
            int idx = _getThreadIndexBySession(session);
            _threads[idx].queueTask(t);

            // change task status
            aest.onQueued();

            return t;
        }

        public void tickPorcess(ulong timestamp)
        {
            
        }
    }

    // default service, all new session will bind to default service first
    public class AEDefaultService : AEServiceBase
    {

    }
}
