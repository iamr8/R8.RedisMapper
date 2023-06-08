namespace R8.RedisHelper.Models
{
    internal class RedisReaderTask : RedisReaderTaskBase
    {
        public Func<RedisValue[], RedisCache> PostAction { get; set; }
    }
}