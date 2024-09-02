using System.Linq;
using MatchUp.Business;
using MatchUp.Data;

namespace Presentation
{
    public interface IHomeWindowPresenter
    {
        void Init(IHomeWindowModifier window);
        void OnLevelSelected(int levelId);
        void Save();
        void Load();
    }

    public class HomeWindowPresenter : IHomeWindowPresenter
    {
        private ILevelsData _levelsData;
        private IWindowManager _windowManager;
        private IGameSerializer _gameSerializer;

        private IHomeWindowModifier _window;

        public HomeWindowPresenter(
            ILevelsData levelsData, 
            IWindowManager windowManager,
            IGameSerializer gameSerializer)
        {
            _levelsData = levelsData;
            _windowManager = windowManager;
            _gameSerializer = gameSerializer;
        }
        
        public void Init(IHomeWindowModifier window)
        {
            _window = window;

            _gameSerializer.LoadProgress();
            
            _window.ResetLevelIds(_levelsData.Levels.Select(l => l.Id));
        }

        public void OnLevelSelected(int levelId)
        {
            _windowManager.ShowGame(levelId);
        }

        public void Save()
        {
            _gameSerializer.SaveProgress();
        }

        public void Load()
        {
            _windowManager.Load();
        }
    }
}