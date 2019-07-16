namespace AEServer.Session
{
    public interface IAssociateSession
    {
        object orgSessionID
        {
            get;
        }

        ListenerType type
        {
            get;
        }

        bool close(string msg);
    }

    public interface ISession
    {
        // AEServer session id of this session
        ulong sessionID
        {
            get;
        }

        // the number of tasks that already queued in associate service
        int queuedTaskCound
        {
            get;
        }

        // last active time, unix time stamp in ms repesent last time receive message from this session
        ulong lastActiveTime
        {
            get;
        }

        bool isClosing
        {
            get;
        }

        bool isClosed
        {
            get;
        }

        string lastErrorMsg
        {
            get;
            set;
        }
        int lastErrorCode
        {
            get;
            set;
        }

        bool isChangingService
        {
            get;
            set;
        }

        void addAssociateSession(IAssociateSession s);

        IAssociateSession getAssociateSession(ListenerType type);

        bool setSessionData(string key, object data);

        object getSessionData(string key);

        bool closing(string msg);

        void notifyError(int errCode, string errMsg);
    }
}
