using System.Dynamic;
using System.Collections.Generic;

namespace AEServer.DB
{
    public class AEDBObject : IDBObject
    {
        internal DBRedisDB _memDB = null;

        protected IDBTable _table   = null;
        protected string _id        = "";

        internal Dictionary<string, dynamic> _datas            = new Dictionary<string, dynamic>();
        internal Dictionary<string, object> _lastSaveDatas     = new Dictionary<string, object>();

        public AEDBObject(IDBTable table, object memDB, string id, object data, bool isFromDB)
        {
            _table = (IDBTable)table;
            _memDB = (DBRedisDB)memDB;
            _id = id;

            IDictionary<string, object> IDic = (IDictionary<string, object>)data;
            foreach(var item in IDic)
            {
                _datas[item.Key] = item.Value;

                if(isFromDB)
                {
                    // data from database, update last save datas
                    byte[] objBytes = AEDBHelper.serializeObject(item.Value);
                    _lastSaveDatas[item.Key] = objBytes;
                }
            }

        }

        public string id
        {
            get
            {
                return _id;
            }
        }

        virtual public bool isPersistObj
        {
            get
            {
                return false;
            }
        }

        public dynamic getData(string key)
        {
            dynamic ret = null;
            _datas.TryGetValue(key, out ret);
            return ret;
        }

        public bool modifyData(string key, dynamic val)
        {
            _datas[key] = val;

            return true;
        }

        virtual public bool _onFlushData(bool persist, List<KeyValuePair<string, object>> changedObjects)
        {
            bool ret = true;
            foreach (var itm in changedObjects)
            {
                if (!_memDB.setHash(_id, itm.Key, itm.Value))
                {
                    ret = false;
                    continue;
                }

                _lastSaveDatas[itm.Key] = itm.Value;
            }

            return ret;
        }


        public bool flush(bool persist)
        {
            // find changed datas
            List<KeyValuePair<string, object>> changedObjects = new List<KeyValuePair<string, object>>();

            foreach (var ditm in _datas)
            {
                byte[] objBytes = AEDBHelper.serializeObject(ditm.Value);
                object lastSavedBytes = null;

                if (_lastSaveDatas.TryGetValue(ditm.Key, out lastSavedBytes) && AEDBHelper.isSameData(objBytes, (byte[])lastSavedBytes))
                {
                    continue;
                }

                changedObjects.Add(new KeyValuePair<string, object>(ditm.Key, objBytes));

            }

            return _onFlushData(persist, changedObjects);
        }

    }

    public class AEDBPersistObject : AEDBObject
    {
        DBMySqlDB _persistDB = null;

        public AEDBPersistObject(object persistDB, IDBTable table, object memDB, string id, object data, bool isFromDB) : base(table, memDB, id, data, isFromDB)
        {
            _persistDB = (DBMySqlDB)persistDB;
        }

        override public bool _onFlushData(bool persist, List<KeyValuePair<string, object>> changedObjects)
        {
            bool ret = true;
            foreach (var itm in changedObjects)
            {
                if (!_memDB.setHash(_id, itm.Key, itm.Value))
                {
                    ret = false;
                    continue;
                }

                if(!persist)
                {
                    _lastSaveDatas[itm.Key] = itm.Value;
                }
            }

            if(persist)
            {
                // persist
                _persistDB.update(_table.name, _table.keyName, this.id, changedObjects);

                // update last save datas
                foreach (var itm in changedObjects)
                {
                    _lastSaveDatas[itm.Key] = itm.Value;
                }
            }

            return ret;
        }
    }

}