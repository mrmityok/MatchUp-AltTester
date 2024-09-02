using System.Collections.Generic;
using MatchUp.Business;
using UnityEngine;
using Zenject;

namespace Presentation
{
    public class GameBoard : MonoBehaviour
    {
        [SerializeField] private CardGrid cardGrid = null;
        
        private ICardViewFactory _cardViewFactory;
        private List<ICardView> _cardViewPool = new List<ICardView>();

        [Inject] 
        private void Init(ICardViewFactory cardViewFactory)
        {
            _cardViewFactory = cardViewFactory;
        }

        public void ClearCards()
        {
            ResetCardViewPool(0);
        }

        public void SetCards(ICard[] cards, int boardWidth, int boardHeight)
        {
            if (cards == null || cards.Length != boardWidth * boardHeight)
            {
                Debug.LogError("Cannot set cards: wrong arguments");
                
                ClearCards();
                
                return;
            }
            
            ResetCardViewPool(cards.Length);
            
            for (var i = 0; i < cards.Length; i++) 
                _cardViewPool[i].Show(cards[i]);

            ResetGridSize(boardWidth, boardHeight);
        }

        private void ResetCardViewPool(int size)
        {
            // hide unused cardViews
            for (int i = size; i < _cardViewPool.Count; i++) 
                _cardViewPool[i].Hide();
            
            // trying to use existing cardViews to show cards
            for (int i = 0; i < size; i++)
            {
                if (_cardViewPool.Count <= i)
                    _cardViewPool.Add(CreateCardView());
            }
        }

        private ICardView CreateCardView()
        {
            return _cardViewFactory.Create(cardGrid.transform);
        }

        private void ResetGridSize(int boardWidth, int boardHeight)
        {
            var aspectRatio = boardWidth * boardHeight > 0
                ? _cardViewPool[0].AspectRatio() * boardWidth / boardHeight
                : 1;

            cardGrid.SetSize(boardWidth, boardHeight, aspectRatio);
        }
    }
}