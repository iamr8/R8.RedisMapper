namespace R8.RedisHelper;

public class RedisHelperOptions
{
    /// <summary>
    /// Gets or sets the Redis Database Id.
    /// </summary>
    public int DatabaseId { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets the Redis Client Configuration.
    /// </summary>
    public ConfigurationOptions Configurations { get; set; }
}