using UnityEngine;
using Zenject;

namespace Presentation
{
    public interface ILevelSelectorViewFactory
    {
        ILevelSelectorView Create(Transform parent);
    }
    
    public class LevelSelectorViewFactory : ILevelSelectorViewFactory
    {
        private readonly DiContainer _diContainer;
        private readonly ILevelSelectorView _levelSelectorView;

        public LevelSelectorViewFactory(DiContainer diContainer, ILevelSelectorView levelSelectorView)
        {
            _diContainer = diContainer;
            _levelSelectorView = levelSelectorView;
        }

        public ILevelSelectorView Create(Transform parent)
        {
            var levelSelector = _diContainer.InstantiatePrefab(_levelSelectorView as UnityEngine.Object, parent);
            
            if (levelSelector != null)
                return levelSelector.GetComponent<ILevelSelectorView>();

            return null;
        }
    }
}