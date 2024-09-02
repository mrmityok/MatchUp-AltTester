
namespace Serialization
{
    public sealed class JsonStringDataSerializer : AstractJsonStreamDataSerializer//<string, string>
    {
        public bool BeginReading(string json)
        {
            return BeginReadingFromString(json);
        }

        public bool EndWriting(out string json)
        {
            return EndWritingToString(out json);
        }

    }
}