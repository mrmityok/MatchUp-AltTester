using MatchUp.Business;
using TMPro;
using UnityEngine;
using Zenject;

namespace Presentation
{
    public interface IGameWindowModifier
    {
        void SetTryCount(int count);
        void SetMatchCount(int count);
        void SetTimeout(float time);
        void StartTimeout(float time);
        void StopTimeout();
        void ClearCards();
        void SetCards(ICard[] gameManagerCards, int boardWidth, int boardHeight);
    }
    
    public class GameWindow : Window<int>
    {
        [SerializeField] private TimeoutView timeoutView = null;
        [SerializeField] private TextMeshProUGUI tryCountText = null;
        [SerializeField] private TextMeshProUGUI matchCountText = null;
        [SerializeField] private GameBoard gameBoard = null;
        
        private IGameWindowPresenter _presenter;
        private bool _isInitialized;
        
        [Inject]
        private void Init(IGameWindowPresenter presenter)
        {
            _presenter = presenter;
            _presenter.Init(new Modifier(this));

            _isInitialized = true;
        }

        protected override void OnShow(int levelId)
        {
            _presenter.ResetGame(levelId);
        }

        public void LoadGame()
        {
            Show();

            if (!_presenter.LoadGame())
                GoHome();
        }
        
        public override void Hide()
        {
            if (_isInitialized)
                _presenter.OnHide();
            
            base.Hide();
        }

        public void GoHome()
        {
            _presenter.GoHome();
        }
        
        public void Save()
        {
            _presenter.Save();
        }

        private class Modifier : IGameWindowModifier
        {
            private GameWindow _window;

            public Modifier(GameWindow window)
            {
                _window = window;
            }

            public void SetTryCount(int count)
            {
                _window.tryCountText.text = count.ToString();
            }

            public void SetMatchCount(int count)
            {
                _window.matchCountText.text = count.ToString();
            }

            public void SetTimeout(float time)
            {
                _window.timeoutView.SetTimeout(time);
            }

            public void StartTimeout(float time)
            {
                _window.timeoutView.SetTimeout(time);
                _window.timeoutView.StartTimeout();
            }

            public void StopTimeout()
            {
                _window.timeoutView.StopTimeout();
            }

            public void ClearCards()
            {
                _window.gameBoard.ClearCards();
            }

            public void SetCards(ICard[] cards, int boardWidth, int boardHeight)
            {
                _window.gameBoard.SetCards(cards, boardWidth, boardHeight);
            }
        }
    }
}