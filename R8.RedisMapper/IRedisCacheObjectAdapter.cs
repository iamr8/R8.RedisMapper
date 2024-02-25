using System;
using System.Collections.Immutable;
using StackExchange.Redis;

namespace R8.RedisMapper
{
    /// <summary>
    /// A base class for Redis Cached Model.
    /// </summary>
    /// <typeparam name="TModel">A type of model.</typeparam>
    public interface IRedisCacheObjectAdapter<in TModel>
    {
        /// <summary>
        /// Gets a list of properties of the model.
        /// </summary>
        ImmutableArray<string> Properties { get; }
        
        /// <summary>
        /// Sets a value to the model according to the property name.
        /// </summary>
        /// <param name="model">An instance of <typeparamref name="TModel"/>.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value to be set.</param>
        /// <returns>A boolean value indicating whether the value has been set or not.</returns>
        bool SetValue(TModel model, string propertyName, RedisValue value);
        
        /// <summary>
        /// Returns the value of the property from the model.
        /// </summary>
        /// <param name="model">An instance of <typeparamref name="TModel"/>.</param>
        /// <param name="propertyNme">The name of the property.</param>
        /// <returns>A <see cref="RedisValue"/> representing the value of the property.</returns>
        RedisValue GetValue(TModel model, string propertyNme);
    }
}