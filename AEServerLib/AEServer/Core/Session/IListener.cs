namespace AEServer.Session
{
    public enum ListenerType
    {
        LT_NONE = 0,
        LT_HTTP = 1,
        LT_SIGNALR = 2,
        LT_WEBSOCKET = 3,
        LT_SOCKET_TCP = 4,
        LT_SOCKET_UDP = 5,

        LT_Count
    }

    public interface IListener
    {
        ListenerType type
        {
            get;
        }

        int sessionCount
        {
            get;
        }

        ISessionManager sessionManager
        {
            get;
        }

        bool init(object config);

        bool fin();

        ISession getSession(object orgSessionID);
        
        int addSession(ISession s);

        void removeSession(object orgSessionID);

        // unix timestamp in ms
        void tickPorcess(ulong timestamp);
    }
}
