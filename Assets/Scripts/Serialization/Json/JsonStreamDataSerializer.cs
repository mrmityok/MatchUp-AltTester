using System.IO;

namespace Serialization
{
    public class JsonStreamDataSerializer : AstractJsonStreamDataSerializer//<StreamReader, StreamWriter>
    {
        public override bool BeginReading(StreamReader reader)
        {
            return BeginReadingFromStream(reader);
        }

        public override bool EndWriting(StreamWriter writer)
        {
            return EndWritingToStream(writer);
        }
    }
} 