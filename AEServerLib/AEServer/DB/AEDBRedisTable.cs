namespace AEServer.DB
{
    public class AEDBRedisTable : IDBTable<IDBObject>
    {
        protected string _name = "";

        public string name 
        {
            get
            {
                return _name;
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
            return true;
        }

        public IDBObject insertData(object Data)
        {
            return null;
        }

        public IDBObject getData(ulong id)
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