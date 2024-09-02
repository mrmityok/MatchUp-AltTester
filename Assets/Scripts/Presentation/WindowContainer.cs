using System.Collections.Generic;

namespace Presentation
{
    public interface IWindowContainer
    {
        bool AddWindow(IWindow window);
    }

    public class WindowContainer : IWindowContainer
    {
        private readonly List<IWindow> _windows = new List<IWindow>();

        public bool AddWindow(IWindow window)
        {
            if (_windows.Contains(window))
                return false;

            InitWindow(window);
            _windows.Add(window);

            return true;
        }

        private void InitWindow(IWindow window)
        {
            window.Hide();
            window.Showing += OnWindowShowing;
        }

        private void OnWindowShowing(IWindow window)
        {
            // hide other visible windows if new one is not modal
            if (window.IsModal == false)
            {
                foreach (var w in _windows)
                {
                    if (w.IsVisible && w != window)
                        w.Hide();
                }
            }
        }
    }
    
}