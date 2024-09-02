using MatchUp.Business;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Presentation
{
    public interface ICardView
    {
        void Show(ICard cardInfo);
        void Hide();
        void Select();
        float AspectRatio();
    }
    
    public interface ICardViewModifier
    {
        void SetSprite(Sprite sprite);
        void SetFacedUp(bool isFacedUp);
        void SetMatched(bool isMatched);
    }
    
    public class CardView : MonoBehaviour, ICardView
    {
        [SerializeField] private Image iconImage = null;
        [SerializeField] private GameObject foreground = null;
        [SerializeField] private GameObject background = null;
        [SerializeField] private GameObject content = null;

        private ICardViewPresenter _presenter;
        private AspectRatioFitter _aspectRatioFitter;

        [Inject]
        private void Init(ICardViewPresenter presenter)
        {
            _presenter = presenter;
            _presenter.Init(new Modifier(this));
            
            _aspectRatioFitter = content.GetComponent<AspectRatioFitter>();
        }

        public float AspectRatio()
        {
            return _aspectRatioFitter != null 
                ? _aspectRatioFitter.aspectRatio 
                : -1f;
        }

        public void Show(ICard cardInfo)
        {
            gameObject.SetActive(true);
            
            _presenter.SetCard(cardInfo);
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
            
            _presenter.SetCard(null);
        }

        public void Select()
        {
            _presenter.Select();
        }

        #region Modifier

        private class Modifier : ICardViewModifier
        {
            private readonly CardView _view;

            public Modifier(CardView view)
            {
                _view = view;
            }

            public void SetSprite(Sprite sprite)
            {
                _view.iconImage.sprite = sprite;
            }

            public void SetFacedUp(bool isFacedUp)
            {
                _view.foreground.SetActive(isFacedUp);
                _view.background.SetActive(!isFacedUp);
            }

            public void SetMatched(bool isMatched)
            {
               _view.content.SetActive(!isMatched);
            }
        }
        
        #endregion
    }
}