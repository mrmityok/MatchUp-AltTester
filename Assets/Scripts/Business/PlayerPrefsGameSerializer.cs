using Serialization;

namespace MatchUp.Business
{
    public class PlayerPrefsGameSerializer : AbstractGameSerializer
    {
        private JsonPlayerPrefsDataSerializer _serializer = new JsonPlayerPrefsDataSerializer();

        public string ProgressKey { get; set; }
        public string GameKey { get; set; }
        
        public PlayerPrefsGameSerializer(IPlayer player, IGameManager gameManager) 
            : base (player, gameManager) { }

        protected override IDataSerializer BeginWriting(SerializationType serializationType)
        {
            return _serializer;
        }

        protected override bool EndWriting(IDataSerializer serializer, SerializationType serializationType)
        {
            switch (serializationType)
            {
                case SerializationType.Progress:
                    return _serializer.EndWriting(ProgressKey);
                        
                case SerializationType.Game:
                    return _serializer.EndWriting(GameKey);
            }

            return false;
        }

        protected override IDataSerializer BeginReading(SerializationType serializationType)
        {
            switch (serializationType)
            {
                case SerializationType.Progress:
                    if (!_serializer.BeginReading(ProgressKey))
                        return null;
                    break;
                        
                case SerializationType.Game:
                    if (!_serializer.BeginReading(GameKey))
                        return null;
                    break;
            }

            return _serializer;
        }

        protected override void EndReading(IDataSerializer serializer, SerializationType serializationType)
        {
            _serializer.Reset();
        }
    }
}