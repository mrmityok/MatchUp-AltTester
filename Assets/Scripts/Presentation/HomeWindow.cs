using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Presentation
{
    public interface IHomeWindowModifier
    {
        void ResetLevelIds(IEnumerable<int> levelIds);
    }
    
    public class HomeWindow : Window
    {
        [SerializeField] private Transform levelSelectionContainer = null;
        
        private IHomeWindowPresenter _presenter;
        private ILevelSelectorViewFactory _levelSelectorViewFactory;

        private List<ILevelSelectorView> selectorViewPool = new List<ILevelSelectorView>();
        
        [Inject]
        private void Init(IHomeWindowPresenter presenter, ILevelSelectorViewFactory levelSelectorViewFactory)
        {
            _levelSelectorViewFactory = levelSelectorViewFactory;
                
            _presenter = presenter;
            _presenter.Init(new Modifier(this));
        }

        public void Save()
        {
            _presenter.Save();
        }

        public void Load()
        {
            _presenter.Load();
        }
        
        private void ShowLevelIds(IEnumerable<int> levelIds)
        {
            // trying to use existing views to show cards
            for (int i = 0; i < levelIds.Count(); i++)
            {
                if (selectorViewPool.Count <= i)
                    selectorViewPool.Add(CreateSelectorView());

                var cv = selectorViewPool[i];
                cv.Show(levelIds.ElementAt(i));
            }

            // hide unused views
            for (int i = levelIds.Count(); i < selectorViewPool.Count; i++) 
                selectorViewPool[i].Hide();
        }

        private ILevelSelectorView CreateSelectorView()
        {
            var levelSelectionItem = _levelSelectorViewFactory.Create(levelSelectionContainer);
            levelSelectionItem.Selected += _presenter.OnLevelSelected;
            
            return levelSelectionItem;
        }

        #region IHomeWindowModifier

        private class Modifier : IHomeWindowModifier
        {
            private readonly HomeWindow _window;
            
            public Modifier(HomeWindow window)
            {
                _window = window;
            }

            public void ResetLevelIds(IEnumerable<int> levelIds)
            {
                _window.ShowLevelIds(levelIds);
            }
        }
        
        #endregion
    }
}