using Zenject;

namespace MatchUp.Business
{
    public class GameSerializerFactory : IFactory<IGameSerializer>
    {
        DiContainer _container;
        
        public GameSerializerFactory(DiContainer container)
        {
            _container = container;
        }
            
        public IGameSerializer Create()
        {
            #if FILE_GAME_SERIALIZER
            
            var s = _container.Instantiate<FileGameSerializer>();
            s.ProgressFilePath = Application.persistentDataPath + "/progress.json";
            s.GameFilePath = Application.persistentDataPath + "/game.json";
            return s;
            
            #else

            var s = _container.Instantiate<PlayerPrefsGameSerializer>();
            s.ProgressKey = "progress";
            s.GameKey = "game";
            return s;
                
            #endif
        }
    }
}