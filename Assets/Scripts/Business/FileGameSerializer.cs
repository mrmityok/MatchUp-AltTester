using Serialization;

namespace MatchUp.Business
{
    public class FileGameSerializer : AbstractGameSerializer
    {
        private JsonPlayerPrefsDataSerializer _serializer = new JsonPlayerPrefsDataSerializer();
        
        public string ProgressFilePath { get; set; }
        public string GameFilePath { get; set; }

        public FileGameSerializer(IPlayer player, IGameManager gameManager) 
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
                    return _serializer.EndWriting(ProgressFilePath);
                        
                case SerializationType.Game:
                    return _serializer.EndWriting(GameFilePath);
            }

            return false;
        }

        protected override IDataSerializer BeginReading(SerializationType serializationType)
        {
            switch (serializationType)
            {
                case SerializationType.Progress:
                    if (!_serializer.BeginReading(ProgressFilePath))
                        return null;
                    break;
                        
                case SerializationType.Game:
                    if (!_serializer.BeginReading(GameFilePath))
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