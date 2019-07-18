using System;
using System.Collections.Generic;
using System.Dynamic;

using MySql.Data;
using MySql.Data.MySqlClient;

namespace AEServer.DB
{
    class DBMySqlDB
    {
        protected MySqlConnection _conn = null;
        protected dynamic _conf = null;

        public string name
        {
            get
            {
                return _conf ? _conf.name : "";
            }
        }

        public bool init(object config)
        {
            _conf = config;

            Debug.logger.log(LogType.LOG_SYSTEM, "Initialize mysql db[" + _conf.name + "] source[" + _conf.source + "]");

            _conn = new MySqlConnection(_conf.source + "," + _conf.options);
            bool ret = true;

            try
            {
                _conn.Open();
            }
            catch(Exception e)
            {
                Debug.logger.log(LogType.LOG_ERR, "Message[" + e.Message + "] Source[" + e.Source + "] Stack: " + e.StackTrace);
                Debug.logger.log(LogType.LOG_ERR, "mysql db[" + _conf.name + "] source[" + _conf.source + "] connect failed!");

                ret = false;
            }

            return ret;
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

        public List<object> query(string query)
        {
            if (_conn == null)
            {
                // error: without _conn
                // important: don't log val, or may expose sensitive data!!!
                Debug.logger.log(LogType.LOG_ERR, "mysql db[" + _conf.name + "] source[" + _conf.source + "] query[" + query + "] without connection!");
                return null;
            }

            List<object> ret = new List<object>();
            try
            {
                //string sql = "SELECT Name, HeadOfState FROM Country WHERE Continent='Oceania'";
                MySqlCommand cmd = new MySqlCommand(query, _conn);
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    ExpandoObject obj = new ExpandoObject();
                    IDictionary<string, object> IDic = obj;

                    for(int i=0; i<rdr.FieldCount; ++i)
                    {
                        IDic.Add(rdr.GetName(i), rdr.GetString(i));
                    }

                    ret.Add(obj);
                }
                rdr.Close();
            }
            catch (Exception e)
            {
                Debug.logger.log(LogType.LOG_ERR, "Message[" + e.Message + "] Source[" + e.Source + "] Stack: " + e.StackTrace);
                Debug.logger.log(LogType.LOG_ERR, "mysql db[" + _conf.name + "] source[" + _conf.source + "] query[" + query + "] failed!");
            }

            return ret;
        }

        public bool modify(string query)
        {
            if (_conn == null)
            {
                // error: without _conn
                // important: don't log query, or may expose sensitive data!!!
                Debug.logger.log(LogType.LOG_ERR, "mysql db[" + _conf.name + "] source[" + _conf.source + "] modify without connection!");
                return false;
            }

            bool ret = true;
            try
            {
                //string sql = "INSERT, UPDATE, DELETE";
                MySqlCommand cmd = new MySqlCommand(query, _conn);

                int rowaffect = cmd.ExecuteNonQuery();

                ret = (rowaffect > 0);
            }
            catch (Exception e)
            {
                Debug.logger.log(LogType.LOG_ERR, "Message[" + e.Message + "] Source[" + e.Source + "] Stack: " + e.StackTrace);
                Debug.logger.log(LogType.LOG_ERR, "mysql db[" + _conf.name + "] source[" + _conf.source + "] query[" + query + "] failed!");
            }

            return ret;
        }

        public bool insert(string table, List<KeyValuePair<string, object>> obj)
        {
            if (_conn == null)
            {
                // error: without _conn
                // important: don't log query, or may expose sensitive data!!!
                Debug.logger.log(LogType.LOG_ERR, "mysql db[" + _conf.name + "] source[" + _conf.source + "] insert["+ table + "] without connection!");
                return false;
            }

            bool ret = true;
            try
            {
                //"INSERT INTO testtab(TestCol1,TestCol2) VALUES(@test1,@test2)";
                string sql = "INSERT INTO " + table + "(";
                string sql1 = ") VALUES(";
                int i = 0;
                foreach(var item in obj)
                {
                    if(i>0)
                    {
                        sql += ",";
                        sql1 += ",";
                    }
                    sql += item.Key;
                    sql1 += "@v" + i;
                    ++i;
                }

                sql = sql + sql1 + ");";

                MySqlCommand cmd = new MySqlCommand(sql, _conn);

                i = 0;
                foreach(var item in obj)
                {
                    string pname = "@v" + i;
                    cmd.Parameters.AddWithValue(pname, item.Value);

                    ++i;
                }

                int rowaffect = cmd.ExecuteNonQuery();

                ret = (rowaffect > 0);
            }
            catch (Exception e)
            {
                Debug.logger.log(LogType.LOG_ERR, "Message[" + e.Message + "] Source[" + e.Source + "] Stack: " + e.StackTrace);
                Debug.logger.log(LogType.LOG_ERR, "mysql db[" + _conf.name + "] source[" + _conf.source + "] insert[" + table + "] failed!");
            }

            return ret;
        }

        public bool update(string table, string keyName, string keyVal, List<KeyValuePair<string, object>> obj)
        {
            if (_conn == null)
            {
                // error: without _conn
                // important: don't log query, or may expose sensitive data!!!
                Debug.logger.log(LogType.LOG_ERR, "mysql db[" + _conf.name + "] source[" + _conf.source + "] update[" + table + "] keyname["+keyName+"] without connection!");
                return false;
            }

            bool ret = true;
            try
            {
                //update privatepropsinfo set propsbuffer = '%s' where uin = %llu
                string sql = "UPDATE " + table + " SET ";
                int i = 0;
                foreach (var item in obj)
                {
                    if (i > 0)
                    {
                        sql += ", ";
                    }
                    sql += item.Key + "=@v" + i; 
                    ++i;
                }

                sql = sql + " WHERE " + keyName +"="+ keyVal+";";

                MySqlCommand cmd = new MySqlCommand(sql, _conn);

                i = 0;
                foreach (var item in obj)
                {
                    string pname = "@v" + i;
                    cmd.Parameters.AddWithValue(pname, item.Value);

                    ++i;
                }

                int rowaffect = cmd.ExecuteNonQuery();

                ret = (rowaffect > 0);
            }
            catch (Exception e)
            {
                Debug.logger.log(LogType.LOG_ERR, "Message[" + e.Message + "] Source[" + e.Source + "] Stack: " + e.StackTrace);
                Debug.logger.log(LogType.LOG_ERR, "mysql db[" + _conf.name + "] source[" + _conf.source + "] upate[" + table + "] keyname[" + keyName + "]  failed!");
            }

            return ret;
        }
    }
}
