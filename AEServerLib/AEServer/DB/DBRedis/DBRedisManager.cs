using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using StackExchange.Redis;

namespace AEServer.DB
{
    class DBRedisManager
    {
        protected ConcurrentDictionary<string, DBRedisDB> _dbs = new ConcurrentDictionary<string, DBRedisDB>();

        public bool init(object config)
        {
            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Start Initialize DBManager redis...");

            dynamic conf = config;

            foreach(var item in conf.db)
            {
                DBRedisDB db = new DBRedisDB();
                db.init(item);

                _dbs[item.name] = db;
            }

            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Start Initialize DBManager redis OK");

            return true;
        }

        public bool fin()
        {
            if (_dbs != null)
            {
                foreach(var item in _dbs)
                {
                    item.Value.fin();
                }

                _dbs = null;
            }

            return true;
        }

        public DBRedisDB getDB(string name)
        {
            DBRedisDB db = null;
            _dbs.TryGetValue(name, out db);
            return db;
        }
    }
}
