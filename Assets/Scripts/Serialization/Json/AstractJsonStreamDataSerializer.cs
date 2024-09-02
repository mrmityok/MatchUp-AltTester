#define DEBUG
#define NULL_TO_DEFAULT

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Boomlagoon.JSON;
using UnityEngine;

#if !USESTRINGBUILDERUNITYJSON
using JsonFx.Json;
#endif

namespace Serialization
{
    public abstract class AstractJsonStreamDataSerializer : IDataSerializer
    {
        #region Fields
        
        private StringBuilder jsonSaveData = new StringBuilder();
        private List<SerializableKeyValuePair<JSONValueType, int>> jsonSaveDataTypes = new List<SerializableKeyValuePair<JSONValueType, int>>();
        private JSONValue jsonLoadData = null;
        private List<JSONValue> lastJsonLoadDataList = new List<JSONValue>();
        private bool begin = false;

        private const string AppendSerializableTemplate = "\"{0}\" : ";
        private const string Delimiter = ", ";
        private const int SubstringLength = 200;

        private static readonly Type StringType = typeof(string);
        
        //Controls the deserialization settings for JsonReader
        protected JsonReaderSettings readSettings = new JsonReaderSettings();
        //Controls the serialization settings for JsonWriter
        protected JsonWriterSettings writeSettings = new JsonWriterSettings();

        #endregion

        /*public bool PrettyPrint
        {
            get { return writeSettings.PrettyPrint; }
            set { writeSettings.PrettyPrint = value; }
        }*/

        #region Functions
        
        ~AstractJsonStreamDataSerializer()
        {
            ClearTransactions(true);
        }

        private static string Substring(string text)
        {
            return text.Substring(0, Mathf.Min(SubstringLength, text.Count()));
        }

        public virtual void Reset()
        {
            ClearTransactions(false);
            
            jsonSaveData.Clear();
            jsonSaveDataTypes.Clear();

            jsonLoadData = null;
            begin = false;

            lastJsonLoadDataList.Clear();
        }

        private bool Begin(string name)
        {
            if (!jsonSaveDataTypes.Any() && !string.IsNullOrEmpty(name) && jsonSaveData.Length == 0)
                Begin(null);
            
            if (jsonSaveDataTypes.Count == 0)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    Debug.LogWarningFormat("JsonStreamDataSerializer: failed to begin '{0}' object due to it has no parent", name);
                    return false;
                }

                begin = true;
                jsonSaveDataTypes.Add(new SerializableKeyValuePair<JSONValueType, int>(JSONValueType.Object, 0));

                jsonSaveData.Append("{ ");

                return true;
            }
            else
            {
                var v = jsonSaveDataTypes.Last();
                if (v.key == JSONValueType.Object)
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        Debug.LogWarning("JsonStreamDataSerializer: failed to begin object due to error with null name");
                        return false;
                    }

                    if (v.value > 0)
                        jsonSaveData.Append(Delimiter);

                    jsonSaveData.Append(Environment.NewLine + string.Format(AppendSerializableTemplate, name) + "{ ");
                    v.value = v.value + 1;
                    jsonSaveDataTypes.Add(new SerializableKeyValuePair<JSONValueType, int>(JSONValueType.Object, 0));

