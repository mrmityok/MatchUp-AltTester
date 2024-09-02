using UnityEngine;

namespace Serialization
{
    public sealed class JsonPlayerPrefsDataSerializer : AstractJsonStreamDataSerializer
    {
        public bool BeginReading(string key)
        {
            string json = PlayerPrefs.GetString(key, null);
            
            //Debug.LogError(json);
            
            return BeginReadingFromString(json);
        }

        public bool EndWriting(string key)
        {
            if (!EndWritingToString(out string json))
                return false;

            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
            
            //Debug.LogError(json);
            
            return true;
        }

    }
}