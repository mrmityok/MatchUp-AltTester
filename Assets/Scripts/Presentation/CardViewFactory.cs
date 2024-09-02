using UnityEngine;
using Zenject;

namespace Presentation
{
    public interface ICardViewFactory
    {
        ICardView Create(Transform parent);
    }
    
    public class CardViewFactory : ICardViewFactory
    {
        private readonly DiContainer _diContainer;
        private readonly ICardView _cardViewPrefab;

        public CardViewFactory(DiContainer diContainer, ICardView cardViewPrefab)
        {
            _diContainer = diContainer;
            _cardViewPrefab = cardViewPrefab;
        }

        public ICardView Create(Transform parent)
        {
            var cardView = _diContainer.InstantiatePrefab(_cardViewPrefab as UnityEngine.Object, parent);
            
            if (cardView != null)
                return cardView.GetComponent<ICardView>();

            return null;
        }
    }
}