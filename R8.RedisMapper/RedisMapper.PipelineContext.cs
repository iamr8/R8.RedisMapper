using System.Collections.Generic;
using System.Text.Json;

namespace R8.RedisMapper
{
    public abstract class PipelineContextBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether to ignore the default value of a property.
        /// When set to true, the property's default value will be ignored by the program to be cached. Default: The value set in <see cref="Configuration.IgnoreDefaultValues"/>.
        /// </summary>
        public bool? IgnoreDefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the options for the JSON serializer. Default: The serializer set in <see cref="Configuration.SerializerOptions"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="SerializerOptions"/> property allows customizing the behavior of the JSON serializer.
        /// Use this property to set options such as naming policy, default value handling, ignore null values, and more.
        /// </remarks>
        public JsonSerializerOptions SerializerOptions { get; set; }

        /// <summary>
        /// Gets or sets the Redis Value Formatter. Default: The formatter set in <see cref="Configuration.FieldFormatter"/>.
        /// </summary>
        public RedisFieldFormatter FieldFormatter { get; set; }

        /// <summary>
        /// Gets or sets the Redis Value Formatters. Default: The formatters set in <see cref="Configuration.ValueFormatters"/>.
        /// </summary>
        public IList<IRedisValueFormatter> ValueFormatters { get; set; } = new List<IRedisValueFormatter>();
    }


    public class PipelineContext<T> : PipelineContextBase
    {
        /// <summary>
        /// Gets or sets the specific properties of the <see cref="T"/> to be retrieved from the cache.
        /// </summary>
        /// <value>
        /// The properties of the <see cref="T"/>.
        /// </value>
        /// <remarks>This property is only used when <see cref="AllProperties"/> is set to <c>false</c>.</remarks>
        public PipelineContextPropertyCollection<T> Properties { get; } = new PipelineContextPropertyCollection<T>();

        /// <summary>
        /// Gets or sets a value indicating whether all properties should be included. Default value is <c>true</c>.
        /// </summary>
        /// <value>
        /// <c>true</c> if all properties should be included; otherwise, <c>false</c>.
        /// </value>
        public bool AllProperties { get; set; } = true;
    }

    public class PipelineContext : PipelineContextBase
    {
        /// <summary>
        /// Gets or sets the list of field names.
        /// </summary>
        /// <value>
        /// The list of field names.
        /// </value>
        public IList<string> Fields { get; set; } = new List<string>();
    }
}