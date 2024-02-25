namespace R8.RedisMapper.ConsoleSample;

public partial class UserCacheModel : IRedisCacheObject
{
    public int UserId { get; set; }
    public string Username { get; set; }
    
    [RedisFormatter(typeof(DateTimeValueSerializer))]
    public DateTime CreatedAt { get; set; }

    public bool IsAdmin { get; set; }
}