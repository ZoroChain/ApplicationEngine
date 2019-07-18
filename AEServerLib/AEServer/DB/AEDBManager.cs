using System.Collections.Concurrent;

namespace AEServer.DB
{
    public class AEDBManager : IDBManager
    {
        ConcurrentDictionary<string, AEDBTable> _memtables = new ConcurrentDictionary<string, AEDBTable>();
        ConcurrentDictionary<string, AEDBTable> _persisttables = new ConcurrentDictionary<string, AEDBTable>();

        public bool init(object config)
        {
            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Start Initialize DBManager ...");

            dynamic conf = config;

            // initialize DB
            DBMySqlManager.manager.init(conf.mysql);
            DBRedisManager.manager.init(conf.redis);

            // initialize tables
            foreach(var item in conf.memtables)
            {
                AEDBTable tab = new AEDBTable();
                if(!tab.init(item))
                {
                    // error
                    continue;
                }

                _memtables[item.name] = tab;
            }

            foreach (var item in conf.persisttables)
            {
                AEDBTable tab = new AEDBTable();
                if (!tab.init(item))
                {
                    // error
                    continue;
                }

                _persisttables[item.name] = tab;
            }

            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Initialize DBManager redis OK!");
            return true;
        }

        public bool fin()
        {
            if(DBMySqlManager.manager != null)
            {
                DBMySqlManager.manager.fin();
                DBMySqlManager.manager = null;
            }
            if(DBRedisManager.manager != null)
            {
                DBRedisManager.manager.fin();
                DBRedisManager.manager = null;
            }

            if (_memtables != null)
            {
                foreach(var item in _memtables)
                {
                    item.Value.fin();
                }
                _memtables = null;
            }
            if (_persisttables != null)
            {
                foreach (var item in _persisttables)
                {
                    item.Value.fin();
                }
                _persisttables = null;
            }
            return true;
        }

        public IDBTable getMemDBTalbe(string name)
        {
            AEDBTable obj = null;
            _memtables.TryGetValue(name, out obj);
            return (IDBTable)obj;
        }

        public IDBTable getPersistDBTable(string name)
        {
            AEDBTable obj = null;
            _persisttables.TryGetValue(name, out obj);
            return (IDBTable)obj;
        }
    }
}