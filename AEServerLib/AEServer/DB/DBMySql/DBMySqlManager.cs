using System.Collections.Concurrent;
using System.Threading;

namespace AEServer.DB
{
    class DBMySqlManager
    {
        public static DBMySqlManager manager = new DBMySqlManager();

        // per thread connections
        protected ConcurrentDictionary<int, ConcurrentDictionary<string, DBMySqlDB> > _dbs = new ConcurrentDictionary<int, ConcurrentDictionary<string, DBMySqlDB> >();
        protected ConcurrentDictionary<string, dynamic> _dbConfs = new ConcurrentDictionary<string, dynamic>();

        public bool init(object config)
        {
            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Start Initialize DBManager mysql...");

            dynamic conf = config;

            foreach (var item in conf.db)
            {
                //DBMySqlDB db = new DBMySqlDB();
                //db.init(item);

                //ConcurrentDictionary<string, DBMySqlDB> dbDic = new ConcurrentDictionary<string, DBMySqlDB>();
                //dbDic.TryAdd(item.name, db);
                //_dbs[_dbs.Count] = dbDic;

                //_dbs[item.name] = db;

                // record conf only, create db when get in thread
                _dbConfs[item.name] = item;
            }

            Debug.logger.log(LogType.LOG_SYSTEM, "AEServer Start Initialize DBManager mysql OK");

            return true;
        }


        public bool fin()
        {
            if (_dbs != null)
            {
                foreach (var item in _dbs)
                {
                    foreach(var dbkvp in item.Value)
                    {
                        dbkvp.Value.fin();
                    }
                }

                _dbs = null;
            }

            return true;
        }

        // create a db instance outside manager, caller is response to maintain the instance
        public DBMySqlDB createDB(string name)
        {
            dynamic dbconf = null;
            DBMySqlDB db = null;

            _dbConfs.TryGetValue(name, dbconf);

            if (dbconf == null)
            {
                // config not exist
                return null;
            }

            db = new DBMySqlDB();
            if (!db.init(dbconf))
            {
                return null;
            }

            return db;
        }

        // get per thread cached db instance by name
        public DBMySqlDB getDB(string name)
        {
            DBMySqlDB db = null;
            ConcurrentDictionary<string, DBMySqlDB> dbs = null;

            // get current thread db instance
            dbs = _dbs.GetOrAdd(Thread.CurrentThread.ManagedThreadId, new ConcurrentDictionary<string, DBMySqlDB>());
            dbs.TryGetValue(name, out db);

            if(db == null)
            {
                dynamic dbconf = null;
                _dbConfs.TryGetValue(name, out dbconf);

                if(dbconf == null)
                {
                    // config not exist
                    return null;
                }

                db = new DBMySqlDB();
                if(!db.init(dbconf))
                {
                    return null;
                }

                dbs[name] = db;
            }

            return db;
        }
    }
}
