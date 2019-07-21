using System.Collections.Generic;
using System.Dynamic;

namespace AEServer.DB
{
    public class AEDBTableDesc : IDBTableDesc
    {
        protected string _name = "";
        protected string _keyName = "";
        protected List<string> _colNames = new List<string>();
        protected Dictionary<string, AEDBDataType> _colums = new Dictionary<string, AEDBDataType>();

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

        public int columCount
        {
            get
            {
                return _colNames.Count;
            }
        }

        public bool initFromConf(object config)
        {
            dynamic conf = config;

            _name = conf.name;
            _keyName = conf.keyName;

            foreach(var item in conf.colum)
            {
                _colNames.Add(item.Key);
                switch (item.Value)
                {
                    case "int":
                        _colums[item.Key] = AEDBDataType.ADDT_INT;
                        break;
                    case "uint":
                        _colums[item.Key] = AEDBDataType.ADDT_UINT;
                        break;
                    case "float":
                        _colums[item.Key] = AEDBDataType.ADDT_FLOAT;
                        break;
                    case "long":
                        _colums[item.Key] = AEDBDataType.ADDT_LONG;
                        break;
                    case "ulong":
                        _colums[item.Key] = AEDBDataType.ADDT_ULONG;
                        break;
                    case "double":
                        _colums[item.Key] = AEDBDataType.ADDT_DOUBLE;
                        break;
                    case "string":
                        _colums[item.Key] = AEDBDataType.ADDT_STRING;
                        break;
                    case "binary":
                        _colums[item.Key] = AEDBDataType.ADDT_BINARY;
                        break;
                    default:
                        _colums[item.Key] = AEDBDataType.ADDT_None;
                        break;
                }
            }

            return true;
        }

        public string getName(int colIndex)
        {
            return _colNames[colIndex];
        }

        public AEDBDataType getDataType(string colName)
        {
            AEDBDataType ret = AEDBDataType.ADDT_None;
            _colums.TryGetValue(colName, out ret);
            return ret;
        }
    }

    class AEDBMemTable : IDBTable
    {
        protected AEDBTableDesc   _desc = new AEDBTableDesc();
        protected DBRedisDB       _memDB = null;

        public ulong dataCount
        {
            get
            {
                // TO DO : return data count
                return 0;
            }
        }

        public IDBTableDesc desc
        {
            get
            {
                return _desc;
            }
        }

        virtual public bool init(object config)
        {
            dynamic conf = config;

            _desc.initFromConf(conf);
            _memDB = DBRedisManager.manager.getDB(conf.memDBName);
            if(_memDB == null)
            {
                Debug.logger.log(LogType.LOG_ERR, "AEDBMemTable name[" + this.desc.name + "] not exist!");
                return false;
            }

            // TO DO : memtable data never expire

            return true;
        }

        virtual public bool fin()
        {
            if(_memDB != null)
            {
                // _memDB is managed by DB manager, don't release here
                _memDB = null;
            }

            return true;
        }

        virtual protected IDBObject _onInsertData(List<KeyValuePair<string, object>> pars, object key, object Data)
        {
            if (!_memDB.setHashs(key.ToString(), pars, _desc))
            {
                Debug.logger.log(LogType.LOG_ERR, "AEDBMemTable name[" + this.desc.name + "] insert data keyName[" + _desc.keyName + "] keyVal[" + key + "] sethashs failed!");
                return null;
            }

            // create AEDBObject
            AEDBObject obj = new AEDBObject(this, _memDB, key.ToString(), Data, true);

            return obj;
        }

        public IDBObject insertData(object Data)
        {
            dynamic d = Data;

            if(!AEHelper.HasProperty(d, _desc.keyName))
            {
                Debug.logger.log(LogType.LOG_ERR, "AEDBMemTable name[" + this.desc.name + "] insert data keyName["+ _desc.keyName + "] not exist!");
                return null;
            }

            List<KeyValuePair<string, object>> pars = new List<KeyValuePair<string, object>>();
            for(int i=0; i< _desc.columCount; ++i)
            {
                string keyName = _desc.getName(i);
                if(!AEHelper.HasProperty(d, keyName))
                {
                    Debug.logger.log(LogType.LOG_ERR, "AEDBMemTable name[" + this.desc.name + "] insert data keyName[" + keyName + "] not exist!");
                    return null;
                }

                pars.Add(new KeyValuePair<string, object>(keyName, d[keyName]));
            }

            return _onInsertData(pars, d[_desc.keyName], Data);
        }


        virtual public IDBObject getDataObject(string id)
        {
            dynamic objdatas = _memDB.getHashs(id, this.desc);
            if(objdatas == null)
            {
                return null;
            }

            if (!AEHelper.HasProperty(objdatas, _desc.keyName))
            {
                Debug.logger.log(LogType.LOG_ERR, "AEDBMemTable name[" + this.desc.name + "] getDataObject id["+id+"] keyName[" + _desc.keyName + "] not exist!");
                return null;
            }

            AEDBObject obj = new AEDBObject(this, _memDB, objdatas[_desc.keyName], objdatas, true);

            return obj;
        }

