using System;
using System.Collections.Generic;
using UnityEngine;

namespace MatchUp.Data
{
    [CreateAssetMenu(fileName = "LevelsData", menuName = "Levels Data", order = 1051)]
    public class LevelsData : ScriptableObject, ILevelsData
    {
        [Serializable]
        public class LevelDataItem : ILevelInfo
        {
            public int Id => id;
            public string Name => name;
            public int BoardWidth => boardWidth;
            public int BoardHeight => boardHeight;
            public int TimeLimit => timeLimit;
            public int MatchingCount => matchingCount;


            public int id = 0;
            public string name = string.Empty;
            public int boardWidth = 2;
            public int boardHeight = 2;
            public int timeLimit = 1;
            public int matchingCount = 2;
        }

        IEnumerable<ILevelInfo> ILevelsData.Levels => levels;

        public List<LevelDataItem> levels = new List<LevelDataItem>();
    }
}