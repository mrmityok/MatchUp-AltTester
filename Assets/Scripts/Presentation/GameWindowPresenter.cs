using MatchUp.Business;
using UnityEngine;

namespace Presentation
{
    internal interface IGameWindowPresenter
    {
        void Init(IGameWindowModifier window);
        void ResetGame(int levelId);
        bool LoadGame();
        void Save();
        void GoHome();
        void OnHide();
    }
    
    public class GameWindowPresenter : IGameWindowPresenter
    {
        private IWindowManager _windowManager;
        private IGameManager _gameManager;
        private IGameSerializer _gameSerializer;
        private IPlayer _player;
        
        private IGameWindowModifier _window;
        private int _levelId;

        public GameWindowPresenter(IWindowManager windowManager, IGameManager gameManager, IGameSerializer gameSerializer, IPlayer player)
        {
            _windowManager = windowManager;
            _gameManager = gameManager;
            _gameSerializer = gameSerializer;
            _player = player;

            _gameManager.GameStateChanged += OnGameStateChanged;
            _gameManager.TryCountChanged += OnTryCountChanged;
        }

        public void Init(IGameWindowModifier window)
        {
            _window = window;
        }

        public void ResetGame(int levelId)
        {
            _levelId = levelId;
            _gameManager.Reset(levelId);
        }

        public bool LoadGame()
        {
            return _gameSerializer.LoadGame();
        }

        public void Save()
        {
            _gameSerializer.SaveProgress();
            
            if (_gameManager.GameState == GameState.Waiting || _gameManager.GameState == GameState.Started)
                _gameSerializer.SaveGame();
        }

        public void GoHome()
        {
            StopGame();
            _windowManager.GoHome();
        }

        public void OnHide()
        {
            _window.ClearCards();
            Resources.UnloadUnusedAssets();
        }

        private void ResetGame()
        {
            ResetGame(_levelId);
        }

        private void StopGame()
        {
            if (_gameManager.GameState == GameState.Started)
                _gameManager.Reset(_gameManager.LevelInfo.Id);
        }

        private void OnTryCountChanged(int count)
        {
            _window.SetTryCount(count);
        }

        private void OnGameStateChanged(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.Waiting:
                    OnGameReset();
                    break;
                
                case GameState.Started:
                    OnGameStarted();
                    break;
                
                case GameState.Failed:
                    OnGameFailed();
                    break;
                
                case GameState.Won:
                    OnGameWon();
                    break;
            }
        }

        private void OnGameReset()
        {
            _window.SetCards(
                _gameManager.Cards, 
                _gameManager.LevelInfo.BoardWidth,
                _gameManager.LevelInfo.BoardHeight);

            _window.StopTimeout();
            _window.SetTimeout(_gameManager.LevelInfo.TimeLimit);
            _window.SetMatchCount(_gameManager.LevelInfo.MatchingCount);
            OnTryCountChanged(_gameManager.TryCount);
        }

        private void OnGameStarted()
        {
            _window.StartTimeout(_gameManager.StartTime.Value + _gameManager.LevelInfo.TimeLimit - Time.timeSinceLevelLoad);
        }

        private void OnGameFailed()
        {
            _window.StopTimeout();
            
            _windowManager.ShowGameOver(ResetGame, GoHome);
        }

        private void OnGameWon()
        {
            _window.StopTimeout();
            
            _windowManager.ShowWin(
                new WinWindowArgs(
                    _gameManager.ResultTime.Value, 
                    _gameManager.TryCount, 
                    _player.IsBestResultUpdated,
                    ResetGame,
                    GoHome));
        }
    }
}