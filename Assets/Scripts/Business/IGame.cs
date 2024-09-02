using System;
using Serialization;

namespace MatchUp.Business
{
    public interface ICard
    {
        event Action<bool> FacedUpChanged;
        event Action Matched;

        int SpriteId { get; }

        bool IsFacedUp { get; }
        bool IsMatched { get; }

        void Select();
    }

    public interface IGame  : IDataSerializable
    {
        event Action Started;
        event Action CardFacedUp;
        event Action PairSelected;
        event Action Complete;

        bool Reset(int cardCount, int matchingCount);
        
        void Stop();
        
        ICard[] Cards { get; }
    }
}