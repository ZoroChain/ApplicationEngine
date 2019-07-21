using System;
using System.Timers;
using AEServer.Service;
using AEServer.DB;
using AEServer.Session;
using AEServer.Game;

namespace AEServer
{
    public class AEServerInst
    {
        private static AEDBManager _dbManger = null;
        private static AESessionManager _sessionManager = null;
        private static AEServiceManager _serviceManager = null;
        private static AEGameManager _gameManager = new AEGameManager();
        private static ConfigManager _confManager = new ConfigManager();
        private static Timer _tickTimer = new Timer();

        public static bool init(object config)
        {
            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Start Initialize...");

            dynamic conf = config;

            _dbManger = new AEDBManager();
            _dbManger.init(conf.DBManagerConf); // TO DO : expand dbManager types

            _sessionManager = new AESessionManager();
            _sessionManager.init(conf.SessionManagerConf);
            
            _gameManager.init();

            _serviceManager = new AEServiceManager();
            _serviceManager.init(conf.ServiceManagerConf);

            // initialize ticker
            _tickTimer.Enabled = true;
            _tickTimer.Interval = conf.tickTime; // ms
            _tickTimer.Elapsed += new ElapsedEventHandler(onTick);

            _tickTimer.Start();

            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Initialize Finished!");

            return true;
        }

        public static bool fin()
        {
            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Start Finalize...");

            _tickTimer.Stop();

            if(_sessionManager != null)
            {
                _sessionManager.fin();
                _sessionManager = null;
            }

            if(_serviceManager != null)
            {
                _serviceManager.fin();
                _serviceManager = null;
            }

            if(_dbManger != null)
            {
                _dbManger.fin();
                _dbManger = null;
            }

            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Finalize Finished!");

            return true;
        }

        private static void onTick(object source, ElapsedEventArgs e)
        {
            ulong timestamp = AEHelper.ToUnixTimeStampMS(DateTime.UtcNow);

            try
            {
                _sessionManager.tickPorcess(timestamp);
            }
            catch(Exception ee)
            {
                Debug.logger.log(LogType.LOG_ERR, "AEServer sessionManager tickProcess exception catched!");
                Debug.logger.log(LogType.LOG_ERR, "Message["+ee.Message+"] Source["+ee.Source+"] Stack: " +ee.StackTrace);
            }
            
            try
            {
                _serviceManager.tickProcess(timestamp);
            }
            catch(Exception ee)
            {
                Debug.logger.log(LogType.LOG_ERR, "AEServer serviceManager tickProcess exception catched!");
                Debug.logger.log(LogType.LOG_ERR, "Message["+ee.Message+"] Source["+ee.Source+"] Stack: " +ee.StackTrace);
            }
        }

        public static IConfigManager ConfigManager
        {
            get
            {
                return _confManager;
            }
        }

        public static IServiceManager serviceManager
        {
            get
            {
                return _serviceManager;
            }
        }

        public static IGameManager gameManager
        {
            get
            {
                return _gameManager;
            }
        }

        public static ISessionManager sessionManager
        {
            get
            {
                return _sessionManager;
            }
        }

        public static IDBManager IDBManager
        {
            get
            {
                return _dbManger;
            }
        }
    }
}
