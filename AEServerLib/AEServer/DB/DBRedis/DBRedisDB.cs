using System;
using System.Collections.Generic;
using System.Text;
using StackExchange.Redis;

namespace AEServer.DB
{
    class DBRedisDB
    {
        protected ConnectionMultiplexer _conn = null;
        protected dynamic _conf = null;

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

            _conn = ConnectionMultiplexer.Connect(_conf.source+","+ _conf.options);
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
            bool ret = db.StringSet(key, val);

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
            byte[] val = db.StringGet("key");

            return val;
        }
    }
}
