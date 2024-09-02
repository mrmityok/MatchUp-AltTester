using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using MatchUp.Data;
using UnityEngine;
using Serialization;
using Zenject;

namespace MatchUp.Business
{
    public class Game : MonoBehaviour, IGame
    {
        private enum GameState
        {
            Invalid,
            Waiting,
            Started,
            Complete
        }

        public event Action Started;
        public event Action CardFacedUp;
        public event Action PairSelected;
        public event Action Complete;

        private List<int> usedCardIds = new List<int>();
        private List<int> lastUsedCardIds = new List<int>();
        private List<int> currentUsedCardIds = new List<int>();

        private Coroutine delayedFaceDownCoroutine = null;

        ICard[] IGame.Cards => cards;

        private Card[] cards = new Card[0];
        private List<Card> tempCardsPool = new List<Card>();
        private List<Card> cardToConfigure = new List<Card>();
        private Dictionary<Card, bool> cardsPool = new Dictionary<Card, bool>();
        private GameState _state = GameState.Invalid;
        private int? selectedCardId;
        private int matchingCount;

        private IEnumerable<IResourcedSpriteInfo> CardSprites { get; set; }

        private GameState State
        {
            get => _state;
            set
            {
                if (_state == value)
                    return;

                _state = value;

                switch (_state)
                {
                    case GameState.Started:
                        Started?.Invoke();
                        break;

                    case GameState.Complete:
                        Complete?.Invoke();
                        break;
                }
            }
        }

        private void OnCardSelected(Card card)
        {
            if (State != GameState.Waiting && State != GameState.Started)
                return;
            
            if (card == null || !cards.Contains(card))
                return;

            if (delayedFaceDownCoroutine != null)
            {
                StopCoroutine(delayedFaceDownCoroutine);
                delayedFaceDownCoroutine = null;

                var facedUpCards = cards.Where(c => !c.IsMatched && c.IsFacedUp && c != card);
                foreach (var c in facedUpCards)
                    c.FaceDown();
            }

            if (card.FaceUp() || !selectedCardId.HasValue && !card.IsMatched && card.IsFacedUp)
            {
                if (State == GameState.Waiting)
                    State = GameState.Started; // TODO started before card is facedUp?

                CardFacedUp?.Invoke();

                if (!selectedCardId.HasValue)
                    OnFirstCardSelected(card);
                else if (selectedCardId == card.SpriteId)
                    OnNextValidCardSelected();
                else
                    OnWronCardSelected();
            }
        }

        private void OnFirstCardSelected(Card card)
        {
            selectedCardId = card.SpriteId;
        }
        
        private void OnNextValidCardSelected()
        {
            var selectedCards = cards.Where(c => !c.IsMatched && c.IsFacedUp);
            if (selectedCards.Count() >= matchingCount)
            {
                selectedCardId = null;
                        
                foreach (var c in selectedCards)
                    c.IsMatched = true;
                        
                PairSelected?.Invoke();

                if (cards.Count(c => !c.IsMatched) <= matchingCount)
                {
                    var remainingCards = cards.Where(c => !c.IsMatched);
                    foreach (var c in remainingCards)
                        c.FaceUp();
                            
                    State = GameState.Complete;
                }
            }
        }
        
        private void OnWronCardSelected()
        {
            selectedCardId = null;

            PairSelected?.Invoke();

            var selectedCards = cards.Where(c => !c.IsMatched && c.IsFacedUp);
            delayedFaceDownCoroutine = StartCoroutine(DelayedFaceDown(selectedCards, 1));
        }

        private IEnumerator DelayedFaceDown(IEnumerable<Card> cards, float delay)
        {
            yield return new WaitForSeconds(delay);

            foreach (var c in cards)
                c.FaceDown();

            delayedFaceDownCoroutine = null;
        }

        private void ResetCards(int cardCount)
        {
            cards = new Card[cardCount]; //TODO
            for (var i = 0; i < cards.Length; i++)
            {
                Card c;
                if (i < cardsPool.Count)
                {
                    c = cardsPool.ElementAt(i).Key;
                    
                    if (!cardsPool[c])
                        c.Selected += OnCardSelected;
                    
                    cardsPool[c] = true;
                }
                else
                {
                    c = new Card();
                    cardsPool.Add(c, true);
                    c.Selected += OnCardSelected;
                }

                cards[i] = c;
            }

            for (int i = cards.Length; i < cardsPool.Count; i++)
            {
                var c = cardsPool.ElementAt(i).Key;
                
                if (cardsPool[c])
                    c.Selected -= OnCardSelected;
                
                cardsPool[c] = false;
            }
        }

