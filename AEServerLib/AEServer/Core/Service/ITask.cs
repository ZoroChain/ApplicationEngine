
using System;
using System.Threading.Tasks;

using AEServer.Game;

namespace AEServer.Service
{
    public enum AETaskStatus
    {
        ATS_NONE = 0,
        ATS_QUEUED = 1,
        ATS_PREPARING = 2,
        ATS_WORKING = 3,
        ATS_PERSISTING = 4,
        ATS_ENDED = 5,
    }
    
    public interface ITask
    {
        long id
        {
            get;
        }

        AETaskStatus status
        {
            get;
        }

        // use a Task to keep compitable with other await/async type concurrent programes
        Task awaitTask
        {
            get;
        }

        void addOnFinishCallback(Action<ITask> cb);

        void run(IService svr, IGame g);
    }
}
