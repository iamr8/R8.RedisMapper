using NSubstitute;
using StackExchange.Redis;

namespace R8.RedisMapper.Tests
{
    internal static class Utils
    {
        public static (IConnectionMultiplexer Multiplexer, IDatabase Database, IBatch Batch) CreateMock()
        {
            var batch = Substitute.For<IBatch>();

            var database = Substitute.For<IDatabase>();
            database.CreateBatch().Returns(batch);

            var connectionMultiplexer = Substitute.For<IConnectionMultiplexer>();
            connectionMultiplexer.GetDatabase(Arg.Any<int>()).Returns(args =>
            {
                var dbId = args.Arg<int>();
                database.Database.Returns(dbId);
                return database;
            });
            
            return (connectionMultiplexer, database, batch);
        }
    }
}