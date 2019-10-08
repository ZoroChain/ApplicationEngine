using System;
using System.Collections.Generic;
using StackExchange.Redis;
using System.Dynamic;

namespace AEServer.DB
{
    class DBRedisDB
    {
        protected ConnectionMultiplexer _conn = null;
        protected dynamic _conf = null;
        protected int _expireTime = -1;

        public string name
        {
            get
            {
                return _conf?_conf.name:"";
            }
        }

        public bool init(object config)
        {
            _conf = config;

            if(AEHelper.HasProperty(_conf, "keyExpireTime"))
            {
                _expireTime = _conf.keyExpireTime;
            }

            _conn = ConnectionMultiplexer.Connect(_conf.source + "," + _conf.options);
            if(_conn == null || !_conn.IsConnected)
            {
                Debug.logger.log(LogType.LOG_ERR, "Initialize redis db[" + _conf.name + "] source[" + _conf.source + "] failed!");
                return false;
            }

            // TO DO : handle conn failed restored etc...
            //_conn.ConnectionFailed
            //_conn.ConnectionRestored

            // TO DO : handle error
            //_conn.ErrorMessage = ;
            //_conn.InternalError = ;

            Debug.logger.log(LogType.LOG_SYSTEM, "Initialize redis db[" + _conf.name + "] source[" + _conf.source + "] ok");
            return true;
        }

        public bool fin()
        {
            // TO DO : fin
            if(_conn != null)
            {
                _conn.Close();
                _conn = null;
            }
            return true;
        }

        public bool setValue(string key, byte[] val)
        {
            if(_conn == null)
            {
                // error: without _conn
                // important: don't log val, or may expose sensitive data!!!
                Debug.logger.log(LogType.LOG_ERR, "DBRedisDB name["+this.name+"] setValue key["+key+"] without connection!");
                return false;
            }

            IDatabase db = _conn.GetDatabase();
            bool ret = true;

            if(_expireTime > 0)
            {
                db.StringSet(key, val, TimeSpan.FromSeconds(_expireTime));
            }
            else
            {
                db.StringSet(key, val);
            }

            if (!ret)
            {
                // error: without _conn
                // important: don't log val, or may expose sensitive data!!!
                Debug.logger.log(LogType.LOG_ERR, "DBRedisDB name[" + this.name + "] setValue key[" + key + "] failed!");
                return false;
            }

            return ret;
        }

        public byte[] getValue(string key)
        {
            if (_conn == null)
            {
                // error: without _conn
                Debug.logger.log(LogType.LOG_ERR, "DBRedisDB name[" + this.name + "] getValue key[" + key + "] without connection");
                return null;
            }

            IDatabase db = _conn.GetDatabase();
            byte[] val = db.StringGet(key);
            if (_expireTime > 0)
            {
                // update expire time
                db.KeyExpire(key, TimeSpan.FromSeconds(_expireTime));
            }

            return val;
        }

        public bool setHashs(string hash, List<KeyValuePair<string, object>> par, IDBTableDesc desc)
        {
            if (_conn == null)
            {
                // error: without _conn
                // important: don't log par, or may expose sensitive data!!!
                Debug.logger.log(LogType.LOG_ERR, "DBRedisDB name[" + this.name + "] setHashs hash[" + hash + "] without connection!");
                return false;
            }

            IDatabase db = _conn.GetDatabase();
            bool ret = true;

            HashEntry[] hashs = new HashEntry[par.Count];
            for(int i = 0; i< par.Count; ++i)
            {
                AEDBDataType dt = desc.getDataType(par[i].Key);
                switch (dt)
                {
                    case AEDBDataType.ADDT_INT:
                        hashs[i] = new HashEntry(par[i].Key, (int)par[i].Value);
                        break;
                    case AEDBDataType.ADDT_UINT:
                        hashs[i] = new HashEntry(par[i].Key, (uint)par[i].Value);
                        break;
                    case AEDBDataType.ADDT_FLOAT:
                        hashs[i] = new HashEntry(par[i].Key, (float)par[i].Value);
                        break;
                    case AEDBDataType.ADDT_LONG:
                        hashs[i] = new HashEntry(par[i].Key, (long)par[i].Value);
                        break;
                    case AEDBDataType.ADDT_ULONG:
                        hashs[i] = new HashEntry(par[i].Key, (ulong)par[i].Value);
                        break;
                    case AEDBDataType.ADDT_DOUBLE:
                        hashs[i] = new HashEntry(par[i].Key, (double)par[i].Value);
                        break;
                    case AEDBDataType.ADDT_BINARY:
                        {
                            byte[] objBytes = AEDBHelper.serializeObject(par[i].Value);
                            hashs[i] = new HashEntry(par[i].Key, objBytes);
                        }
                        break;
                    //case AEDBDataType.ADDT_STRING: // deault is string
                    default:
                        hashs[i] = new HashEntry(par[i].Key, (string)par[i].Value);
                        break;
                }
            }

            if (_expireTime > 0)
            {
                db.HashSet(hash, hashs);
                db.KeyExpire(hash, TimeSpan.FromSeconds(_expireTime));
            }
            else
            {
                db.HashSet(hash, hashs);
            }

            return ret;
        }

        public object getHashs(string hash, IDBTableDesc desc)
        {
            if (_conn == null)
            {
                // error: without _conn
                Debug.logger.log(LogType.LOG_ERR, "DBRedisDB name[" + this.name + "] getHashs hash[" + hash + "] without connection!");
                return null;
            }

