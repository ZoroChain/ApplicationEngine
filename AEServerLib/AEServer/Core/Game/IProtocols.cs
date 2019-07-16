namespace AEServer.Game
{
    public interface IRpcProtocol
    {
        bool doCommand(IPlayer p, object sCtx, IGameModule m, dynamic par);
    }

    // TO DO : add protocol manager
}