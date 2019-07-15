using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

namespace AEServer
{
    public class ConfigManager : IConfigManager
    {
        protected dynamic _serverConf = new ExpandoObject();

        public bool init()
        {
            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Start Load Configs...");
            // for Debug...

            dynamic conf = _serverConf;

            conf.tickTime = 500; // 500 millisecond tick proc

            conf.DBManagerConf = new ExpandoObject();


            conf.SessionManagerConf = new ExpandoObject();
            conf.SessionManagerConf.sessionExpireTime = 300000; // 300 seconds
            conf.SessionManagerConf.checkSessionExipreInterval = 60000; // 1 min per check
            conf.SessionManagerConf.checkSessionExipreCount = 1000;

            conf.SessionManagerConf.Listener = new ExpandoObject();
            conf.SessionManagerConf.Listener.HTTP = new ExpandoObject();
            //conf.SessionManagerConf.Listener.SignalR = new ExpandoObject();
            //conf.SessionManagerConf.Listener.WebSocket = new ExpandoObject();

            conf.ServiceManagerConf = new ExpandoObject();
            conf.ServiceManagerConf.defaultService = new ExpandoObject();

            dynamic defSvrConf = conf.ServiceManagerConf.defaultService;
            defSvrConf.name = "loginService";
            defSvrConf.Game = new ExpandoObject();
            defSvrConf.Game.name = "outGame";
            defSvrConf.Threading = new ExpandoObject();
            defSvrConf.Threading.threads = new ExpandoObject[4];

            var serviceList = new List<ExpandoObject>();
            
            dynamic webGameSvrConf = new ExpandoObject();
            webGameSvrConf.name = "webGameService";
            webGameSvrConf.Game = new ExpandoObject();
            webGameSvrConf.Game.name = "webGame";
            webGameSvrConf.Threading = new ExpandoObject();
            webGameSvrConf.Threading.threads = new ExpandoObject[4];

            serviceList.Add(webGameSvrConf);
            
            conf.ServiceManagerConf.Service = serviceList;


            // TO DO : initialize configs

            
            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Load Configs Finished");

            return true;
        }

        public object serverConfig
        {
            get
            {
                return _serverConf;
            }
        }

        public int serverConfigVersion
        {
            get
            {
                return _serverConf.version;
            }
        }

        public object refreshConfig(string configName)
        {
            return null;
        }

        public object getConfig(string configName)
        {
            return null;
        }

        public int getConfigVersion(string configName)
        {
            return 0x00000000;
        }
    }
}