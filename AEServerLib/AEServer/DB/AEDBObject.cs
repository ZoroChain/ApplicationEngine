using System.Collections.Generic;

namespace AEServer.DB
{
    public class AEDBObject : IDBObject
    {
        internal DBRedisDB _memDB = null;

        protected IDBTable _table   = null;
        protected string _id        = "";

        internal Dictionary<string, object> _datas            = new Dictionary<string, object>();
        internal Dictionary<string, object> _lastSaveDatas     = new Dictionary<string, object>();

        public AEDBObject(IDBTable table, object memDB, string id, object data, bool isFromDB)
        {
            _table = (IDBTable)table;
            _memDB = (DBRedisDB)memDB;
            _id = id;

            dynamic d = data;
            for (int i = 0; i < _table.desc.columCount; ++i)
            {
                string keyName = _table.desc.getName(i);
                _datas[keyName] = d[keyName];

                if(isFromDB)
                {
                    // data from database, update last save datas
                    if(_table.desc.getDataType(keyName) == AEDBDataType.ADDT_BINARY)
                    {
                        byte[] objBytes = AEDBHelper.serializeObject(d[keyName]);
                        _lastSaveDatas[keyName] = objBytes;
                    }
                    else
                    {
                        _lastSaveDatas[keyName] = d[keyName];
                    }
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

        public object getData(string key)
        {
            object ret = null;
            _datas.TryGetValue(key, out ret);
            return ret;
        }

        public bool modifyData(string key, object val)
        {
            _datas[key] = val;

            return true;
        }

        virtual public bool _onFlushData(bool persist, List<KeyValuePair<string, object>> changedObjects)
        {
            bool ret = true;
            if(!_memDB.setHashs(_id, changedObjects, _table.desc))
            {
                return false;
            }

            // update last save datas
            foreach (var itm in changedObjects)
            {
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
                if (_table.desc.getDataType(ditm.Key) == AEDBDataType.ADDT_BINARY)
                {
                    byte[] objBytes = AEDBHelper.serializeObject(ditm.Value);
                    object lastSavedBytes = null;

                    if (_lastSaveDatas.TryGetValue(ditm.Key, out lastSavedBytes) && AEDBHelper.isSameData(objBytes, (byte[])lastSavedBytes))
                    {
                        continue;
                    }

                    changedObjects.Add(new KeyValuePair<string, object>(ditm.Key, objBytes));
                }
                else
                {
                    object lastVal;
                    if (_lastSaveDatas.TryGetValue(ditm.Key, out lastVal) && lastVal.Equals(ditm.Value))
                    {
                        continue;
                    }

                    changedObjects.Add(new KeyValuePair<string, object>(ditm.Key, ditm.Value));
                }
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

        override public bool isPersistObj
        {
            get
            {
                return true;
            }
        }

        override public bool _onFlushData(bool persist, List<KeyValuePair<string, object>> changedObjects)
        {
            bool ret = true;

            if (!_memDB.setHashs(_id, changedObjects, _table.desc))
            {
                return false;
            }

            if(persist)
            {
                // persist
                if(!_persistDB.update(_table.desc.name, _table.desc.keyName, this.id, changedObjects, _table.desc))
                {
                    return false;
                }

                // update last save datas
                foreach (var itm in changedObjects)
                {
                    _lastSaveDatas[itm.Key] = itm.Value;
                }
            }
            else
            {
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