        virtual public IDBObject queryDBObject(string query)
        {
            // only persist table can query
            return null;
        }

        virtual public object queryData(string query)
        {
            // only persist table can query
            return null;
        }
    }

    class AEDBPersistTable : AEDBMemTable
    {
        protected DBMySqlDB[] _mysqlDB = null; 

        override public bool init(object config)
        {
            if(!base.init(config))
            {
                return false;
            }

            dynamic conf = config;
            int dbcount = 1;

            if(AEHelper.HasProperty(conf, "distDBCount"))
            {
                dbcount = conf.distDBCount;
            }

            _mysqlDB = new DBMySqlDB[dbcount];

            if(_mysqlDB.Length > 1)
            {
                for (int i = 0; i < _mysqlDB.Length; ++i)
                {
                    _mysqlDB[i] = DBMySqlManager.manager.getDB(conf.persistDBName.ToString() + i);
                    if (_mysqlDB == null)
                    {
                        Debug.logger.log(LogType.LOG_ERR, "AEDBPersistTable name[" + this.desc.name + "] persist db["+i+"] not exist!");
                        return false;
                    }
                }
            }
            else
            {
                _mysqlDB[0] = DBMySqlManager.manager.getDB(conf.persistDBName);
                if (_mysqlDB[0] == null)
                {
                    Debug.logger.log(LogType.LOG_ERR, "AEDBPersistTable name[" + this.desc.name + "] persist db not exist!");
                    return false;
                }
            }

            return true;
        }

        override public bool fin()
        {
            if (_mysqlDB != null)
            {
                // _mysqlDB is managed by DB manager, don't release here
                _mysqlDB = null;
            }

            return true;
        }

        protected uint _getDBIndexByKey(object key)
        {
            // TO DO : calculate db index by key, support distribute db storage
            return 0;
        }

        override protected IDBObject _onInsertData(List<KeyValuePair<string, object>> pars, object key, object Data)
        {
            if (!_memDB.setHashs(key.ToString(), pars, _desc))
            {
                Debug.logger.log(LogType.LOG_ERR, "AEDBPersistTable name[" + this.desc.name + "] insert data keyName[" + _desc.keyName + "] keyVal[" + key + "] sethashs failed!");
                return null;
            }

            uint dbindex = _getDBIndexByKey(key);

            if (!_mysqlDB[dbindex].insert(_desc.name, pars, _desc))
            {
                Debug.logger.log(LogType.LOG_ERR, "AEDBPersistTable name[" + this.desc.name + "] insert data keyName[" + _desc.keyName + "] keyVal[" + key + "] to mysql failed!");
                return null;
            }

            // create AEDBPersistObject
            AEDBPersistObject obj = new AEDBPersistObject(_mysqlDB, this, _memDB, key.ToString(), Data, true);

            return obj;
        }

        override public IDBObject getDataObject(string id)
        {
            AEDBPersistObject obj = null;

            dynamic objdatas = _memDB.getHashs(id, this.desc);
            if (objdatas != null && AEHelper.HasProperty(objdatas, _desc.keyName))
            {
                // get from cache
                // create AEDBPersistObject
                obj = new AEDBPersistObject(_mysqlDB, this, _memDB, objdatas[_desc.keyName], objdatas, true);

                return obj;
            }

            uint dbindex = _getDBIndexByKey(id);

            objdatas =  _mysqlDB[dbindex].getByKey(id, _desc);

            if (!AEHelper.HasProperty(objdatas, _desc.keyName))
            {
                Debug.logger.log(LogType.LOG_ERR, "AEDBPersistTable name[" + this.desc.name + "] getDataObject id[" + id + "] keyName[" + _desc.keyName + "] not exist!");
                return null;
            }

            // cache data object
            List<KeyValuePair<string, object>> pars = new List<KeyValuePair<string, object>>();
            for (int i = 0; i < _desc.columCount; ++i)
            {
                string keyName = _desc.getName(i);
                if (!AEHelper.HasProperty(objdatas, keyName))
                {
                    Debug.logger.log(LogType.LOG_ERR, "AEDBPersistTable name[" + this.desc.name + "] update cache data keyName[" + keyName + "] not exist!");
                    continue;
                }

                pars.Add(new KeyValuePair<string, object>(keyName, objdatas[keyName]));
            }
            if (!_memDB.setHashs(objdatas[_desc.keyName].ToString(), pars, _desc))
            {
                // cache failed!
                Debug.logger.log(LogType.LOG_ERR, "AEDBPersistTable name[" + this.desc.name + "] update cache data keyName[" + _desc.keyName + "] keyVal[" + objdatas[_desc.keyName] + "] sethashs failed!");
            }

            obj = new AEDBPersistObject(_mysqlDB, this, _memDB, objdatas[_desc.keyName], objdatas, true);

            return obj;
        }

        override public IDBObject queryDBObject(string query)
        {
            // TO DO : persist table can query
            return null;
        }

        override public object queryData(string query)
        {
            // TO DO : persist table can query
            return null;
        }
    }
}