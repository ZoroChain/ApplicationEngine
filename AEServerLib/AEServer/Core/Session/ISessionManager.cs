using AEServer.Service;

namespace AEServer.Session
{
    public interface ISessionManager
    {
        bool init(object conf);

        bool fin();

        ISession findSession(ListenerType type, object orgSessionID);

        ISession getSessionByID(ulong sessionID);

        IListener getListener(ListenerType type);

        void changeSessionService(ISession s, IService sv);

        // return : =0 if failed, otherwise return service queued task count
        ITask queueSessionTask(ISession session, ITask t);

        ISession newSession(IAssociateSession s);

        void addAssociateSession(IAssociateSession ases, ISession s);

        void onSessionClose(ISession s);

        // unix timestamp in ms
        void tickPorcess(ulong timestamp);
    }
}