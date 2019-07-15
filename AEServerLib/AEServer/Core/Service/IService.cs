using AEServer.Session;

namespace AEServer.Service
{
    public interface IService
    {
        int workerThreadCount
        {
            get;
        }

        int queuedTaskCount
        {
            get;
        }

        int sessionCount
        {
            get;
        }

        string name
        {
            get;
        }

        object gameConf
        {
            get;
        }

        bool init(object config);

        bool fin();

        ISession getSession(ulong sessionid);

        void addSession(ISession s);

        void removeSession(ISession s);

        ITask getTask(long taskid);

        // return : =0 if failed, otherwise return current queued task count
        ITask queueTask(ISession session, ITask t);

        // unix timestamp in ms
        void tickPorcess(ulong timestamp);
    }
}
