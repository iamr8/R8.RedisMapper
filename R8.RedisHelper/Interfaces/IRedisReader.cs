namespace R8.RedisHelper.Interfaces
{
    public interface IRedisReader : IRedisOperation
    {
        Task ExecuteAsync();
        RedisValue[] GetResult();
    }
}