using System;

namespace AEServer
{
    class _LoggerHelper
    {
        static public string formatLogMessage(LogType t, string msg)
        {
            string retMsg;

            switch(t)
            {
            case LogType.LOG_ERR:
                {
                    // TO DO : dump callstack
                    retMsg = "[ERROR]: ";
                }
                break;
            case LogType.LOG_WARNNING:
                {
                    // TO DO : dump callstack
                    retMsg = "[WARNNING]: ";
                }
                break;
            case LogType.LOG_SYSTEM:
                {
                    retMsg = "[SYSTEM]: ";
                }
                break;
            case LogType.LOG_DEBUG:
                {
                    retMsg = "[DEBUG]: ";
                }
                break;
            default:
                {
                    retMsg = "[NONE]: ";
                }
                break;
            }

            return retMsg + msg;
        }

    }

    public class AEConsoleLogger : ILogger
    {
        public AEConsoleLogger()
        {
            Debug.regLogger(this);
        }
        
        private LogType _logLevel = LogType.LOG_SYSTEM;

        public bool setLogLevel(LogType level)
        {
            _logLevel = level;
            return true;
        }

        public void log(LogType t, string msg)
        {
            if(t > _logLevel)
            {
                // no need log
                return;
            }

            Console.WriteLine($"[{DateTime.Now.TimeOfDay:hh\\:mm\\:ss\\.fff}]"+_LoggerHelper.formatLogMessage(t, msg));
        }
    }
}