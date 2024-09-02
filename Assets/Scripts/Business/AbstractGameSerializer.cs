using UnityEngine;
using Serialization;

namespace MatchUp.Business
{
    public abstract class AbstractGameSerializer : IGameSerializer
    {
        protected enum SerializationType { Progress, Game }
        
        private IPlayer _player;
        private IGameManager _gameManager;
        
        public AbstractGameSerializer(IPlayer player, IGameManager gameManager)
        {
            _player = player;
            _gameManager = gameManager;
            
            _gameManager.GameStateChanged += OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Won)
                SaveProgress();
        }

        protected abstract IDataSerializer BeginWriting(SerializationType serializationType);
        
        protected abstract bool EndWriting(IDataSerializer serializer, SerializationType serializationType);

        protected abstract IDataSerializer BeginReading(SerializationType serializationType);

        protected abstract void EndReading(IDataSerializer serializer, SerializationType serializationType);
            
        private bool Save(SerializationType serializationType)
        {
            IDataSerializer serializer = BeginWriting(serializationType);
            if (serializer == null)
            {
                Debug.LogErrorFormat("GameSerializer failed to init writing to save '{0}'", serializationType);
                return false;
            }

            bool saved = false;
            
            try
            {
                switch (serializationType)
                {
                    case SerializationType.Progress:
                        saved = serializer.Write("player", _player);
                        break;
                        
                    case SerializationType.Game:
                        saved = serializer.Write("_gameManager", _gameManager);
                        break;
                }
            }
            finally
            { 
                saved = saved && EndWriting(serializer, serializationType);
            }

            return saved;
        }
        
        private bool Load(SerializationType serializationType)
        {
            IDataSerializer serializer = BeginReading(serializationType);
            if (serializer == null)
            {
                Debug.LogWarningFormat("GameSerializer failed to init reading to load '{0}'", serializationType);
                return false;
            }

            bool loaded = false;

            try
            { 
                var transaction = serializer.BeginTransaction();
                
                if (transaction != null)
                {
                    switch (serializationType)
                    {
                        case SerializationType.Progress:
                            loaded = serializer.Read("player", _player);
                            break;
                        
                        case SerializationType.Game:
                            loaded = serializer.Read("_gameManager", _gameManager);
                            break;
                    }

                    loaded = (loaded ? transaction.Commit() : transaction.Rollback()) && loaded;
                }
            }
            finally
            {
                EndReading(serializer, serializationType);
            }

            return loaded;
        }

        public bool SaveProgress()
        {
            return Save(SerializationType.Progress);
        }

        public bool LoadProgress()
        {
            return Load(SerializationType.Progress);
        }

        public bool SaveGame()
        {
            return Save(SerializationType.Game);
        }

        public bool LoadGame()
        {
            return Load(SerializationType.Game);
        }
    }
}