using System.Collections.Generic;

namespace MatchUp.Data
{
    public interface ILevelInfo
    {
        int Id { get; }
        string Name { get; }
        int BoardWidth { get; }
        int BoardHeight { get; }
        int TimeLimit { get; }
        int MatchingCount { get; }
    }

    public interface ILevelsData
    {
        IEnumerable<ILevelInfo> Levels { get; }
    }
}