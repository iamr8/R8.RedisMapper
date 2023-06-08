namespace R8.RedisHelper.Interfaces
{
    public interface IRedisWriter : IRedisOperation
    {
        Task ExecuteAsync();
        TResult GetResult<TResult>() where TResult : unmanaged;
    }
}