using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AEServer.Session;
using AEServer.Game;

namespace AEServer.Service
{
    public class AETaskBase : ITask
    {
        static private long _taskIDSeed = 0; 

        protected long _taskid;
        protected AETaskStatus _status = AETaskStatus.ATS_NONE;
        protected Task _awaitTask = new Task(() =>
                                {
                                   // Doing nothing, just for inform the awaiter.
                                   // TO DO : compare with TaskCompletionSource<TResult>
                                });

        protected IService _runingService;
        protected IGame _runingGame;
        protected Stack<Action<ITask> > _onFinCbs = new Stack<Action<ITask> >();

        public AETaskBase() 
        {
            // fetech task id
            Interlocked.Increment(ref _taskIDSeed); 
            _taskid = _taskIDSeed;
        }

        public long id
        {
            get
            {
                return _taskid;
            }
        }

        public AETaskStatus status
        {
            get
            {
                return _status;
            }
        }
        
        // use a Task to keep compitable with other await/async type concurrent programes
        public Task awaitTask
        {
            get
            {
                return _awaitTask;
            }
        }

        virtual public void run(IService svr, IGame g)
        {
            _runingService = svr;
            _runingGame = g;
            _prepare();
            _doWork();
            _persist();
            _fin();
        }

        public void addOnFinishCallback(Action<ITask> cb)
        {
            if(!_onFinCbs.Contains(cb))
            {
                _onFinCbs.Push(cb);
            }
        }

        virtual public void onQueued()
        {
            _status = AETaskStatus.ATS_QUEUED;
        }

        virtual protected void _prepare()
        {
            _status = AETaskStatus.ATS_PREPARING;
        }

        virtual protected void _doWork()
        {
            _status = AETaskStatus.ATS_WORKING;
        }

        virtual protected void _persist()
        {
            _status = AETaskStatus.ATS_PERSISTING;
        }

        virtual protected void _fin()
        {
            // notify listener task finish
            while(_onFinCbs.Count > 0)
            {
                Action<ITask> ac = _onFinCbs.Pop();
                ac.Invoke(this);
            }

            // change status
            _status = AETaskStatus.ATS_ENDED;

            if(_awaitTask != null)
            {
                _awaitTask.Start(); // tell the awaiter that I am finish
                _awaitTask = null;
            }
        }
        
    }

    public class AESessionTask : AETaskBase
    {
        protected ISession _session = null;

        protected string _cls = "";
        protected string _method = "";
        protected object _par = null;

        public ISession assocSession
        {
            get
            {
                return _session;
            }
        }

        public AESessionTask(ISession s, string cls, string method, object par) 
        {
            _session = s;
            _cls = cls;
            _method = method;
            _par = par;
        }

        override protected void _fin()
        {
            base._fin();

            // dec ref
            _session = null;
        }
        
    }

    public class AESessionCloseTask : AESessionTask
    {
        public AESessionCloseTask(ISession s, string cls, string method, object par) : base(s, cls, method, par)
        {
        }

        override protected void _doWork()
        {
            base._doWork();

            Debug.logger.log(LogType.LOG_DEBUG, "service: "+_runingService.name+" close session:"+_session.sessionID+" cls:" + _cls +" method:"+_method +" par:"+AEHelper.dumpObject(_par));

            // close session
            _runingGame.onSessionClose(_session, _cls, _method, _par);

            _runingService.removeSession(_session);

            AEServerInst.sessionManager.onSessionClose(_session);
        }
    }

    public class AESessionAddTask : AESessionTask
    {
        public AESessionAddTask(ISession s, string cls, string method, object par) : base(s, cls, method, par)
        {
        }

        override protected void _doWork()
        {
            base._doWork();

            Debug.logger.log(LogType.LOG_DEBUG, "service:"+_runingService.name+" add session:"+_session.sessionID+" cls:" + _cls +" method:"+_method +" par:"+AEHelper.dumpObject(_par));

            // add session
            _runingGame.onAddSession(_session, _cls, _method, _par);
            
            _runingService.addSession(_session);

            // changing service is finish
            _session.isChangingService = false;
        }
    }
    
    public class AESessionRemoveTask : AESessionTask
    {
        protected IService _newSvr = null;

        public AESessionRemoveTask(IService newSvr, ISession s, string cls, string method, object par) : base(s, cls, method, par)
        {
            _newSvr = newSvr;
        }

        override protected void _doWork()
        {
            base._doWork();

            Debug.logger.log(LogType.LOG_DEBUG, "service:"+_runingService.name+" remove session:"+_session.sessionID+" cls:" + _cls +" method:"+_method +" par:"+AEHelper.dumpObject(_par));

            // remove session
            _runingGame.onRemoveSession(_session, _cls, _method, _par);

            _runingService.removeSession(_session);

            // finish remove session

            // add session to new service
            AESessionAddTask addTsk = new AESessionAddTask(_session, "__session", "add", "changing service");
            _newSvr.queueTask(_session, addTsk);
        }
    }

    public class AEHttpSessionTask : AESessionTask
    {
        
        public AEHttpSessionTask(ISession s, string cls, string method, object par) : base(s, cls, method, par)
        {
        }

        override protected void _doWork()
        {
            base._doWork();

            //Debug.logger.log(LogType.LOG_DEBUG, "service: "+_runingService.name+" thread: "+Thread.CurrentThread.ManagedThreadId+"session:"+_session.sessionID+" task cls:" + _cls +" method:"+_method +" par:"+AEHelper.dumpObject(_par));

            
            try
            {
                if(!_runingGame.onSessionCmd(_session, _cls, _method, _par))
                {
                    AEHttpAssociateSession htases = (AEHttpAssociateSession)_session.getAssociateSession(ListenerType.LT_HTTP);
                    htases.writeErrorAsync(_session.lastErrorCode, _session.lastErrorMsg);
                }
            }
            catch(Exception e)
            {
                Debug.logger.log(LogType.LOG_ERR, "Message["+e.Message+"] Source["+e.Source+"] Stack: " +e.StackTrace);
                Debug.logger.log(LogType.LOG_ERR, "Service["+_runingService.name+"] with game name["+_runingGame.name+"] run AEHttpSessionTask cls["+_cls+"] method["+_method+"] exception catched!");

                AEHttpAssociateSession htases = (AEHttpAssociateSession)_session.getAssociateSession(ListenerType.LT_HTTP);
                htases.writeErrorAsync(AEErrorCode.ERR_SYS_SERVER_INTERNAL_ERROR, e.Message);
            }
        }
    }
}
