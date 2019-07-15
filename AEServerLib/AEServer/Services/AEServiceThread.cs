using System;
using System.Collections.Generic;
using System.Threading;

using AEServer.Game;

namespace AEServer.Service
{
    public class AEServiceThreadBase<ServiceT> where ServiceT : class, IService
    {
        protected ServiceT _service = null;

        protected IGame _game = null;

        protected int _serviceThreadIndex = 0;

        protected Thread _workerThread = null;

        protected object _tasksLock = new Object();
        protected Queue<ITask> _tasks = new Queue<ITask>();
        protected AutoResetEvent _taskEvent = new AutoResetEvent(false);
        protected bool _finState = false;
        protected AutoResetEvent _finEvent = new AutoResetEvent(false);

        public int queuedTaskCount
        {
            get
            {
                lock(_tasksLock)
                {
                    return _tasks.Count;
                }
            }
        }

        public bool init(ServiceT s, object config, int index)
        {
            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Starting Service["+s.name+"] index["+index+"] Thread...");
            
            _service = s;
            _serviceThreadIndex = index;
            _workerThread = new Thread(this._wokerThreadProc);
            _workerThread.Start();

            return true;
        }

        public bool fin()
        {
            lock(_tasksLock)
            {
                // notify finish
                _finState = true;
            }
            _taskEvent.Set();
            
            // wait worker thread to finish
            _finEvent.WaitOne();

            // clear to exit

            _service = (ServiceT) null;

            return true;
        }

        public bool queueTask(ITask task)
        {
            lock(_tasksLock)
            {
                // add task
                _tasks.Enqueue(task);
            }

            // notify worker thread
            _taskEvent.Set();

            return true;
        }

        protected void _wokerThreadProc(object data)
        {
            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Enter Service["+_service.name+"] Worker Thread id["+Thread.CurrentThread.ManagedThreadId+"]");

            dynamic gameConf = _service.gameConf;
            _game = AEServerInst.gameManager.createGame(gameConf.name);
            if(_game == null)
            {
                Debug.logger.log(LogType.LOG_ERR, "Service["+_service.name+"] with game name["+gameConf.name+"] create game failed!");
                return;
            }

            _game.onStartup(gameConf, _serviceThreadIndex);

            Queue<ITask> tempTsks = new Queue<ITask>();

            while(true)
            {
                lock(_tasksLock)
                {
                    if(_tasks.Count > 0)
                    {
                        // fetch task
                        while(_tasks.Count > 0)
                        {
                            tempTsks.Enqueue(_tasks.Dequeue());
                        }
                    }
                    else if(_finState)
                    {
                        // finish state is set
                        break;
                    }

                }

                // run task
                while(tempTsks.Count > 0)
                {
                    ITask tsk = tempTsks.Dequeue();
                    try
                    {
                        tsk.run(_service, _game);
                    }
                    catch(Exception e)
                    {
                        Debug.logger.log(LogType.LOG_ERR, "Message["+e.Message+"] Source["+e.Source+"] Stack: " +e.StackTrace);
                        Debug.logger.log(LogType.LOG_ERR, "Service["+_service.name+"] with game name["+_game.name+"] run task["+tsk.id+"] exception catched!");
                    }
                }

                // wait task
                _taskEvent.WaitOne();
            }

            // finState setted, exit

            _game.onShutdown();

            // notify waiting thread finished
            _finEvent.Set();
        }
    }
}