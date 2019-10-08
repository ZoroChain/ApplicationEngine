using System;
using System.Collections.Generic;

namespace AEServer.Game
{
    public class AEGameModuleBase : IGameModule
    {
        protected IGame _game = null;
        protected Dictionary<string, Func<IRpcProtocol> > _protoCreators = new Dictionary<string, Func<IRpcProtocol> >();

        virtual public string name
        {
            get
            {
                return "baseModule";
            }
        }

        virtual public IGame game
        {
            get
            {
                return _game;
            }
        }

        protected void _regProtocol(string name, Func<IRpcProtocol> crt)
        {
            _protoCreators[name] = crt;
        }

        virtual public bool onStartup(IGame game, object conf)
        {
            _game = game;

            return true;
        }

        virtual public bool onShutdown()
        {
            if(_game != null)
            {
                _game = null; // dec ref
            }
            return true;
        }

        virtual public void initPlayer(IPlayer p)
        {

        }

        virtual public bool onPlayerCmd(IPlayer p, object sCtx, string cmd, object par)
        {
            Func<IRpcProtocol> protoCrt = null;

            if(!_protoCreators.TryGetValue(cmd, out protoCrt))
            {
                p.session.lastErrorCode = AEErrorCode.ERR_SYS_SERVER_INTERNAL_ERROR;
                p.session.lastErrorMsg = "onPlayerCmd sid["+p.session.sessionID+"] game module["+this.name+"] cmd["+cmd+"] protocol not found!";
                Debug.logger.log(LogType.LOG_ERR, p.session.lastErrorMsg);
                return false;
            }

            IRpcProtocol proto = protoCrt();
            // if(!proto.fromDynamicObject(par))
            // {
            //     p.session.lastErrorCode = AEErrorCode.ERR_SYS_SERVER_INTERNAL_ERROR;
            //     p.session.lastErrorMsg = "onPlayerCmd sid["+p.session.sessionID+"] game module["+this.name+"] cmd["+cmd+"] protocol from par["+AEHelper.dumpObject(par)+"] failed!";
            //     Debug.logger.log(LogType.LOG_ERR, p.session.lastErrorMsg);
            //     return false;
            // }

            return proto.doCommand(p, sCtx, this, par);
        }

        virtual public void onTick(ulong timeStamp)
        {

        }
    }
}