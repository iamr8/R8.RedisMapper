using FluentAssertions;
using NSubstitute;
using R8.RedisHelper.Handlers;
using R8.RedisHelper.Utils;
using StackExchange.Redis;

namespace R8.RedisHelper.Tests;

public class RedisBatchWriterTests
{
    private readonly IDatabaseAsync _database = Substitute.For<IDatabaseAsync>();
    private CommandFlags _flag;
    private RedisBatchWriter _sut;
    private RedisBatchWriter Sut => _sut ??= new RedisBatchWriter(_database, _flag);

    [Theory]
    [InlineData(CommandFlags.DemandMaster, "field2", When.Always)]
    [InlineData(CommandFlags.DemandReplica, "field3", When.Exists)]
    [InlineData(CommandFlags.FireAndForget, "field5", When.NotExists)]
    public async Task Get_method_should_call_dataBase_HashSetAsync_method_with_correct_fields(CommandFlags flag, string fieldName, When when)
    {
        var redisKey = new RedisKey("test");
        var value = new FakeRedisTestModel()
        {
            field1 = "f1",
            field2 = "f2",
            field3 = string.Empty,
            field4 = "not store",
            field5 = "f5"
        };
        _flag = flag;


        Sut.Set(redisKey, fieldName, value, when);
        await Sut.Writers[0].ExecuteAsync();


        Sut.Writers.Count.Should().Be(1);
        await _database.Received(1).HashSetAsync(redisKey, new RedisValue(fieldName),
             Arg.Is<RedisValue>(s => s.IsTheSameAs(RedisValue.Unbox(value.ToOptimizedObject()))),
              when,flag);
    }

    [Theory]
    [InlineData(CommandFlags.DemandMaster, "field2", When.Always, 3)]
    [InlineData(CommandFlags.FireAndForget, "field1", When.Exists, 5)]
    public async Task N_times_calling_Set_method_with_fieldName_causes_n_times_dataBase_HashSetAsync_method_call(CommandFlags flag, string fieldName, When when, int times)
    {
        _flag = flag;
        var redisKey = new RedisKey("test");
        var executingTimes = times - 1;
        var value = new FakeRedisTestModel()
        {
            field1 = "f1",
            field2 = "f2",
            field3 = string.Empty,
            field4 = "not store",
            field5 = "f5"
        };


        for (var i = 0; i < times; i++)
            Sut.Set(redisKey, fieldName, value, when);
        for (var i = 0; i < executingTimes; i++)
            await Sut.Writers[i].ExecuteAsync();


        Sut.Writers.Count.Should().Be(times);
        await _database.Received(executingTimes).HashSetAsync(redisKey,new RedisValue(fieldName),
            Arg.Is<RedisValue>(s => s.IsTheSameAs(RedisValue.Unbox(value.ToOptimizedObject()))),
            when,flag);
    }

    [Theory]
    [InlineData(CommandFlags.DemandMaster, 3)]
    [InlineData(CommandFlags.DemandReplica, 5)]
    public async Task N_times_calling_object_Set_method_causes_n_times_dataBase_HashSetAsync_method_call(CommandFlags flag, int times)
    {
        _flag = flag;
        var redisKey = new RedisKey("test");
        var executingTimes = times - 1;
        var value = new FakeRedisTestModel()
        {
            field1 = "f1",
            field2 = "f2",
            field3 = String.Empty,
            field4 = "not store",
            field5 = "f5"
        }.ToOptimizedDictionary();

        var hashFields = value
            .Select(field => new HashEntry(new RedisValue(field.Key), RedisValue.Unbox(field.Value)))
            .ToArray();


        for (var i = 0; i < times; i++)
            Sut.Set(redisKey, value);
        for (var i = 0; i < executingTimes; i++)
            await Sut.Writers[i].ExecuteAsync();


        Sut.Writers.Count.Should().Be(times);
        await _database.Received(executingTimes).HashSetAsync(redisKey,
            Arg.Is<HashEntry[]>(s => s.IsTheSameAs(hashFields)),
            flag);
    }

    [Theory]
    [InlineData(CommandFlags.DemandMaster, 3)]
    [InlineData(CommandFlags.DemandReplica, 5)]
    public async Task N_times_calling_class_Set_method_causes_n_times_dataBase_HashSetAsync_method_call(CommandFlags flag, int times)
    {
        _flag = flag;
        var redisKey = new RedisKey("test");
        var executingTimes = times - 1;
        var value = new FakeRedisTestModel()
        {
            field1 = "f1",
            field2 = "f2",
            field3 = string.Empty,
            field4 = "not store",
            field5 = "f5"
        };
        var hashFields = value.ToOptimizedDictionary()
            .Select(field => new HashEntry(new RedisValue(field.Key), RedisValue.Unbox(field.Value)))
            .ToArray();


        for (var i = 0; i < times; i++)
            Sut.Set(redisKey, value);
        for (var i = 0; i < executingTimes; i++)
            await Sut.Writers[i].ExecuteAsync();


        Sut.Writers.Count.Should().Be(times);
        await _database.Received(executingTimes).HashSetAsync(redisKey,
            Arg.Is<HashEntry[]>(s => s.IsTheSameAs(hashFields)),
            flag);
    }

