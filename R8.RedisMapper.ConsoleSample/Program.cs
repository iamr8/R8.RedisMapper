using StackExchange.Redis;

namespace R8.RedisMapper.ConsoleSample
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var options = new ConfigurationOptions();
            options.EndPoints.Add("localhost", 6379);
            var connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(options);
            var database = connectionMultiplexer.GetDatabase();
            
            // Must Register Adapters
            // Must Register ValueSerializers
            
            // var cached = await database.HashGetAsync<TestCacheModel>("test:1");
            //
            // if (cached == null)
            // {
            //     await database.HashSetAsync("test:1", new TestCacheModel
            //     {
            //         Id = 1,
            //         ExtId = "123",
            //         Username = "Test"
            //     });
            // }
            //
            // await database.Fluently<TestCacheModel>()
            //     .ById((long)999)
            //     .AsHashSet()
            //     .Select(x=> new
            //     {
            //         Id = x.Id
            //     })
            //     .FindAsync();
            // new UserCacheModel().SetValue(new UserCacheModel(), nameof(UserCacheModel.UserId), 10);
        }
    }
}