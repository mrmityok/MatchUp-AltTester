using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Serialization;

namespace MatchUp.Business
{
    public class Player : IPlayer
    {
        [Serializable]
        private class Result : IGameResult
        {
            [SerializeField]
            public float timePassed;
            [SerializeField]
            public int tryCount;

            public float TimePassed => timePassed;
            public int TryCount => tryCount;

            public Result(float timePassed, int tryCount)
            {
                this.timePassed = timePassed;
                this.tryCount = tryCount;
            }

            public Result() : this(0, 0)
            {
                
            }
        }
        
        public event Action/*<int>*/ BestResultsChanged;

        public IDictionary<int, IGameResult> BestResults { get; }
        public bool IsBestResultUpdated { get; private set; }

        private IGameManager GameManager { get; set; }

        public Player(IGameManager gameManager)
        {
            BestResults = new Dictionary<int, IGameResult>();
            IsBestResultUpdated = false;
            
            GameManager = gameManager;
            GameManager.GameStateChanged += OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState state)
        {
            IsBestResultUpdated = false;
            
            if (state != GameState.Won)
                return;
            
            if (!BestResults.TryGetValue(GameManager.LevelInfo.Id, out IGameResult bestResult))
            {
                bestResult = new Result(GameManager.ResultTime.Value, GameManager.TryCount);
                BestResults.Add(GameManager.LevelInfo.Id, bestResult);

                BestResultsChanged?.Invoke();//GameManager.LevelInfo.Id);
            }
            else
            {
                if (GameManager.TryCount < bestResult.TryCount
                    || GameManager.TryCount == bestResult.TryCount &&
                    GameManager.ResultTime.Value < bestResult.TimePassed)
                {
                    BestResults[GameManager.LevelInfo.Id] = new Result(GameManager.ResultTime.Value, GameManager.TryCount);

                    IsBestResultUpdated = true;
                    BestResultsChanged?.Invoke();//GameManager.LevelInfo.Id);
                }
            }
        }

        #region IDataSerializable

        bool IDataSerializable.IsArray => false;

        bool IDataSerializable.Save(IDataSerializer serializer)
        {
            bool ok = serializer.Write("isBestResultUpdated", IsBestResultUpdated)
                      && serializer.Write("bestResults", BestResults.ToList().Select(p =>
                          new SerializableKeyValuePair<int, Result>(p.Key, (Result) p.Value)));

            return ok;
        }

        bool IReadDataSerializable.Load(IReadDataSerializer serializer)
        {
            bool ok = true;
            bool temp_isBestResultUpdated = serializer.ReadBool("isBestResultUpdated", ref ok);
            var temp_bestResults = serializer.Read<SerializableKeyValuePair<int, Result>[]>("bestResults", true);
            
            ok = ok && serializer.AddTrSuccessHandler(() =>
            {
                IsBestResultUpdated = temp_isBestResultUpdated;
                
                BestResults.Clear();
                if (temp_bestResults != null)
                {
                    foreach (var r in temp_bestResults)
                        if (!BestResults.ContainsKey(r.key))
                            BestResults.Add(r.key, r.value);
                }
                
                BestResultsChanged?.Invoke();//r.Key);
            });

            return ok;
        }

        #endregion
    }
}