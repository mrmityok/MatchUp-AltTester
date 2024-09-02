using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Serialization
{
  public sealed class JsonFileDataSerializer : AstractJsonStreamDataSerializer//<string, string>
  {
      ~JsonFileDataSerializer()
      {
          ClearTransactions(true);
      }

      public bool BeginReading(string path)
      {
          bool ok = false;
          try
          {
              using (var reader = new StreamReader(path))//, Encoding.Unicode))
              {
                  ok = BeginReadingFromStream(reader);
                  reader.Close();
              }
          }
          catch (Exception e)
          {
              Debug.LogWarning(string.Format("JsonFileDataSerializer failed to load due to '{0}'", e.Message));
          }

          return ok;
      }

      public bool EndWriting(string path)
      {
          bool ok = false;
          try
          {
              using (var writer = new StreamWriter(path, false, Encoding.Unicode))
              {
                  ok = EndWritingToStream(writer);
                  writer.Close();
              }
          }
          catch (Exception e)
          {
              Debug.LogWarning(string.Format("JsonFileDataSerializer failed to write due to '{0}'", e.Message));
          }

          return ok;
      }
      
    }
}