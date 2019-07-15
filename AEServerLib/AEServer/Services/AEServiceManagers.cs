using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;

namespace AEServer.Service
{
    public class AEServiceManager : IServiceManager
    {
        protected AEDefaultService _defaultService = null;

        protected ConcurrentDictionary<string, IService> _services = new ConcurrentDictionary<string, IService>();

        public bool init(object config)
        {
            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Start Initialize Service Manager...");
            // TO DO : intialize services

            dynamic conf = config;

            _defaultService = new AEDefaultService();

            _defaultService.init(conf.defaultService);

            foreach(var item in conf.Service)
            {
                AEDefaultService svr = new AEDefaultService();
                svr.init(item);

                _services[svr.name] = svr;
            }

            
            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Initialize Service Manager Finished");

            return true;
        }
        
        public bool fin()
        {
            if(_services != null)
            {
                foreach(IService serv in _services.Values)
                {
                    serv.fin();
                }
                _services = null;
            }

            // TO DO : wait all service & task finish

            return true;
        }

        public IService defaultService
        {
            get
            {
                return _defaultService;
            }
        }

        public IService getService(string name)
        {
            IService ret = null;
            _services.TryGetValue(name, out ret);

            return ret;
        }
        
        public void tickProcess(ulong timestamp)
        {
            _defaultService.tickPorcess(timestamp);
            foreach(var item in _services)
            {
                item.Value.tickPorcess(timestamp);
            }
        }
    }

}