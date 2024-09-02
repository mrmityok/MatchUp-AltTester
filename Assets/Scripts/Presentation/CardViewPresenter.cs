using System.Linq;
using MatchUp.Business;
using MatchUp.Data;
using UnityLibrary.Helpers;

namespace Presentation
{
    internal interface ICardViewPresenter
    {
        void Init(ICardViewModifier view);

        void SetCard(ICard cardInfo);

        void Select();
    }
    
    public class CardViewPresenter : ICardViewPresenter
    {
        private ICardViewModifier _view;

        private IResourcedData _resourcedData;
        
        private ICard _cardInfo;

        private ResourcedSprite _sprite = null;

        public CardViewPresenter(IResourcedData resourcedData)
        {
            _resourcedData = resourcedData;
        }

        public void Init(ICardViewModifier view)
        {
            _view = view;
        }

        public void SetCard(ICard cardInfo)
        {
            if (_cardInfo != null)
            {
                _cardInfo.Matched -= OnCardMatched;
                _cardInfo.FacedUpChanged -= SetCardFacedUp;
            }
            
            _cardInfo = cardInfo;
            
            FreeSprite();
            
            if (_cardInfo == null)
                return;

            _cardInfo.Matched += OnCardMatched;
            _cardInfo.FacedUpChanged += SetCardFacedUp;

            _sprite = _resourcedData.CardSprites.First(c => c.Id == _cardInfo.SpriteId).Sprite;

            _view.SetSprite(_sprite.Value);
            SetCardFacedUp(_cardInfo.IsFacedUp);
            _view.SetMatched(_cardInfo.IsMatched);
        }

        private void FreeSprite()
        {
            _view.SetSprite(null);
            _sprite?.Free();
            _sprite = null;
        }

        public void Select()
        {
            _cardInfo.Select();
        }

        private void OnCardMatched()
        {
            _view.SetMatched(true);
        }

        private void SetCardFacedUp(bool isFacedUp)
        {
            _view.SetFacedUp(isFacedUp);
        }
    }
}