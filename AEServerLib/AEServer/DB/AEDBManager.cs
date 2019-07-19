using System.Collections.Concurrent;

namespace AEServer.DB
{
    public class AEDBManager : IDBManager
    {
        ConcurrentDictionary<string, AEDBMemTable> _memtables = new ConcurrentDictionary<string, AEDBMemTable>();
        ConcurrentDictionary<string, AEDBPersistTable> _persisttables = new ConcurrentDictionary<string, AEDBPersistTable>();

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
                AEDBMemTable tab = new AEDBMemTable();
                if(!tab.init(item))
                {
                    // error
                    continue;
                }

                _memtables[item.name] = tab;
            }

            foreach (var item in conf.persisttables)
            {
                AEDBPersistTable tab = new AEDBPersistTable();
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
            AEDBMemTable obj = null;
            _memtables.TryGetValue(name, out obj);
            return (IDBTable)obj;
        }

        public IDBTable getPersistDBTable(string name)
        {
            AEDBPersistTable obj = null;
            _persisttables.TryGetValue(name, out obj);
            return (IDBTable)obj;
        }
    }
}