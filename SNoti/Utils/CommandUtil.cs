using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SNotiSSL.Model;

namespace SNotiSSL.Utils
{
    public static class CommandUtil
    {
        public static SNotiCommandType GetCommandType(string key)
        {
            return SNotiCommandType.CommandsDic.TryGetValue(key, out SNotiCommandType cmdType) ? cmdType : null;
        }

        public static bool ContainAllKeys<TJsonObject>(this TJsonObject root, params string[] keys) where TJsonObject : JObject
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (!root.ContainsKey(keys[i])) return false;
                if (i <= keys.Length - 2)
                {
                    if (root[keys[i]].Type != JTokenType.Object) return false;
                    root = root[keys[i]].Value<TJsonObject>();
                }
            }
            return true;
        }

        public static TValue TryGetValueOrDefault<TKey,TValue>(this Dictionary<TKey,TValue> dic,TKey key)
        {
            return (dic.TryGetValue(key,out var value)) ? value : (TValue) GetDefaultValue(typeof(TValue));
        }
        private static object GetDefaultValue(Type type)
        {
            if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}