            IDatabase db = _conn.GetDatabase();
            ExpandoObject ret = new ExpandoObject();
            IDictionary<string, object> IRet = (IDictionary<string, object>)ret;

            HashEntry[] hashs = db.HashGetAll(hash);
            for(int i=0; i< hashs.Length; ++i)
            {
                AEDBDataType dt = desc.getDataType(hashs[i].Name);
                switch (dt)
                {
                    case AEDBDataType.ADDT_INT:
                        IRet[hashs[i].Name] = (int)hashs[i].Value;
                        break;
                    case AEDBDataType.ADDT_UINT:
                        IRet[hashs[i].Name] = (uint)hashs[i].Value;
                        break;
                    case AEDBDataType.ADDT_FLOAT:
                        IRet[hashs[i].Name] = (float)hashs[i].Value;
                        break;
                    case AEDBDataType.ADDT_LONG:
                        IRet[hashs[i].Name] = (long)hashs[i].Value;
                        break;
                    case AEDBDataType.ADDT_ULONG:
                        IRet[hashs[i].Name] = (ulong)hashs[i].Value;
                        break;
                    case AEDBDataType.ADDT_DOUBLE:
                        IRet[hashs[i].Name] = (double)hashs[i].Value;
                        break;
                    case AEDBDataType.ADDT_BINARY:
                        {
                            object o = AEDBHelper.unserializeObject(hashs[i].Value);
                            IRet[hashs[i].Name]= o;
                        }
                        break;
                    //case AEDBDataType.ADDT_STRING: // deault is string
                    default:
                        IRet[hashs[i].Name] = (string)hashs[i].Value;
                        break;
                }
            }

            if (_expireTime > 0)
            {
                // update expire time
                db.KeyExpire(hash, TimeSpan.FromSeconds(_expireTime));
            }

            return ret;
        }

        public bool setHash(string hash, string field, object val, IDBTableDesc desc)
        {
            if (_conn == null)
            {
                // error: without _conn
                // important: don't log val, or may expose sensitive data!!!
                Debug.logger.log(LogType.LOG_ERR, "DBRedisDB name[" + this.name + "] setHash hash[" + hash + "] field[" + field + "] without connection!");
                return false;
            }

            IDatabase db = _conn.GetDatabase();
            bool ret = true;
            AEDBDataType dt = desc.getDataType(field);
            switch(dt)
            {
                case AEDBDataType.ADDT_INT:
                    ret = db.HashSet(hash, field, (int)val);
                    break;
                case AEDBDataType.ADDT_UINT:
                    ret = db.HashSet(hash, field, (uint)val);
                    break;
                case AEDBDataType.ADDT_FLOAT:
                    ret = db.HashSet(hash, field, (float)val);
                    break;
                case AEDBDataType.ADDT_LONG:
                    ret = db.HashSet(hash, field, (long)val);
                    break;
                case AEDBDataType.ADDT_ULONG:
                    ret = db.HashSet(hash, field, (ulong)val);
                    break;
                case AEDBDataType.ADDT_DOUBLE:
                    ret = db.HashSet(hash, field, (double)val);
                    break;
                case AEDBDataType.ADDT_BINARY:
                    {
                        byte[] objBytes = AEDBHelper.serializeObject(val);
                        ret = db.HashSet(hash, field, objBytes);
                    }
                    break;
                //case AEDBDataType.ADDT_STRING: // deault is string
                default:
                    ret = db.HashSet(hash, field, (string)val);
                    break;
            }

            if (!ret)
            {
                // error: without _conn
                // important: don't log val, or may expose sensitive data!!!
                Debug.logger.log(LogType.LOG_ERR, "DBRedisDB name[" + this.name + "] setHash hash[" + hash + "] field[" + field + "] failed!");
                return false;
            }

            if (_expireTime > 0)
            {
                // update expire time
                db.KeyExpire(hash, TimeSpan.FromSeconds(_expireTime));
            }

            return ret;
        }

        public object getHash(string hash, string field, IDBTableDesc desc)
        {
            if (_conn == null)
            {
                // error: without _conn
                Debug.logger.log(LogType.LOG_ERR, "DBRedisDB name[" + this.name + "] getHash hash[" + hash + "] field[" + field + "] without connection");
                return null;
            }

            IDatabase db = _conn.GetDatabase();
            object val;

            AEDBDataType dt = desc.getDataType(field);
            switch (dt)
            {
                case AEDBDataType.ADDT_INT:
                    val = (int)db.HashGet(hash, field);
                    break;
                case AEDBDataType.ADDT_UINT:
                    val = (uint)db.HashGet(hash, field);
                    break;
                case AEDBDataType.ADDT_FLOAT:
                    val = (float)db.HashGet(hash, field);
                    break;
                case AEDBDataType.ADDT_LONG:
                    val = (long)db.HashGet(hash, field);
                    break;
                case AEDBDataType.ADDT_ULONG:
                    val = (ulong)db.HashGet(hash, field);
                    break;
                case AEDBDataType.ADDT_DOUBLE:
                    val = (double)db.HashGet(hash, field);
                    break;
                case AEDBDataType.ADDT_BINARY:
                    {
                        val = AEDBHelper.unserializeObject(db.HashGet(hash, field));
                    }
                    break;
                //case AEDBDataType.ADDT_STRING: // deault is string
                default:
                    val = (string)db.HashGet(hash, field);
                    break;
            }

            if (_expireTime > 0)
            {
                // update expire time
                db.KeyExpire(hash, TimeSpan.FromSeconds(_expireTime));
            }

            return val;
        }


    }
}
