using System;
using System.Text.Json;

namespace R8.RedisMapper
{
    /// <summary>
    /// A context that is used when reading the value.
    /// </summary>
    public sealed class RedisValueReaderContext : IRedisValueContext
    {
        public RedisValueReaderContext(Type returnType)
        {
            var underlyingType = Nullable.GetUnderlyingType(returnType);
            this.IsNullable = underlyingType != null;
            this.ReturnType = underlyingType ?? returnType;
            
            IgnoreDefaultValue = Configuration.IgnoreDefaultValues;
            SerializerOptions = Configuration.SerializerOptions;
        }
        
        internal RedisValueReaderContext(IRedisValueContext internalContext, Type returnType) : this(returnType)
        {
            this.IgnoreDefaultValue = internalContext.IgnoreDefaultValue;
            this.SerializerOptions = internalContext.SerializerOptions;
        }

        public bool IgnoreDefaultValue { get; set; }
        public JsonSerializerOptions SerializerOptions { get; set; }
        public Type ReturnType { get; }
        public bool IsNullable { get; }
    }
}