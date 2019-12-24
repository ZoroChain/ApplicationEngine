using System.Collections.Generic;
using AEServer.Session;

namespace AEServer.Game
{
    public class AEGameBase : IGame
    {
        protected Dictionary<ulong, AEPlayer>   _players = new Dictionary<ulong, AEPlayer>();

        protected Dictionary<string, IGameModule> _modules = new Dictionary<string, IGameModule>();

        protected int _serviceThreadIndex = 0;

        virtual public string name
        {
            get
            {
                return "AEGameBase";
            }
        }
        public int serviceThreadIndex
        {
            get
            {
                return _serviceThreadIndex;
            }
        }
        
        public IPlayer getPlayerByID(ulong id)
        {
            AEPlayer p = null;
            _players.TryGetValue(id, out p);
            return p;
        }

        public IGameModule getGameModule(string name)
        {
            IGameModule m;
            _modules.TryGetValue(name, out m);
            return m;
        }

        protected void _regGameModule(string name, IGameModule m)
        {
            _modules[name] = m;
        }

        virtual protected void _onRegisterModule()
        {
            // for sub class command processor register
        }
        virtual protected AEPlayer _newPlayer()
        {
            return new AEPlayer();
        }

        virtual protected bool _onStartup()
        {
            // for sub class start up
            return true;
        }
        virtual protected bool _onShutdown()
        {
            // for sub class shut down
            return true;
        }
        
        public bool onStartup(object conf, int index)
        {
            _serviceThreadIndex = index;

            _onRegisterModule();

            foreach(var item in _modules)
            {
                item.Value.onStartup(this, conf);
            }

            return _onStartup();
        }

        public bool onShutdown()
        {
            foreach(var item in _modules)
            {
                item.Value.onShutdown();
            }

            return _onShutdown();
        }

        public void onAddSession(ISession s, string cls, string method, object par)
        {
            AEPlayer p = _newPlayer();
            p.init(s);

            _players[s.sessionID] = p;
            
            // initialize player
            foreach(var item in _modules)
            {
                item.Value.initPlayer(p);
            }
        }
        public void onRemoveSession(ISession s, string cls, string method, object par)
        {
            AEPlayer p = null;

            if(!_players.TryGetValue(s.sessionID, out p))
            {
                Debug.logger.log(LogType.LOG_ERR, "game ["+this.name+"] onRemoveSession sid ["+s.sessionID+"] player not exist!");
                return;
            }

            // TO DO : clear player
            
            // foreach(var item in _modules)
            // {
            //     item.Value.onRemovePlayer(p);
            // }

            // changing service, don't need persist & clear db data
            p.flushAll(false);
            p.fin(false);

            _players.Remove(s.sessionID);

        }
        public void onSessionClose(ISession s, string cls, string method, object par)
        {
            AEPlayer p = null;

            if(!_players.TryGetValue(s.sessionID, out p))
            {
                Debug.logger.log(LogType.LOG_ERR, "game ["+this.name+"] onSessionClose sid ["+s.sessionID+"] player not exist!");
                return;
            }

            // TO DO : clear player
            
            // foreach(var item in _modules)
            // {
            //     item.Value.onRemovePlayer(p);
            // }

            // session close, need persist & clear db data
            p.flushAll(true);
            p.fin(true);

            _players.Remove(s.sessionID);
        }

        public bool onSessionCmd(ISession s, object sCtx, string cls, string method, object par)
        {
            AEPlayer p = null;

            if(!_players.TryGetValue(s.sessionID, out p))
            {
                s.lastErrorCode = AEErrorCode.ERR_SESSOIN_PLAYER_NOT_EXIST;
                s.lastErrorMsg = "game ["+this.name+"] onSessionCmd sid ["+s.sessionID+"] player not exist!";
                Debug.logger.log(LogType.LOG_ERR, s.lastErrorMsg);
                return false;
            }

            IGameModule m = getGameModule(cls);
            if(m == null)
            {
                if (this.name == "SHOutGame" && cls == "webGame")
                {
                    s.lastErrorCode = AEErrorCode.ERR_SESSION_NEED_LOGIN;
                    s.lastErrorMsg = "need login!";
                }
                else
                {
                    s.lastErrorCode = AEErrorCode.ERR_SYS_SERVER_INTERNAL_ERROR;
                    s.lastErrorMsg = "game [" + this.name + "] onSessionCmd sid [" + s.sessionID + "] game module[" + cls + "] not exist!";
                }
                if (method != "refreshUser" && method != "logout")
                    Debug.logger.log(LogType.LOG_ERR, s.lastErrorMsg);
                return false;
            }

            return m.onPlayerCmd(p, sCtx, method, par);
        }

        virtual public void onTick(ulong timeStamp)
        {
            // update modules
            foreach(var item in _modules)
            {
                item.Value.onTick(timeStamp);
            }

            // update players
            foreach(var item in _players)
            {
                item.Value.onTick(timeStamp);
            }
        }
    }
}