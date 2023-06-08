using FluentAssertions;
using NSubstitute;
using R8.RedisHelper.Handlers;
using R8.RedisHelper.Models;
using R8.RedisHelper.Utils;
using StackExchange.Redis;

namespace R8.RedisHelper.Tests
{
    public class RedisBatchReaderTests
    {
        private readonly IDatabaseAsync _database = Substitute.For<IDatabaseAsync>();
        private RedisBatchReader<FakeRedisTestModel> _sut;
        private RedisBatchReader<FakeRedisTestModel> Sut => _sut ??= new RedisBatchReader<FakeRedisTestModel>(_database);

        [Fact]
        public async Task Get_method_should_call_dataBase_HashGetAsync_method_with_correct_fields()
        {
            var redisKey = RedisCacheKey.Create("test");
            var fields = new[] { "Field1", "Field2", "Field3", "Field5" };
            RedisValue[] capturedRedisValue = null;
            RedisKey capturedRedisKey;
            await _database.HashGetAsync(Arg.Do<RedisKey>(arg => capturedRedisKey = arg), Arg.Do<RedisValue[]>(arg => capturedRedisValue = arg));


            Sut.Get(redisKey, fields);
            await Sut.Readers[0].ExecuteAsync();


            Sut.Readers.Count.Should().Be(1);
            await _database.Received(1).HashGetAsync(new RedisKey(redisKey), Arg.Any<RedisValue[]>());

            capturedRedisKey.Should().Be(new RedisKey(redisKey));
            capturedRedisValue.Should().AllSatisfy(x => { fields.Select(x => x.ToCamelCase()).Should().Contain(x.ToString()); });
        }

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        public async Task N_times_calling_Get_method_causes__n_times_dataBase_HashGetAsync_method_call(int times)
        {
            var redisKey = RedisCacheKey.Create("test");
            var executingTimes = times - 1;
            var fields = new[] { "Field1", "Field2", "Field3", "Field5" };


            for (var i = 0; i < times; i++)
                Sut.Get(redisKey, fields);
            for (var i = 0; i < executingTimes; i++)
                await Sut.Readers[i].ExecuteAsync();


            Sut.Readers.Count.Should().Be(times);
            await _database.Received(executingTimes).HashGetAsync(new RedisKey(redisKey), Arg.Any<RedisValue[]>());
        }
    }
}