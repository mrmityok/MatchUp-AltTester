using System;
using System.Collections.Generic;
using Serialization;

namespace MatchUp.Business
{
    public interface IPlayer  : IDataSerializable
    {
        event Action/*<int>*/ BestResultsChanged;

        IDictionary<int, IGameResult> BestResults { get; }
        bool IsBestResultUpdated { get; }
    }
    
    public interface IGameResult
    {
        float TimePassed { get; }
        int TryCount { get; }
    }
}