    [Theory]
    [InlineData(CommandFlags.DemandMaster, 3)]
    [InlineData(CommandFlags.DemandReplica, 5)]
    public async Task N_times_calling_Delete_method_causes_n_times_dataBase_KeyDeleteAsync_method_call(CommandFlags flag, int times)
    {
        _flag = flag;
        var redisKey = new RedisKey("test");
        var executingTimes = times - 1;


        for (var i = 0; i < times; i++)
            Sut.Delete(redisKey);
        for (var i = 0; i < executingTimes; i++)
            await Sut.Writers[i].ExecuteAsync();


        Sut.Writers.Count.Should().Be(times);
        await _database.Received(executingTimes).KeyDeleteAsync(redisKey,flag);
    }

    [Theory]
    [InlineData(CommandFlags.DemandMaster, 3, "field1")]
    [InlineData(CommandFlags.DemandReplica, 5, "field2")]
    public async Task N_times_calling_Delete_method_with_fieldName_causes_n_times_dataBase_HashDeleteAsync_method_call(CommandFlags flag, int times, string fieldName)
    {
        _flag = flag;
        var redisKey = new RedisKey("test");
        var executingTimes = times - 1;


        for (var i = 0; i < times; i++)
            Sut.Delete(redisKey, fieldName);
        for (var i = 0; i < executingTimes; i++)
            await Sut.Writers[i].ExecuteAsync();


        Sut.Writers.Count.Should().Be(times);
        await _database.Received(executingTimes).HashDeleteAsync(redisKey,new RedisValue(fieldName),flag);
    }

    [Theory]
    [InlineData(CommandFlags.DemandMaster, 3, 2)]
    [InlineData(CommandFlags.DemandReplica, 5, 5)]
    public async Task N_times_calling_Increment_method_causes_n_times_dataBase_StringIncrementAsync_method_call(CommandFlags flag, int times, int value)
    {
        _flag = flag;
        var redisKey = new RedisKey("test");
        var executingTimes = times - 1;


        for (var i = 0; i < times; i++)
            Sut.Increment(redisKey, value);
        for (var i = 0; i < executingTimes; i++)
            await Sut.Writers[i].ExecuteAsync();


        Sut.Writers.Count.Should().Be(times);
        await _database.Received(executingTimes).StringIncrementAsync( redisKey, value,flag);
    }

    [Theory]
    [InlineData(CommandFlags.DemandMaster, 3, 2, "field1")]
    [InlineData(CommandFlags.DemandReplica, 5, 5, "field2")]
    public async Task N_times_calling_Increment_method_with_fieldName_causes_n_times_dataBase_HashIncrementAsync_method_call(CommandFlags flag, int times, int value, string fieldName)
    {
        _flag = flag;
        var redisKey = new RedisKey("test");
        var executingTimes = times - 1;


        for (var i = 0; i < times; i++)
            Sut.Increment(redisKey, fieldName, value);
        for (var i = 0; i < executingTimes; i++)
            await Sut.Writers[i].ExecuteAsync();


        Sut.Writers.Count.Should().Be(times);
        await _database.Received(executingTimes).HashIncrementAsync(redisKey,new RedisValue(fieldName),value,flag);
    }

    [Theory]
    [InlineData(CommandFlags.DemandMaster, 3, "10:00:04:03")]
    [InlineData(CommandFlags.DemandReplica, 5, "00:03:24:33")]
    public async Task N_times_calling_Increment_method_with_fieldName_causes_n_times_dataBase_KeyExpireAsync_method_call(CommandFlags flag, int times, string time)
    {
        _flag = flag;
        var redisKey = new RedisKey("test");
        var executingTimes = times - 1;
        var expectedValue = TimeSpan.Parse(time);


        for (var i = 0; i < times; i++)
            Sut.Expire(redisKey, expectedValue);
        for (var i = 0; i < executingTimes; i++)
            await Sut.Writers[i].ExecuteAsync();


        Sut.Writers.Count.Should().Be(times);
        await _database.Received(executingTimes).KeyExpireAsync(redisKey, expectedValue, flag);
    }
}