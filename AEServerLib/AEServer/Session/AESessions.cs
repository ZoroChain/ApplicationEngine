using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;

using AEServer.Service;

namespace AEServer.Session
{

    public class AESession : ISession
    {
        public AESession(IAssociateSession ias, ulong sid)
        {
            _associateSessions[(int)ias.type] = ias;
            _sessionID = sid;
        } 

        protected IAssociateSession[] _associateSessions = new IAssociateSession[(int)ListenerType.LT_Count];
        protected ulong     _sessionID = 0;
        protected ulong     _lastActiveTime = AEHelper.ToUnixTimeStamp(DateTime.UtcNow);
        protected string    _lastErrorMsg = "";
        protected int       _lastErrorCode = 1;

        // session datas, don't use it to record massive data, simple & temporary data only
        private Dictionary<string, object> _datas = new Dictionary<string, object>();

        private bool _isClosing = false;
        private bool _isClosed = false;
        private bool _isChangingService = false;

        private int _queuedTaskCount = 0;

        public ulong sessionID
        {
            get
            {
                return _sessionID;
            }
        }

        virtual public object orgSessionID
        {
            get
            {
                return null;
            }
        }

        public int queuedTaskCound
        {
            get
            {
                return _queuedTaskCount;
            }
        }
        public ulong lastActiveTime
        {
            get
            {
                return _lastActiveTime;
            }
        }
        
        public bool isClosing
        {
            get
            {
                return _isClosing;
            }
        }

        public bool isClosed
        {
            get
            {
                return _isClosed;
            }
        }
        
        public string lastErrorMsg
        {
            get
            {
                return _lastErrorMsg;
            }
            set
            {
                _lastErrorMsg = value;
            }
        }
        public int lastErrorCode
        {
            get
            {
                return _lastErrorCode;
            }
            set
            {
                _lastErrorCode = value;
            }
        }

        public bool isChangingService
        {
            get
            {
                return _isChangingService;
            }
            set
            {
                _isChangingService = value;
            }
        }

        public bool setSessionData(string key, object data)
        {
            _datas[key] = data;
            return true;
        }

        public object getSessionData(string key)
        {
            if(_datas.ContainsKey(key))
            {
                return _datas[key];
            }
            return null;
        }

        public void addAssociateSession(IAssociateSession s)
        {
            if(s == null)
            {
                Debug.logger.log(LogType.LOG_ERR, "add empty associate session!");
                return;
            }

            _associateSessions[(int)s.type] = s;
        }

        public IAssociateSession getAssociateSession(ListenerType type)
        {
            return _associateSessions[(int)type];
        }

        public bool onClosed()
        {
            _isClosed = true;

            if(_associateSessions != null)
            {
                _associateSessions = null;
            }
            if(_datas != null)
            {
                _datas = null;
            }

            return true;
        }

        public bool closing(string msg)
        {
            // TO DO : close
            _isClosing = true;

            // closing all associate sessions
            for(int i=0; i< _associateSessions.Length; ++i)
            {
                if(_associateSessions[i] == null)
                {
                    continue;
                }

                _associateSessions[i].close(msg);
            }

            // waiting to close

            return true;
        }

        public void notifyError(int errCode, string errMsg)
        {
            // TO DO : notify error with alive connection
        }

        public void addTask(AESessionTask task)
        {
            // record active time
            _lastActiveTime = AEHelper.ToUnixTimeStampMS(DateTime.UtcNow);

            ++_queuedTaskCount;
        }

        public void removeTask(AESessionTask task)
        {
            --_queuedTaskCount;
        }

        public static string dumpSessionInfo(ISession s)
        {
            // TO DO : dump usefull debug info of session

            return "id:"+s.sessionID;
        }
    }

    public class AEHttpAssociateSession : IAssociateSession
    {
        protected string _orgSessoinID = "";

        public AEHttpAssociateSession(string orgSID)
        {
            _orgSessoinID = orgSID;
        }
        
        public object orgSessionID
        {
            get
            {
                return _orgSessoinID;
            }
        }

        public ListenerType type
        {
            get
            {
                return ListenerType.LT_HTTP;
            }
        }

        public bool close(string msg)
        {
            // TO DO : close

            //foreach (var cookie in this._httpCtx.Request.Cookies.Keys)
            //{
            //    this._httpCtx.Response.Cookies.Delete(cookie);
            //}

            return true;
        }

        public void init(HttpContext ctx)
        {
            // TO DO : record infos
        }

        public Task writeErrorAsync(object sCtx, int errCode, string msg)
        {
            return ((HttpContext)sCtx).Response.WriteAsync("{\"res\":"+errCode+", \"msg\":\""+msg+"\"}");
        }

        public Task writeAsync(object sCtx, string data)
        {
            return ((HttpContext)sCtx).Response.WriteAsync(data);
        }

        public object getSessionInfo(object sCtx)
        {
            // TO DO : return remote ip, http header etc.
            return ((HttpContext)sCtx).Connection.RemoteIpAddress.MapToIPv4();
        }
    }
}
