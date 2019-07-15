namespace AEServer
{
    public enum LogType
    {
        LOG_ERR = 1,
        LOG_WARNNING = 2,
        LOG_SYSTEM = 3,
        LOG_DEBUG = 4
    }

    public interface ILogger
    {
        bool setLogLevel(LogType level);

        void log(LogType t, string msg);
    }
}