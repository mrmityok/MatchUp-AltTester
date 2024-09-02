using System;
using System.Collections.Generic;
using MatchUp.Data;
using Serialization;

namespace MatchUp.Business
{
    public enum GameState
    {
        Invalid,
        Waiting,
        Started,
        Failed,
        Won
    }
    
    public interface IGameManager : IDataSerializable
    {
        event Action<GameState> GameStateChanged;
        event Action<int> TryCountChanged;
        event Action<ILevelInfo> LevelInfoChanged;

        GameState GameState { get; }
        
        float? StartTime { get; }
        float? ResultTime { get; }
        int TryCount { get; }
        
        ICard[] Cards { get; }

        ILevelInfo LevelInfo { get; }
        IEnumerable<ILevelInfo> LevelsInfo { get; }
        
        bool Reset(int levelInfoId);
    }
}