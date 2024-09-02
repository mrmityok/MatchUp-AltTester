namespace MatchUp.Business
{
    public interface IGameSerializer
    {
        bool SaveProgress();
        bool LoadProgress();
        
        bool SaveGame();
        bool LoadGame();
    }
}