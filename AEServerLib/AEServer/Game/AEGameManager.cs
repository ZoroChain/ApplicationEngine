using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace AEServer.Game
{
    class AEGameManager : IGameManager
    {
        protected ConcurrentDictionary<string, Func<IGame> > _gameCreators = new ConcurrentDictionary<string, Func<IGame>>();

        public bool init()
        {
            return true;
        }

        public bool fin()
        {
            return true;
        }

        public void regGame(string name, Func<IGame> creator)
        {
            _gameCreators[name] = creator;
        }

        public IGame createGame(string name)
        {
            Func<IGame> crt = null;
            if(_gameCreators.TryGetValue(name, out crt))
            {
                return crt.Invoke();
            }

            return null;
        }
    }
}