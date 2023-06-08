namespace R8.RedisHelper.Models
{
    internal class RedisReaderTask<T> : RedisReaderTaskBase
    {
        public Func<RedisValue[], RedisCache<T>> PostAction { get; set; }
    }
}