                    return true;
                }
                else if (v.key == JSONValueType.Array)
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        Debug.LogWarningFormat("JsonStreamDataSerializer: failed to begin '{0}' object due to its parent is array", name);
                        return false;
                    }

                    if (v.value > 0)
                        jsonSaveData.Append(Delimiter);

                    jsonSaveData.Append(Environment.NewLine + "{ ");
                    v.value = v.value + 1;
                    jsonSaveDataTypes.Add(new SerializableKeyValuePair<JSONValueType, int>(JSONValueType.Object, 0));

                    return true;
                }

                Debug.LogWarningFormat("JsonStreamDataSerializer: failed to begin '{0}' object due to its parent type is '{1}'", name, v.key);
                return false;
            }
        }

        private bool End()
        {
            if (jsonSaveDataTypes.Count() == 0)
            {
                Debug.LogWarning("JsonStreamDataSerializer: failed to end object due to it has no begin");
                return false;
            }
            else if (jsonSaveDataTypes.Count() == 1)
            {
                var v = jsonSaveDataTypes.Last();
                if (v.key != JSONValueType.Object)
                {
                    Debug.LogWarningFormat("JsonStreamDataSerializer: failed to end object due to its type is '{0}'", v.key);
                    return false;
                }

                if (!begin)
                {
                    Debug.LogWarning("JsonStreamDataSerializer: failed to end object due to it is already finished");
                    return false;
                }

                jsonSaveData.Append(" }");

                begin = false;
                return true;
            }
            else
            { 
                var v = jsonSaveDataTypes.Last();
                if (v.key == JSONValueType.Object)
                {
                    jsonSaveData.Append(" }");

                    jsonSaveDataTypes.Remove(v);
                    return true;
                }
                else if (v.key == JSONValueType.Array)
                {
                    Debug.LogWarning("JsonStreamDataSerializer: failed to end object due to it is array");
                    return false;
                }

                Debug.LogWarningFormat("JsonStreamDataSerializer: failed to end object due to its type is '{0}'", v.key);
                return false;
            }
        }

        private bool BeginList(string name)
        {
            if (!jsonSaveDataTypes.Any() && !string.IsNullOrEmpty(name) && jsonSaveData.Length == 0)
                Begin(null);
            
            if (jsonSaveDataTypes.Count() == 0)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    Debug.LogWarningFormat("JsonStreamDataSerializer: failed to begin '{0}' array due to it has no parent", name);
                    return false;
                }

                begin = true;

                jsonSaveDataTypes.Add(new SerializableKeyValuePair<JSONValueType, int>(JSONValueType.Array, 0));
                jsonSaveData.Append("[ ");

                return true;
            }
            else
            {
                var v = jsonSaveDataTypes.Last();
                if (v.key == JSONValueType.Object)
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        Debug.LogWarning("JsonStreamDataSerializer: failed to begin array due to error with null name");
                        return false;
                    }

                    if (v.value > 0)
                        jsonSaveData.Append(Delimiter);

                    jsonSaveData.Append(Environment.NewLine + string.Format(AppendSerializableTemplate, name) + "[ ");
                    v.value = v.value + 1;
                    jsonSaveDataTypes.Add(new SerializableKeyValuePair<JSONValueType, int>(JSONValueType.Array, 0));

                    return true;
                }
                else if (v.key == JSONValueType.Array)
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        Debug.LogWarningFormat("JsonStreamDataSerializer: failed to begin '{0}' array due to its parent is array", name);
                        return false;
                    }

                    if (v.value > 0)
                        jsonSaveData.Append(Delimiter);

                    jsonSaveData.Append(Environment.NewLine + "[ ");
                    v.value = v.value + 1;
                    jsonSaveDataTypes.Add(new SerializableKeyValuePair<JSONValueType, int>(JSONValueType.Array, 0));

                    return true;
                }

                Debug.LogWarningFormat("JsonStreamDataSerializer: failed to begin '{0}' array due to its parent type is '{1}'", name, v.key);
                return false;
            }
        }

        private bool EndList()
        {

            if (jsonSaveDataTypes.Count() == 0)
            {
                Debug.LogWarning("JsonStreamDataSerializer: failed to end array due to it has no begin");
                return false;
            }
            else if (jsonSaveDataTypes.Count() == 1)
            {
                var v = jsonSaveDataTypes.Last();
                if (v.key != JSONValueType.Array)
                {
                    Debug.LogWarningFormat("JsonStreamDataSerializer: failed to end array due to its type is '{0}'", v.key);
                    return false;
                }

                if (!begin)
                {
                    Debug.LogWarning("JsonStreamDataSerializer: failed to end array due to it is already finished");
                    return false;
                }

                jsonSaveData.Append(" ]");

                begin = false;
                return true;
            }
            else
            {
                var v = jsonSaveDataTypes.Last();
                if (v.key == JSONValueType.Array)
                {
                    jsonSaveData.Append(" ]");

                    jsonSaveDataTypes.Remove(v);
                    return true;
                }
                else if (v.key == JSONValueType.Object)
                {
                    Debug.LogWarning("JsonStreamDataSerializer: failed to end array due to it is object");
                    return false;
                }

                Debug.LogWarningFormat("JsonStreamDataSerializer: failed to end array due to its type is '{0}'", v.key);
                return false;
            }
        }
        
        private string ReadJson(string name, ref bool ok)
        {
            if (jsonLoadData == null || jsonLoadData.Obj == null)
            {
                Debug.LogWarningFormat("JsonStreamDataSerializer: failed to get data due to error parsing '{0}' key in json '{1}'", name, Substring(jsonLoadData.ToString()));
                ok = false;
                return null;
            }

            var jsonObject = jsonLoadData.Obj;

            string json;

            if (!string.IsNullOrEmpty(name))
            {
                var val = jsonObject[name];
                if (val == null)
                {
                    Debug.LogWarningFormat("JsonStreamDataSerializer: failed to read data due to error parsing '{0}' key in json '{1}'", name, Substring(jsonObject.ToString()));

                    ok = false;
                    return null;
                }

                json = val.ToString();
            }
            else
                json = jsonObject.ToString();

            ok = true;
            return json;
        }

        #endregion

        #region JsonSerializer

        private bool Serialize(object data, out string json)
        {
            json = JsonSerializeData(data);

            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning(string.Format("JsonSerializer: failed to serialize '{0}' data", json));
                json = null;
                return false;
            }

            return true;
        }

        private T Deserialize<T>(string name, string json, bool allowNull) where T : class
        {
            if (string.IsNullOrEmpty(json))
            {
                string e = string.Format("JsonStreamDataSerializer: failed to deserialize '{0}' property JSON data '{1}' to {2} value", name, json, typeof(T).Name);
                Debug.LogWarning(e);
                throw new JsonSerializationException(e);
                //return default(T);
            }
            
            if (json.Length == 4 && json.Equals("null"))
            {
                if (allowNull)
                    return null;

                string e = string.Format("JsonStreamDataSerializer: failed to deserialize '{0}' property JSON data '{1}' to {2} value", name, json, typeof(T).Name);
                Debug.LogWarning(e);
                throw new JsonSerializationException(e);
                //return default(T);
            }

            if (typeof(T) == StringType && json.Length >= 2)
                return json.Substring(1, json.Length - 2) as T;

            try
            {
                return JsonDeserializeData<T>(json);
            }
            catch (Exception ex)
            {
                string e = string.Format("JsonStreamDataSerializer: failed to deserialize '{0}' property JSON data '{1}' to {2} value due to: '{3}'", name, json, typeof(T).Name, ex.Message);
                Debug.LogWarning(e);
                throw new JsonSerializationException(e);
                //return default(T);
            }
        }

        private T JsonDeserializeData<T>(string json)
        {
            return JsonFXDeserializeData<T>(json);
        }

        private string JsonSerializeData(object serializable)
        {
            var sb = JsonFXSerializeData(serializable);
            return sb?.ToString();
        }

        private T JsonFXDeserializeData<T>(string json)
        {
            //Reader for consuming JSON data
            var reader = new JsonReader(json, readSettings);

            //Convert from JSON string to Object graph of specific Type
            T deserialized = reader.Deserialize<T>();

            return deserialized;
        }

        private StringBuilder JsonFXSerializeData(object serializable)
        {
            //Represents a mutable string of characters
            var output = new StringBuilder();

            //Writer for producing JSON data
            var writer = new JsonWriter(output, writeSettings);

            //Producing JSON data into StringBuilder
            writer.Write(serializable);

            return output;
        }

        private string ParseStringFromUnicode(string json, ref bool ok)
        {
            if (string.IsNullOrEmpty(json))
                return json;

            int i = 0;
            string s = ParseStringFromUnicode(json, true, ref i, out bool success);
            if (success)
            {
                if (json.Length == i)
                    return s;

                ok = false;
                return null;
            }
            else
            {
                ok = false;
            }

            return json;
        }
        
        public static string ParseStringFromUnicode(string json, bool clearString, ref int index, out bool success)
        {
            if (string.IsNullOrEmpty(json))
            {
                success = false;
                return null;
            }
            
            var s = new StringBuilder();
            char c;
            
            // "
            if (!clearString)
                index++;

            bool complete = false;
            while (!complete)
            {
                if (index == json.Length)
                {
                    complete = clearString;
                    break;
                }

                c = json[index++];
                if (c == '"')
                {
                    complete = true;
                    break;
                }
                else if (c == '\\')
                {
                    if (index == json.Length)
                        break;
                    
                    c = json[index++];
                    if (c == '"')
                        s.Append('"');
                    else if (c == '\\')
                        s.Append('\\');
                    else if (c == '/')
                        s.Append('/');
                    else if (c == 'b')
                        s.Append('\b');
                    else if (c == 'f')
                        s.Append('\f');
                    else if (c == 'n')
                        s.Append('\n');
                    else if (c == 'r')
                        s.Append('\r');
                    else if (c == 't')
                        s.Append('\t');
                    else if (c == 'u')
                    {
                        int remainingLength = json.Length - index;
                        if (remainingLength >= 4)
                        {
                            // parse the 32 bit hex into an integer codepoint
                            uint codePoint;
                            if (!(success = UInt32.TryParse(json.Substring(index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out codePoint)))
                                return string.Empty;

                            // convert the integer codepoint to a unicode char and add to string
                            s.Append(Char.ConvertFromUtf32((int)codePoint));
                            //s.Append((char)Convert.ToInt32(hex.ToString(), 16));
                            // skip 4 chars
                            index += 4;
                        }
                        else
                            break;
                    }
                }
                else
                    s.Append(c);
            }

            if (!complete)
            {
                success = false;
                return null;
            }

            success = true;
            return s.ToString();
        }

        #endregion

        #region IDataSerializer

        private List<DataSerializeTransaction> transactions = new List<DataSerializeTransaction>();

        protected void ClearTransactions(bool rollback)
        {
            if (rollback)
            {
                int rolledbackTransactionCount = 0;
                foreach (IDataSerializeTransaction t in transactions)
                    if (t != null && t.Rollback())
                        rolledbackTransactionCount++;

                if (rolledbackTransactionCount > 0)
                    Debug.LogWarningFormat("JsonStreamDataSerializer: rolled back {0} transactions due to ClearTransactions method call", rolledbackTransactionCount);
            }

            transactions.Clear();
        }

        protected bool Write(string name, object data)
        {
            string json;
            if (!Serialize(data, out json))
                return false;
            
            if (!jsonSaveDataTypes.Any() && jsonSaveData.Length == 0)
                Begin(null);

            if (!jsonSaveDataTypes.Any())
            {
                Debug.LogWarningFormat("JsonStreamDataSerializer: failed to add '{0}' property due to it has no begin", name);
                return false;
            }
            else if (jsonSaveDataTypes.Count() == 1 && !begin)
            {
                Debug.LogWarningFormat("JsonStreamDataSerializer: failed to add '{0}' property due to its parent is already finished", name);
                return false;
            }
            else
            {
                var v = jsonSaveDataTypes.Last();
                if (v.key == JSONValueType.Object)
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        Debug.LogWarning("JsonStreamDataSerializer: failed to add property with null name");
                        return false;
                    }

                    if (v.value > 0)
                        jsonSaveData.Append(Delimiter);

                    v.value = v.value + 1;

                    jsonSaveData.AppendFormat(AppendSerializableTemplate, name);
                    jsonSaveData.Append(json);

                    return true;
                }
                else if (v.key == JSONValueType.Array)
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        Debug.LogWarningFormat("JsonStreamDataSerializer: failed to add '{0}' property due to its parent is array", name);
                        return false;
                    }

                    if (v.value > 0)
                        jsonSaveData.Append(Delimiter);

                    v.value = v.value + 1;

                    jsonSaveData.Append(json);

                    return true;
                }

                Debug.LogWarningFormat("JsonStreamDataSerializer: failed to begin '{0}' object due to its parent type is '{1}'", name, v.key);
                return false;
            }
        }

        protected bool Write(string name, IDataSerializable data)
        {
            if (data == null)
            {
                Debug.LogWarningFormat("JsonStreamDataSerializer: failed to save data due to null '{0}' value", name);
                return false;
            }

            bool ok = true;
            
            if (!jsonSaveDataTypes.Any() && jsonSaveData.Length == 0)
                ok = Begin(null);
            
            if (ok)
            {
                if (data.IsArray)
                    ok = BeginList(name);
                else
                    ok = Begin(name);
            }

            if (!ok)
                return false;

            bool saved = data.Save(this);
            if (!saved)
                Debug.LogWarningFormat("JsonStreamDataSerializer: failed to save data due to '{0}' IDataSerializable.Save() failed", name);
            
            if (data.IsArray)
                ok = EndList();
            else
                ok = End();

            return saved && ok;
        }

        protected bool Write(string name, IDataSerializable[] dataList)
        {
            if (dataList == null)
            {
                Debug.LogWarningFormat("JsonStreamDataSerializer: failed to save data due to null '{0}' value", name);
                return false;
            }

            bool ok = true;
            
            if (!jsonSaveDataTypes.Any() && jsonSaveData.Length == 0)
                ok = Begin(null);

            if (ok)
                ok = BeginList(name);

            if (!ok)
                return false;

            bool saved = true;
            foreach (var data in dataList)
                if (data != null)
                {
                    if (data.IsArray)
                        ok = BeginList(null);
                    else
                        ok = Begin(null);

                    if (!ok)
                        break;

                    if (!data.Save(this))
                        saved = false;

                    if (data.IsArray)
                        ok = EndList();
                    else
                        ok = End();

                    if (!saved || !ok)
                        break;
                }

            if (!saved)
                Debug.LogWarningFormat("JsonStreamDataSerializer: failed to save data due to '{0}' IDataSerializable.Save() failed", name);

            ok = EndList() && ok;

            return saved && ok;
        }

        private bool CheckValue<T>(string name, string json, bool allowNull, ref T val, T defaultValue)
        {
            if (!allowNull || !string.Equals(json, "null"))
            { 
                Debug.LogWarning(string.Format("JsonStreamDataSerializer: failed to convert '{0}' property JSON data '{1}' to {2} value", name, json, typeof(T).Name));
                return false;
            }

            val = defaultValue;
            return true;
        }

        private bool CheckValue<T>(string name, string json, bool allowNull, ref T val)
        {
            if (!allowNull || !string.Equals(json, "null"))
            {
                Debug.LogWarning(string.Format("JsonStreamDataSerializer: failed to convert '{0}' property JSON data '{1}' to {2} value", name, json, typeof(T).Name));
                return false;
            }

            val = default(T);
            return true;
        }

        protected bool IsNull(string name)
        {
            bool res = true;
            string json = ReadJson(name, ref res);
            return res && json.Length == 4 && json.Equals("null");
        }
        
        protected bool ReadBool(string name, ref bool ok, bool allowNull, bool defaultValue)
        {
            bool res = true;
            string json = ReadJson(name, ref res);

            if (res)
            {
                bool v;
                res = bool.TryParse(json, out v);
                if (!res)
                    res = CheckValue<bool>(name, json, allowNull, ref v);

                ok = ok && res;
                return v;
            }
            
            ok = false;
            return defaultValue;
        }

        protected int ReadInt(string name, ref bool ok, bool allowNull, int defaultValue)
        {
            bool res = true;
            string json = ReadJson(name, ref res);

            if (res)
            {
                int v;
                res = int.TryParse(json, out v);
                if (!res)
                    res = CheckValue<int>(name, json, allowNull, ref v, defaultValue);

                ok = ok && res;
                return v;
            }

            ok = false;
            return defaultValue;
        }

        protected uint ReadUInt(string name, ref bool ok, bool allowNull)
        {
            bool res = true;
            string json = ReadJson(name, ref res);

            if (res)
            {
                uint v;
                res = uint.TryParse(json, out v);
                if (!res)
                    res = CheckValue<uint>(name, json, allowNull, ref v);

                ok = ok && res;
                return v;
            }

            ok = false;
            return default(uint);
        }

        protected long ReadLong(string name, ref bool ok, bool allowNull, long defaultValue)
        {
            bool res = true;
            string json = ReadJson(name, ref res);

            if (res)
            {
                long v;
                res = long.TryParse(json, out v);
                if (!res)
                    res = CheckValue<long>(name, json, allowNull, ref v, defaultValue);

                ok = ok && res;
                return v;
            }

            ok = false;
            return defaultValue;
        }

        protected ulong ReadULong(string name, ref bool ok, bool allowNull, ulong defaultValue)
        {
            bool res = true;
            string json = ReadJson(name, ref res);

            if (res)
            {
                ulong v;
                res = ulong.TryParse(json, out v);
                if (!res)
                    res = CheckValue<ulong>(name, json, allowNull, ref v);

                ok = ok && res;
                return v;
            }

            ok = false;
            return defaultValue;
        }

        protected float ReadFloat(string name, ref bool ok, bool allowNull, float defaultValue)
        {
            bool res = true;
            string json = ReadJson(name, ref res);

            if (res)
            {
                float v;
                res = float.TryParse(json, out v);
                if (!res)
                    res = CheckValue<float>(name, json, allowNull, ref v, defaultValue);

                ok = ok && res;
                return v;
            }

            ok = false;
            return defaultValue;
        }

        protected double ReadDouble(string name, ref bool ok, bool allowNull, double defaultValue)
        {
            bool res = true;
            string json = ReadJson(name, ref res);

            if (res)
            {
                double v;
                res = double.TryParse(json, out v);
                if (!res)
                    res = CheckValue<double>(name, json, allowNull, ref v, defaultValue);

                ok = ok && res;
                return v;
            }

            ok = false;
            return defaultValue;
        }

        protected DateTime? ReadDateTime(string name, string format, ref bool ok, bool allowNull, DateTime? defaultValue)
        {
            bool res = true;
            string json = ReadJson(name, ref res);

            if (res)
            {
                if (json.Length >= 2)
                {
                    string s = json.Substring(1, json.Length - 2);

                    DateTime dt;
                    res = DateTime.TryParseExact(s, format, null, System.Globalization.DateTimeStyles.None, out dt);
                    DateTime? v = dt;
                    if (!res)
                        res = CheckValue<DateTime?>(name, json, allowNull, ref v, defaultValue);

                    ok = ok && res;
                    return v;
                }

                Debug.LogWarning(string.Format("JsonStreamDataSerializer: failed to convert '{0}' property JSON data '{1}' due to converting to 'DateTime' with '{2}' format error", name, json, format));
            }

            ok = false;
            return defaultValue; 
        }

        protected string ReadString(string name, ref bool ok, bool allowNull)
        {
            bool res = true;
            string json = ReadJson(name, ref res);

            if (res)
            {
                if (json.Length == 4 && json.Equals("null"))
                {
                    if (allowNull)
                        return default(string);
                }
                else if (json.Length >= 2)
                {
                    //return json.Substring(1, json.Length - 2);
                    return ParseStringFromUnicode(json.Substring(1, json.Length - 2), ref ok);
                }

                Debug.LogWarning(string.Format("JsonStreamDataSerializer: failed to convert '{0}' property JSON data '{1}' due to converting to 'string' error", name, json));
            }

            ok = false;
            return default(string);
        }

        protected T Read<T>(string name, bool allowNull) where T : class
        {
            bool res = true;
            string json = ReadJson(name, ref res);

            if (res)
                return Deserialize<T>(name, json, allowNull);

            string e = jsonLoadData.Obj != null ? 
                string.Format("JsonStreamDataSerializer: failed to read data due to failed to find '{0}' key in '{1}'", name, Substring(jsonLoadData.Obj.ToString()))
                : string.Format("JsonStreamDataSerializer: failed to read data due to failed to find '{0}' key", name); 

            Debug.LogWarning(e);
            throw new JsonSerializationException(e);
            //return default(T);
        }

        public bool Read(string name, Func<IDataSerializer, bool> dataDelegate)
        {
            if (dataDelegate == null)
            {
                Debug.LogWarning(string.Format("JsonStreamDataSerializer: failed to read data due to '{0}' null value",
                    name));
                return false;
            }
            
            if (!GoInto(name))
                return false;

            bool loaded = dataDelegate(this);
            if (!loaded && !string.IsNullOrEmpty(name))
                Debug.LogWarning(string.Format("JsonStreamDataSerializer: failed to load '{0}' data", name));

            if (!GoOut())
                return false;
            
            return loaded;
        }

        protected bool GoInto(string name)
        {
            if (jsonLoadData == null || jsonLoadData.Obj == null)
            {
                Debug.LogWarning(
                    string.Format(
                        "JsonStreamDataSerializer: failed to read data due to parsing for '{0}' value failed", name));
                return false;
            }

            var jsonObject = jsonLoadData;

            if (!string.IsNullOrEmpty(name))
            {
                var obj = jsonObject.Obj[name];
                if (obj == null || (obj.Obj == null && obj.Array == null))
                {
                    if (obj == null)
                    {
                        Debug.LogWarning(
                            string.Format(
                                "JsonStreamDataSerializer: failed to read data due to failed to find '{0}' key in '{1}'",
                                name,
                                Substring(jsonLoadData.ToString())));
                    }
                    else
                    {
                        Debug.LogWarning(
                            string.Format(
                                "JsonStreamDataSerializer: failed to go read data due to value with '{0}' key is null in '{1}'",
                                name,
                                Substring(jsonLoadData.ToString())));
                    }

                    return false;
                }

                jsonObject = obj;
            }

            lastJsonLoadDataList.Add(jsonLoadData);
            jsonLoadData = jsonObject;

            return true;
        }

        protected bool GoOut()
        {
            if (jsonLoadData == null)
            {
                Debug.LogWarning("JsonStreamDataSerializer: failed to go out data due to no data");
                return false;
            }

            //var obj = jsonLoadData.Parent;
            var obj = lastJsonLoadDataList.LastOrDefault();
            if (obj != null)
                lastJsonLoadDataList.RemoveAt(lastJsonLoadDataList.Count - 1);

            if (obj == null || (obj.Obj == null && obj.Array == null))
            {
                if (obj == null)
                {
                    Debug.LogWarning(
                        string.Format(
                            "JsonStreamDataSerializer: failed to go out data '{0}' due to failed to find parent",
                            Substring(jsonLoadData.ToString())));
                }
                else
                {
                    Debug.LogWarning(
                        string.Format(
                            "JsonStreamDataSerializer: failed to go out data '{0}' due to parent is null",
                            Substring(jsonLoadData.ToString())));
                }

                return false;
            }

            jsonLoadData = obj;

            return true;
        }

        protected bool Read(int index, IReadDataSerializable data)
        {
            if (data == null)
            {
                Debug.LogWarning(string.Format("JsonStreamDataSerializer: failed to read data[{0}] due to null value",
                    index));
                return false;
            }

            return Read(index, data.Load);
        }

        protected bool Read(int index, Func<IDataSerializer, bool> dataDelegate)
        {
            if (dataDelegate == null)
            {
                Debug.LogWarning(string.Format("JsonStreamDataSerializer: failed to read data[{0}] due to null value",
                    index));
                return false;
            }
            
            if (!GoInto(index))
                return false;

            bool loaded = dataDelegate(this);
            if (!loaded)
                Debug.LogWarning(string.Format("JsonStreamDataSerializer: failed to load data[{0}] in '{1}...'", index, Substring(jsonLoadData.ToString())));

            if (!GoOut())
                return false;

            return loaded;
        }

        protected bool GoInto(int index)
        {
            if (jsonLoadData == null)
            {
                Debug.LogWarning(
                    string.Format(
                        "JsonStreamDataSerializer: failed to read data[{0}] due to parsing failed in '{1}...'", index, Substring(jsonLoadData.ToString())));
                return false;
            }

            var array = jsonLoadData.Array;
            if (array == null || index < 0 || index >= array.Length)
            {
                Debug.LogWarning(
                    string.Format(
                        "JsonStreamDataSerializer: failed to read data[{0}] due to invalid array or index in '{1}...'", index, Substring(jsonLoadData.ToString())));
                return false;
            }

            var obj = array[index];
            if (obj == null || (obj.Obj == null && obj.Array == null))
            {
                Debug.LogWarning(
                    string.Format(
                        "JsonStreamDataSerializer: failed to read data due to failed to find [{0}] value in '{1}'",
                        index, Substring(jsonLoadData.ToString())));

                return false;
            }

            lastJsonLoadDataList.Add(jsonLoadData);
            jsonLoadData = obj;

            return true;
        }

        protected int ArrayLength
        {
            get
            {
                if (jsonLoadData == null)
                {
                    Debug.LogWarning("JsonStreamDataSerializer: failed to get array length due to parsing error");
                    return -1;
                }

                var array = jsonLoadData.Array;
                if (array == null)
                {
                    Debug.LogWarning(
                        string.Format(
                            "JsonStreamDataSerializer: failed to get array length due to invalid array in '{0}'",
                            Substring(jsonLoadData.ToString())));
                    return -1;
                }

                return array.Length;
            }
        }

        protected IEnumerable<string> Keys
        {
            get
            {
                if (jsonLoadData == null || jsonLoadData.Obj == null)
                {
                    Debug.LogWarning("JsonStreamDataSerializer: failed to read keys due to parsing error");
                    return null;
                }

                return jsonLoadData.Obj.Keys;
            }
        }

        protected bool ReadJsonFromStream(StreamReader reader, out string json)
        {
            if (reader.BaseStream == null || !reader.BaseStream.CanRead)
            {
                Debug.LogWarning("JsonStreamDataSerializer: failed to read json due to StreamReader is closed");
                json = null;
                return false;
            }
            
            json = reader.ReadToEnd();

            //reader.Close();

            return true;
        }

        protected bool BeginReadingFromStream(StreamReader reader) //, Encoding.Unicode)
        {
            bool ok = false;
            if (reader != null)
            {
                string json;
                if (!ReadJsonFromStream(reader, out json))
                    return false;

                return BeginReadingFromString(json);
            }
            else
                Debug.LogWarning("JsonStreamDataSerializer: failed to read json");

            return ok;
        }

        protected bool BeginReadingFromString(string json) //, Encoding.Unicode)
        {
            Reset();

            bool ok = false;

            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("JsonStreamDataSerializer: failed to read json due to empty json");
                return false;
            }

            var sp = JSONObject.SkipWhitespace(json, 0);

            jsonLoadData = null;
            if (sp < json.Length && json[sp] == '[')
            {
                var jo = JSONArray.Parse(json);
                if (jo != null)
                    jsonLoadData = new JSONValue(jo);
            }
            else
            {
                var jo = JSONObject.Parse(json);
                if (jo != null)
                    jsonLoadData = new JSONValue(jo);
            }

            ok = jsonLoadData != null;

            //#if DEBUG
            if (jsonLoadData == null)
                Debug.LogWarning(
                    string.Format("JsonStreamDataSerializer: failed to read data due to failed to parse '{0}'",
                        Substring(jsonLoadData != null ? jsonLoadData.ToString() : json)));
            //#endif

            return ok;
        }

        private void EndReadingFromStream()
        {
            Reset();
        }

        protected bool EndWritingToStream(StreamWriter writer)
        {
            if (writer != null)
            {
                string json;
                bool ok = EndWritingToString(out json);
                writer.Write(json);
                
                return ok;
            }
            
            Reset();
            return false;
        }

        protected bool EndWritingToString(out string json)
        {
            if (jsonSaveDataTypes.Count != 1 || begin && !End())
            {
                if (jsonSaveDataTypes.Count < 1)
                    Debug.LogWarning("JsonStreamDataSerializer: failed to end writing data due to data has no begin");
                else
                    Debug.LogWarning("JsonStreamDataSerializer: failed to end writing data due to data in not ended");

                json = null;
                Reset();
                
                return false;
            }

            json = jsonSaveData.ToString();
            Reset();
            
            return true;
        }

        //------------------------------------------------------------------------

        public IDataSerializeTransaction BeginTransaction()
        {
            var t = new DataSerializeTransaction();
            transactions.Add(t);

            return t;
        }

        bool IDataSerializeTransactionHolder.AddTrSuccessHandler(Action onSuccess)
        {
            DataSerializeTransaction t = null;
            if (transactions.Any())
                t = transactions.Last();

            if (t == null)
            {
                Debug.LogWarning("JsonStreamDataSerializer: failed to add callback on transaction success due to null transaction");
                return false;
            }

            return t.AddCommitCallback(onSuccess);
        }

        bool IDataSerializeTransactionHolder.AddTrFailHandler(Action onFail)
        {
            DataSerializeTransaction t = null;
            if (transactions.Any())
                t = transactions.Last();

            if (t == null)
            {
                Debug.LogWarning("JsonStreamDataSerializer: failed to add callback on transaction fail due to null transaction");
                return false;
            }

            return t.AddRollbackCallback(onFail);
        }

        bool IDataSerializeTransactionHolder.AddTrCommittedHandler(Action onCommitted)
        {
            DataSerializeTransaction t = null;
            if (transactions.Any())
                t = transactions.Last();
			
            if (t == null)
            {
                Debug.LogWarning("JsonStreamDataSerializer: failed to add callback on transaction commited due to null transaction");
                return false;
            }

            return t.AddCommittedCallback(onCommitted);
        }

        bool IDataSerializeTransactionHolder.AddTrRollbackedHandler(Action onRollbacked)
        {
            DataSerializeTransaction t = null;
            if (transactions.Any())
                t = transactions.Last();

            if (t == null)
            {
                Debug.LogWarning("JsonStreamDataSerializer: failed to add callback on transaction rollbacked due to null transaction");
                return false;
            }

            return t.AddRollbackedCallback(onRollbacked);
        }

        bool IDataSerializer.Write(string name, object data)
        {
            return Write(name, data);
        }

        bool IDataSerializer.Write(string name, IDataSerializable data)
        {
            return Write(name, data);
        }

        bool IDataSerializer.Write(string name, IDataSerializable[] dataList)
        {
            return Write(name, dataList);
        }
        
        bool IDataSerializer.Write(string name, IEnumerable<IDataSerializable> dataList)
        {
            return Write(name, dataList);
        }

        bool IReadDataSerializer.ReadBool(string name, ref bool ok, bool allowNull, bool defaultValue)
        {
            return ReadBool(name, ref ok, allowNull, defaultValue);
        }

        int IReadDataSerializer.ReadInt(string name, ref bool ok, bool allowNull, int defaultValue)
        {
            return ReadInt(name, ref ok, allowNull, defaultValue);
        }

        uint IReadDataSerializer.ReadUInt(string name, ref bool ok, bool allowNull)
        {
            return ReadUInt(name, ref ok, allowNull);
        }

        long IReadDataSerializer.ReadLong(string name, ref bool ok, bool allowNull, long defaultValue)
        {
            return ReadLong(name, ref ok, allowNull, defaultValue);
        }

        bool IReadDataSerializer.IsNull(string name)
        {
            return IsNull(name);
        }

        ulong IReadDataSerializer.ReadULong(string name, ref bool ok, bool allowNull, ulong defaultValue)
        {
            return ReadULong(name, ref ok, allowNull, defaultValue);
        }

        float IReadDataSerializer.ReadFloat(string name, ref bool ok, bool allowNull, float defaultValue)
        {
            return ReadFloat(name, ref ok, allowNull, defaultValue);
        }

        double IReadDataSerializer.ReadDouble(string name, ref bool ok, bool allowNull, double defaultValue)
        {
            return ReadDouble(name, ref ok, allowNull, defaultValue);
        }

        DateTime? IReadDataSerializer.ReadDateTime(string name, string format, ref bool ok, bool allowNull, DateTime? defaultValue)
        {
            return ReadDateTime(name, format, ref ok, allowNull, defaultValue);
        }

        string IReadDataSerializer.ReadString(string name, ref bool ok, bool allowNull)
        {
            return ReadString(name, ref ok, allowNull);
        }

        T IReadDataSerializer.Read<T>(string name, bool allowNull)
        {
            return Read<T>(name, allowNull);
        }

        bool IReadDataSerializer.Read(int index, IReadDataSerializable data)
        {
            return Read(index, data);
        }

        int IReadDataSerializer.ArrayLength
        {
            get { return ArrayLength; }
        }

        IEnumerable<string> IReadDataSerializer.Keys
        {
            get { return Keys; }
        }

        bool IReadDataSerializer.Read(string name, IReadDataSerializable data)
        {
            if (data == null)
            {
                Debug.LogWarning(string.Format("JsonStreamDataSerializer: failed to read data due to '{0}' null value", name));
                return false;
            }

            bool loaded = Read(name, data.Load);
            if (!loaded)
                Debug.LogWarning(string.Format("JsonStreamDataSerializer: failed to load '{0}' data due to '{1}' load failed", name, data));

            return loaded;
        }

        bool IReadDataSerializer.Read(string name, IReadDataSerializable[] data)
        {
            return Read(name, data);
        }
        
        public bool Read(string name, IEnumerable<IReadDataSerializable> data)
        {
            if (data == null)
            {
                Debug.LogWarning(string.Format("JsonStreamDataSerializer: failed to read data due to '{0}' null value", name));
                return false;
            }

            bool isErrorShown = false;
            
            bool loaded = Read(name, s =>
            {
                int l = s.ArrayLength;
                if (l < 0)
                    return false;

                if (data.Count() != l)
                {
                    Debug.LogWarning(string.Format("JsonStreamDataSerializer: failed to load '{0}' data due to '{1}' array has invalid length", name, data));
                    isErrorShown = true;
                    return false;
                }

                for (var i = 0; i < data.Count(); i++)
                {
                    if (data.ElementAt(i) == null || !s.Read(i, data.ElementAt(i)))
                    {
                        Debug.LogWarning(string.Format("JsonStreamDataSerializer: failed to load '{0}' data due to '{1}' load failed at '{2}' index", name, data, i));
                        isErrorShown = true;
                        return false;
                    }
                }

                return true;
            });
            
            if (!loaded && !isErrorShown)
                Debug.LogWarning(string.Format("JsonStreamDataSerializer: failed to load '{0}' data due to '{1}' load failed", name, data));

            return loaded;
        }

        #endregion

        #region IControlledDataSerializer

        public virtual bool BeginReading(StreamReader reader)
        {
            return BeginReadingFromStream(reader);
        }
        
        public virtual bool EndWriting(StreamWriter writer)
        {
            return EndWritingToStream(writer);
        }

        #endregion
    }
    
    public class JsonSerializationException : InvalidOperationException
    {
        public JsonSerializationException() : base() { }

        public JsonSerializationException(string message) : base(message) { }

        public JsonSerializationException(string message, Exception innerException) : base(message, innerException) { }

        public JsonSerializationException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
    
    [Serializable]
    public class SerializableKeyValuePair<T1, T2>
    {
        public T1 key;
        public T2 value;
        
        public SerializableKeyValuePair()
            : this(default(T1), default(T2))
        {
        }

        public SerializableKeyValuePair(T1 key, T2 value)
        {
            this.key = key;
            this.value = value;
        }
    }
}