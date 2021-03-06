namespace AEServer.Game
{
    public interface IGameModule
    {
        string name
        {
            get;
        }

        IGame game
        {
            get;
        }

        bool onStartup(IGame game, object conf);

        bool onShutdown();

        // Add & remove player don't need in module
        // void onAddPlayer(IPlayer p);
        // void onRemovePlayer(IPlayer p);

        void initPlayer(IPlayer p);

        bool onPlayerCmd(IPlayer p, object sCtx, string cmd, object par);

        void onTick(ulong timeStamp);
    }
}