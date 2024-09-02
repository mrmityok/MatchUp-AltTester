using System.Linq;
using MatchUp.Business;
using MatchUp.Data;

namespace Presentation
{
    internal interface ILevelSelectorViewPresenter
    {
        void Init(ILevelSelectorViewModifier view);

        void SetLevelId(int levelId);

        void Select();
    }
    
    public class LevelSelectorViewPresenter : ILevelSelectorViewPresenter
    {
        private ILevelSelectorViewModifier _view;

        private ILevelsData _levelsData;
        private IPlayer _player;
        
        private int _levelId;

        public LevelSelectorViewPresenter(ILevelsData levelsData, IPlayer player)
        {
            _levelsData = levelsData;
            _player = player;

            _player.BestResultsChanged += OnBestResultsChanged;
        }

        public void Init(ILevelSelectorViewModifier view)
        {
            _view = view;
        }

        private void OnBestResultsChanged()
        {
            UpdateView();
        }

        public void SetLevelId(int levelId)
        {
            _levelId = levelId;
            UpdateView();
        }
        
        private void UpdateView()
        {
                var level = _levelsData.Levels.FirstOrDefault(l => l.Id == _levelId);
            if (level == null)
                return;

            _view.SetLevelName(level.Name);

            if (_player.BestResults.TryGetValue(_levelId, out IGameResult bestResult))
            {
                _view.SetBestResult(bestResult.TimePassed, bestResult.TryCount);
            }
            else
            {
                _view.SetBestResult(null, null);
            }
        }

        public void Select()
        {
            _view.RiseSelected(_levelId);
        }
    }
}