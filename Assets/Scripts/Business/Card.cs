using System;
using Serialization;

namespace MatchUp.Business
{
    public class Card : ICard, IDataSerializable
    {
        private bool _isFacedUp;
        private bool _isMatched;

        public event Action<bool> FacedUpChanged;
        public event Action Matched;
        public event Action<Card> Selected;

        public int SpriteId { get; private set; } = -1;

        public bool IsFacedUp
        {
            get => _isFacedUp;
            private set
            {
                if (_isFacedUp == value)
                    return;

                _isFacedUp = value;

                FacedUpChanged?.Invoke(_isFacedUp);
            }
        }

        public bool IsMatched
        {
            get => _isMatched;
            set
            {
                if (_isMatched == value)
                    return;

                _isMatched = value;

                if (_isMatched)
                    Matched?.Invoke();
            }
        }

        public void Select()
        {
            Selected?.Invoke(this);
        }

        public bool FaceUp()
        {
            if (IsMatched || IsFacedUp)
                return false;

            return IsFacedUp = true;
        }

        public void FaceDown()
        {
            IsFacedUp = false;
        }

        public void Reset(int spriteId)
        {
            SpriteId = spriteId;
            IsMatched = false;
            FaceDown();
        }
        
        #region IDataSerializable

        bool IDataSerializable.IsArray => false;

        bool IDataSerializable.Save(IDataSerializer serializer)
        {
            bool ok = serializer.Write("spriteId", SpriteId)
                      && serializer.Write("isFacedUp", IsFacedUp)
                      && serializer.Write("isMatched", IsMatched);
            
            return ok;
        }

        bool IReadDataSerializable.Load(IReadDataSerializer serializer)
        {
            bool ok = true;
            
            int temp_spriteId = serializer.ReadInt("spriteId", ref ok);
            bool temp_isFacedUp = serializer.ReadBool("isFacedUp", ref ok);
            bool temp_isMatched = serializer.ReadBool("isMatched", ref ok);

            ok = ok && serializer.AddTrSuccessHandler(() =>
            {
                SpriteId = temp_spriteId;
                IsFacedUp = temp_isFacedUp;
                IsMatched = temp_isMatched;
            });

            return ok;
        }

        #endregion
    }
}