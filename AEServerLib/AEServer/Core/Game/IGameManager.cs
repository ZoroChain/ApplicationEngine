using System;

namespace AEServer.Game
{
    public interface IGameManager
    {
        bool init();

        bool fin();

        void regGame(string name, Func<IGame> creator);

        IGame createGame(string name);
    }
}