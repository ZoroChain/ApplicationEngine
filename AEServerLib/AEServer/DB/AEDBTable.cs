namespace AEServer.DB
{
    class AEDBTable : IDBTable
    {
        protected string _name = "";
        protected string _keyName = "";

        DBRedisDB _memDB = null;

        public string name
        {
            get
            {
                return _name;
            }
        }

        public string keyName
        {
            get
            {
                return _keyName;
            }
        }

        public ulong dataCount
        {
            get
            {
                // TO DO : return data count
                return 0;
            }
        }

        public bool init(object config)
        {
            dynamic conf = config;

            _name = conf.name;
            _memDB = DBRedisManager.manager.getDB(conf.name);
            if(_memDB == null)
            {
                Debug.logger.log(LogType.LOG_ERR, "AEDBMemTable name[" + this.name + "] not exist!");
                return false;
            }

            // TO DO : memtable data never expire

            return true;
        }
        public bool fin()
        {
            if(_memDB != null)
            {
                // _memDB is managed by DB manager, don't release here
                _memDB = null;
            }

            return true;
        }

        public IDBObject insertData(object Data)
        {
            return null;
        }

        public IDBObject getDataObject(ulong id)
        {
            return null;
        }

        public IDBObject queryDBObject(string query)
        {
            return null;
        }

        public object queryData(string query)
        {
            return null;
        }
    }
}