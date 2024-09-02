using System;
using UnityEngine;
using Zenject;

namespace Presentation
{
    public interface IWindowManager
    {
        void GoHome();
        void ShowGame(int levelId);
        void Load();
        void ShowWin(WinWindowArgs args);
        void ShowGameOver(Action onConfirm, Action onCancel);
    }

    public class WindowManager : MonoBehaviour, IWindowManager
    {
        [SerializeField] private HomeWindow homeWindow = null;
        [SerializeField] private GameWindow gameWindow = null;
        [SerializeField] private WinWindow winWindow = null;
        [SerializeField] private DialogWindow gameOverWindow = null;

        private IWindowContainer _windowContainer;

        [Inject]
        private void Init(IWindowContainer windowContainer)
        {
            _windowContainer = windowContainer;
            
            _windowContainer.AddWindow(homeWindow);
            _windowContainer.AddWindow(gameWindow);
            _windowContainer.AddWindow(winWindow);
            _windowContainer.AddWindow(gameOverWindow);
        }

        private void Start()
        {
            homeWindow.Show();
        }

        public void GoHome()
        {
            homeWindow.Show();
        }

        public void ShowGame(int levelId)
        {
            gameWindow.Show(levelId);
        }
        
        public void Load()
        {
            gameWindow.LoadGame();
        }

        public void ShowWin(WinWindowArgs args)
        {
            winWindow.Show(args);
        }

        public void ShowGameOver(Action onConfirm, Action onCancel)
        {
            gameOverWindow.Show(onConfirm, onCancel);
        }
    }
}