        private void ResetCardsFrom(IEnumerable<ICard> sourceCards)
        {
            ResetCards(sourceCards.Count());

            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].Reset(sourceCards.ElementAt(i).SpriteId);

                if (sourceCards.ElementAt(i).IsFacedUp)
                    cards[i].FaceUp();
                
                cards[i].IsMatched = sourceCards.ElementAt(i).IsMatched;
            }
        }

        private void ConfigureCards()
        {
            var unusedCardIds =
                CardSprites.Select(c => c.Id).Except(usedCardIds).ToList(); //TODO remove ToList?
            
            currentUsedCardIds.Clear();
            cardToConfigure.Clear();
            cardToConfigure.AddRange(cards);
            
            for (var i = 0; i < cards.Length / matchingCount; i++)
            {
                if (!unusedCardIds.Any())
                {
                    usedCardIds.Clear();
                    usedCardIds.AddRange(currentUsedCardIds);

                    int leftCount = cards.Length / matchingCount - i;
                    int unusedCount = CardSprites.Count() - usedCardIds.Count;
                    if (unusedCount - lastUsedCardIds.Count >= leftCount)
                        usedCardIds.AddRange(lastUsedCardIds);
                    
                    unusedCardIds.AddRange(CardSprites.Select(c => c.Id).Except(usedCardIds));
                }

                int j = UnityEngine.Random.Range(0, unusedCardIds.Count);

                int id = unusedCardIds.ElementAt(j);
                unusedCardIds.RemoveAt(j);
                usedCardIds.Add(id);
                currentUsedCardIds.Add(id);
                
                for (int k = 0; k < matchingCount; k++)
                {
                    int l = UnityEngine.Random.Range(0, cardToConfigure.Count);
                    cardToConfigure.ElementAt(l).Reset(id);
                    cardToConfigure.RemoveAt(l);
                }
            }
        }

        public bool Reset(int cardCount, int matchingCount)
        {
            if (matchingCount < 2 || cardCount <= matchingCount || cardCount % matchingCount != 0 ||
                cardCount / matchingCount > CardSprites.Count())
                return false;

            this.matchingCount = matchingCount;
            
            ResetCards(cardCount);

            ConfigureCards();
            
            selectedCardId = null;
            lastUsedCardIds.Clear();
            lastUsedCardIds.AddRange(currentUsedCardIds);
            
            State = GameState.Waiting;

            return true;
        }

        public void Stop()
        {
            State = GameState.Invalid;
        }
        
        [Inject]
        public void Init(IResourcedData resourcedData)
        {
            CardSprites = resourcedData.CardSprites;
            usedCardIds.RemoveAll(id => CardSprites.All(c => c.Id != id));
        }
        
        #region IDataSerializable

        bool IDataSerializable.IsArray => false;

        bool IDataSerializable.Save(IDataSerializer serializer)
        {
            bool ok = serializer.Write("gameState", (int) State)
                      && serializer.Write("usedCardIds", usedCardIds)
                      && serializer.Write("lastUsedCardIds", lastUsedCardIds)
                      && serializer.Write("selectedCardId", selectedCardId ?? -1)
                      && serializer.Write("cardCount", cards?.Length ?? 0)
                      && serializer.Write("cards", cards);
            
            return ok;
        }

        bool IReadDataSerializable.Load(IReadDataSerializer serializer)
        {
            bool ok = true;
            
            int temp_gameStateId = serializer.ReadInt("gameState", ref ok);
            int[] temp_usedCardIds = serializer.Read<int[]>("usedCardIds");
            int[] temp_lastUsedCardIds = serializer.Read<int[]>("lastUsedCardIds");
            int temp_selectedCardId = serializer.ReadInt("selectedCardId", ref ok, true, -1);
            int temp_cardCount = serializer.ReadInt("cardCount", ref ok);

            GameState temp_gameState = GameState.Invalid;

            if (ok && Enum.IsDefined(typeof(GameState), temp_gameStateId))
                temp_gameState = (GameState) temp_gameStateId;
            else
                ok = false;
                
            if (ok)
            {
                for (var i = tempCardsPool.Count; i < temp_cardCount; i++)
                    tempCardsPool.Add(new Card());
                
                ok = serializer.Read("cards", tempCardsPool.Take(temp_cardCount));
            }
            
            ok = ok && serializer.AddTrSuccessHandler(() =>
            {
                usedCardIds.Clear();
                usedCardIds.AddRange(temp_usedCardIds);
                lastUsedCardIds.Clear();
                lastUsedCardIds.AddRange(temp_lastUsedCardIds);
                selectedCardId = temp_selectedCardId;
                ResetCardsFrom(tempCardsPool.Take(temp_cardCount));
                State = temp_gameState;
            });

            return ok;
        }

        #endregion
    }
}