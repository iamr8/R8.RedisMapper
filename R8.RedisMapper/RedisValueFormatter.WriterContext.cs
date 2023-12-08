using System;
using System.Text.Json;

namespace R8.RedisMapper
{
    /// <summary>
    /// A context that is used when writing the value.
    /// </summary>
    public sealed class RedisValueWriterContext : IRedisValueContext
    {
        internal RedisValueWriterContext()
        {
        }
        
        internal RedisValueWriterContext(IRedisValueContext internalContext, Type dataType)
        {
            this.IgnoreDefaultValue = internalContext.IgnoreDefaultValue;
            this.SerializerOptions = internalContext.SerializerOptions;
            
            var underlyingType = Nullable.GetUnderlyingType(dataType);
            this.DataType = underlyingType ?? dataType;
        }

        /// <summary>
        /// Gets a value indicating whether to ignore default value according to the <see cref="DataType"/> property.
        /// </summary>
        public bool IgnoreDefaultValue { get; internal set; }
        
        /// <summary>
        /// Gets the <see cref="JsonSerializerOptions"/> that is used when need to serialize the value.
        /// </summary>
        public JsonSerializerOptions SerializerOptions { get; internal set; }
        
        /// <summary>
        /// Gets the type of the value.
        /// </summary>
        public Type DataType { get; internal set; }
    }
}