using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MatchUp.Data;
using UnityEngine;
using Serialization;
using Zenject;

namespace MatchUp.Business
{
    public class GameManager : MonoBehaviour, IGameManager
    {
        public event Action<GameState> GameStateChanged;
        public event Action<int> TryCountChanged;
        public event Action<ILevelInfo> LevelInfoChanged;

        [Inject]
        private void Init(ILevelsData levelsData, IGame game)
        {
            LevelsData = levelsData;

            Game = game;
            Game.Started += OnGameStarted;
            Game.Complete += OnGameComplete;
            Game.PairSelected += OnPairSelected;
        }

        public GameState GameState
        {
            get => _gameState;
            private set
            { 
                if (timeoutCoroutine != null)
                {
                    StopCoroutine(timeoutCoroutine);
                    timeoutCoroutine = null;
                }

                _gameState = value;

                switch (_gameState)
                {
                    case GameState.Waiting:
                        StartTime = null;
                        ResultTime = null;
                        TryCount = 0;
                        break;

                    case GameState.Started:
                        StartTime = Time.timeSinceLevelLoad;
                        break;

                    case GameState.Won:
                        ResultTime = Time.timeSinceLevelLoad - StartTime.Value;
                        break;
                    
                    case GameState.Failed:
                        Game.Stop();
                        break;
                }

                GameStateChanged?.Invoke(_gameState);
            }
        }

        public int TimeLimit => LevelInfo.TimeLimit;
        public float? StartTime { get; private set; }

        public float? ResultTime { get; private set; }

        public int TryCount
        {
            get => _tryCount;
            private set
            {
                if (_tryCount == value)
                    return;
                
                _tryCount = value;
                TryCountChanged?.Invoke(TryCount);
            }
        }

        private Coroutine timeoutCoroutine = null;

        public ICard[] Cards => Game.Cards;

        private GameState _gameState = GameState.Invalid;
        private int _tryCount;

        public ILevelInfo LevelInfo { get; private set; }

        public IEnumerable<ILevelInfo> LevelsInfo => LevelsData.Levels;

        public bool Reset(int levelInfoId)
        { 
            var lvl = LevelsInfo.FirstOrDefault(l => l.Id == levelInfoId);
            if (lvl == null)
                return false;

            if (!Game.Reset(lvl.BoardWidth * lvl.BoardHeight, lvl.MatchingCount))
                return false;

            LevelInfo = lvl;
            LevelInfoChanged?.Invoke(LevelInfo);

            GameState = GameState.Waiting;

            return true;
        }

        private ILevelsData LevelsData { get; set; }

        private IGame Game { get; set; }

        private IGameSerializer _gameSerializer;

        
        private void OnGameStarted()
        {
            GameState = GameState.Started;
            
            timeoutCoroutine = StartCoroutine(DelayedTimeout(TimeLimit));
        }

        private void OnPairSelected()
        {
            TryCount++;
        }

        private void OnGameComplete()
        {
            GameState = GameState.Won;
        }

        private IEnumerator DelayedTimeout(float delay)
        {
            yield return new WaitForSeconds(delay);

            timeoutCoroutine = null;
            
            GameState = GameState.Failed;
        }
        
        #region IDataSerializable

        bool IDataSerializable.IsArray => false;

        bool IDataSerializable.Save(IDataSerializer serializer)
        {
            bool ok = serializer.Write("levelId", LevelInfo?.Id ?? -1)
                && serializer.Write("gameState", (int) GameState)
                && serializer.Write("passedTime", StartTime.HasValue ? (Time.timeSinceLevelLoad - StartTime.Value) : -1)
                && serializer.Write("resultTime", ResultTime ?? -1)
                && serializer.Write("tryCount", TryCount)
                && serializer.Write("game", Game);
            
            return ok;
        }

        bool IReadDataSerializable.Load(IReadDataSerializer serializer)
        {
            bool ok = true;
            
            int temp_levelId = serializer.ReadInt("levelId", ref ok, true, -1);
            int temp_gameStateId = serializer.ReadInt("gameState", ref ok);
            float temp_passedTime = serializer.ReadFloat("passedTime", ref ok, true, -1);
            float temp_resultTime = serializer.ReadFloat("resultTime", ref ok, true, -1);
            int temp_tryCount = serializer.ReadInt("tryCount", ref ok);
            ok = ok && LevelsInfo.Any(l => l.Id == temp_levelId);

            GameState temp_gameState = GameState.Invalid;
            if (ok && Enum.IsDefined(typeof(GameState), temp_gameStateId))
                temp_gameState = (GameState) temp_gameStateId;

            IDataSerializeTransaction transaction = null;
                
            ok = ok && serializer.AddTrSuccessHandler(() =>
            {
                if (Reset(temp_levelId))
                {
                    ResultTime = temp_resultTime > 0 ? temp_resultTime : (float?) null;
                    if (temp_passedTime > 0)
                        StartTime = Time.timeSinceLevelLoad - temp_passedTime;
                    
                    transaction.Commit();
                    
                    GameState = temp_gameState;
                    TryCount = temp_tryCount;
                    ResultTime = temp_resultTime > 0 ? temp_resultTime : (float?) null; 

                    if (temp_passedTime > 0)
                        StartTime = Time.timeSinceLevelLoad - temp_passedTime;
                    
                    GameStateChanged?.Invoke(GameState.Waiting); //TODO refactor
                    GameStateChanged?.Invoke(_gameState);
                    
                    timeoutCoroutine = StartCoroutine(DelayedTimeout(TimeLimit - temp_passedTime));
                }
                else
                {
                    transaction.Rollback();
                }
            });
            
            if (ok)
                transaction = serializer.BeginTransaction();
            
            ok = ok && serializer.AddTrFailHandler(() => transaction.Rollback()) &&
                 transaction != null && 
                 serializer.Read("game", Game);
            
            return ok;
        }

        #endregion
    }
}