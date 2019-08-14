using System;
using Newtonsoft.Json;

namespace SNotiSSL.Model
{
    public class CommonBody
    {
        [JsonProperty("cmd")]
        [JsonConverter(typeof(SNotiCommandTypeConverter))]
        public SNotiCommandType cmd { get; set; }
    }

    public class SNotiCommandTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            bool NotNullable = objectType.IsValueType && Nullable.GetUnderlyingType(objectType) == null;
            if (reader.TokenType == JsonToken.Null)
            {
                if (NotNullable) throw new Exception(string.Format("Cannot convert null value to {0}.", objectType));
                return null;
            }
            try
            {
                if (reader.TokenType == JsonToken.String)
                {
                    return new SNotiCommandType(reader.Value.ToString(), "");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                throw new Exception(string.Format("Error when converting null value {0} to {1}.", reader.Value, objectType));
            }
            throw new Exception(string.Format("Unexpected token {0} when parsing SNotiCommandType", reader.TokenType));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if(value == null)
            {
                writer.WriteNull();
            }
            if(value is SNotiCommandType)
            {
                var TypeVal = (SNotiCommandType) value;
                writer.WriteValue(TypeVal.Cmd);
            }
            else writer.WriteNull();
        }
    }
}