using FluentAssertions;
using NSubstitute;
using R8.RedisHelper.Models;
using StackExchange.Redis;
using R8.RedisHelper.Utils;

namespace R8.RedisHelper.Tests;

public class CommandsTests
{
    private readonly IDatabaseAsync _database = Substitute.For<IDatabaseAsync>();

    [Theory]
    [InlineData(CommandFlags.DemandMaster)]
    [InlineData(CommandFlags.FireAndForget)]
    public async Task Increment_by_redisKey_causes_dataBase_StringIncrementAsync_call(CommandFlags flag)
    {
        var redisKey = new RedisKey("test");
        var value = 5;


        var result = _database.Increment(new RedisCacheKey(redisKey), value, flag);
        await result.ExecuteAsync();


        result.CacheKey.ToString().Should().Be("test");
        result.Command.Should().Be("INCR");
        result.Fields.Should().BeEmpty();
        await _database.Received(1).StringIncrementAsync(redisKey, value, flag);
    }

    [Theory]
    [InlineData(CommandFlags.DemandMaster)]
    [InlineData(CommandFlags.FireAndForget)]
    public async Task Increment_by_FieldName_causes_dataBase_HashIncrementAsync_call(CommandFlags flag)
    {
        var redisKey = new RedisKey("test");
        var value = 5;
        var fieldName = "field2";


        var result = _database.Increment(new RedisCacheKey(redisKey), fieldName, value, flag);
        await result.ExecuteAsync();


        result.CacheKey.ToString().Should().Be(redisKey);
        result.Command.Should().Be("HINCRBY");
        result.Fields.Should().BeEquivalentTo(fieldName);
        await _database.Received(1).HashIncrementAsync(redisKey, new RedisValue(fieldName), value, flag);
    }

    [Theory]
    [InlineData(CommandFlags.DemandMaster)]
    [InlineData(CommandFlags.FireAndForget)]
    public async Task Expire_causes_dataBase_KeyExpireAsync_call(CommandFlags flag)
    {
        var redisKey = new RedisKey("test");
        var time = new TimeSpan(1, 2, 3, 4);


        var result = _database.Expire(new RedisCacheKey(redisKey), time, flag);
        await result.ExecuteAsync();


        result.CacheKey.ToString().Should().Be(redisKey);
        result.Command.Should().Be("EXPIRE");
        result.Fields.Should().BeEmpty();
        await _database.Received(1).KeyExpireAsync(redisKey, time, flag);
    }

    [Theory]
    [InlineData(CommandFlags.DemandMaster)]
    [InlineData(CommandFlags.FireAndForget)]
    public async Task Delete_causes_dataBase_KeyDeleteAsync_call(CommandFlags flag)
    {
        var redisKey = new RedisKey("test");


        var result = _database.Delete(new RedisCacheKey(redisKey), flag);
        await result.ExecuteAsync();


        result.CacheKey.ToString().Should().Be(redisKey);
        result.Command.Should().Be("DEL");
        result.Fields.Should().BeEmpty();
        await _database.Received(1).KeyDeleteAsync(redisKey, flag);
    }

    [Theory]
    [InlineData(CommandFlags.DemandMaster)]
    [InlineData(CommandFlags.FireAndForget)]
    public async Task Delete_by_fieldName_causes_dataBase_KeyDeleteAsync_call(CommandFlags flag)
    {
        var redisKey = new RedisKey("test");
        var fieldName = "field2";


        var result = _database.Delete(new RedisCacheKey(redisKey), fieldName, flag);
        await result.ExecuteAsync();


        result.CacheKey.ToString().Should().Be(redisKey);
        result.Command.Should().Be("HDEL");
        result.Fields.Should().BeEquivalentTo(fieldName);
        await _database.Received(1).HashDeleteAsync(redisKey, new RedisValue(fieldName), flag);
    }

    [Theory]
    [InlineData(CommandFlags.DemandMaster)]
    [InlineData(CommandFlags.FireAndForget)]
    public async Task Set_object_with_more_than_one_field_causes_dataBase_HashSetAsync_call(CommandFlags flag)
    {
        var redisKey = new RedisKey("test");
        var value = new { field1 = "f1", field2 = "f2" };


        var result = _database.Set(new RedisCacheKey(redisKey), value, flag);
        await result.ExecuteAsync();


        result.CacheKey.ToString().Should().Be(redisKey);
        result.Command.Should().Be("HMSET");
        result.Fields.Should().BeEquivalentTo("field1", "field2");
        await _database.Received(1).HashSetAsync(
            redisKey,
            Arg.Is<HashEntry[]>(s => s.IsTheSameAs(new HashEntry[] { new("field1", "f1"), new("field2", "f2") })),
            flag
        );
    }

    [Theory]
    [InlineData(CommandFlags.DemandMaster)]
    [InlineData(CommandFlags.FireAndForget)]
    public async Task Set_object_with_one_field_causes_dataBase_HashSetAsync_call(CommandFlags flag)
    {
        var redisKey = new RedisKey("test");
        var value = new { field1 = "f1" };


        var result = _database.Set(new RedisCacheKey(redisKey), value, flag);
        await result.ExecuteAsync();


        result.CacheKey.ToString().Should().Be(redisKey);
        result.Command.Should().Be("HSET");
        result.Fields.Should().BeEquivalentTo("field1");
        await _database.Received(1).HashSetAsync(redisKey, new RedisValue("field1"), new RedisValue("f1"), Arg.Any<When>(), flag);
    }

    [Theory]
    [InlineData("field1", null, true)]
    [InlineData(null, "value", true)]
    [InlineData("field1", "value", false)]
    public void Set_with_argument_causes_ArgumentNullException(string fieldName, string value, bool expectedError)
    {
        var act = () => _database.Set(
            new RedisCacheKey(new RedisKey("cacheKey")),
            fieldName,
            value,
            When.Always,
            CommandFlags.DemandMaster);

        if (expectedError)
        {
            act.Should().Throw<ArgumentNullException>();
        }
        else
        {
            act.Should().NotThrow<ArgumentNullException>();
        }
    }

    [Fact]
    public async Task Set_with_not_optimizable_argument_causes_database_HashDeleteAsync()
    {
        var redisKey = new RedisKey("cacheKey");
        var fieldName = "fieldName";
        var fieldName2 = "fieldName";


        var writer = _database.Set(new RedisCacheKey(redisKey), fieldName, string.Empty, When.Always, CommandFlags.DemandMaster);
        await writer.ExecuteAsync();

        var writer2 = _database.Set(new RedisCacheKey(redisKey), fieldName2, new List<string>(), When.Always, CommandFlags.FireAndForget);
        await writer2.ExecuteAsync();


        writer.CacheKey.ToString().Should().Be(redisKey);
        writer.Command.Should().Be("HDEL");
        writer.Fields.Should().BeEquivalentTo(fieldName);
        await _database.Received(1).HashDeleteAsync(redisKey, new RedisValue(fieldName), CommandFlags.DemandMaster);

        writer2.CacheKey.ToString().Should().Be(redisKey);
        writer2.Command.Should().Be("HDEL");
        writer2.Fields.Should().BeEquivalentTo(fieldName2);
        await _database.Received(1).HashDeleteAsync(redisKey, new RedisValue(fieldName2), CommandFlags.FireAndForget);
    }

    [Theory]
    [InlineData(When.Always, CommandFlags.DemandMaster, "HSET")]
    [InlineData(When.Exists, CommandFlags.FireAndForget, "HSETNX")]
    [InlineData(When.NotExists, CommandFlags.DemandMaster, "HSETNX")]
    public async Task Set_with_correct_argument_causes_HashSetAsync(When when, CommandFlags flag, string expectedCommand)
    {
        var redisKey = new RedisKey("cacheKey");
        var fieldName = "fieldName";
        var value = "testValue";


        var writer = _database.Set(new RedisCacheKey(redisKey), fieldName, value, when, flag);
        await writer.ExecuteAsync();


        writer.CacheKey.ToString().Should().Be(redisKey);
        writer.Command.Should().Be(expectedCommand);
        writer.Fields.Should().BeEquivalentTo(fieldName);
        await _database.Received(1).HashSetAsync(redisKey, new RedisValue(fieldName), new RedisValue(value), when, flag);
    }


    [Fact]
    public async Task Get_object_with_one_field_causes_dataBase_HashGetAsync_call()
    {
        var redisKey = new RedisKey("test");
        var fieldName = "field1";


        var result = _database.Get(new RedisCacheKey(redisKey), fieldName);
        await result.ExecuteAsync();


        result.CacheKey.ToString().Should().Be(redisKey);
        result.Command.Should().Be("HGET");
        result.Fields.Should().BeEquivalentTo(fieldName);
        await _database.Received(1).HashGetAsync(redisKey, new RedisValue(fieldName));
    }

    [Fact]
    public async Task Get_object_with_more_than_one_field_causes_dataBase_HashGetAsync_call()
    {
        var redisKey = new RedisKey("test");
        var fieldName = "field1";
        var fieldName2 = "field2";


        var result = _database.Get(new RedisCacheKey(redisKey), fieldName, fieldName2);
        await result.ExecuteAsync();


        result.CacheKey.ToString().Should().Be(redisKey);
        result.Command.Should().Be("HMGET");
        result.Fields.Should().BeEquivalentTo(fieldName, fieldName2);
        await _database.Received(1).HashGetAsync(redisKey,
            Arg.Is<RedisValue[]>(s => s.IsTheSameAs(new RedisValue[] { new(fieldName), new(fieldName2) })));
    }

    [Fact]
    public async Task Get_with_more_than_one_field_causes_dataBase_HashGetAsync_call()
    {
        var redisKey = new RedisKey("test");
        var fieldName = "field1";
        var fieldName2 = "field2";


        var result = _database.Get<FakeRedisTestModel>(new RedisCacheKey(redisKey), fieldName, fieldName2);
        await result.ExecuteAsync();


        result.CacheKey.ToString().Should().Be(redisKey);
        result.Command.Should().Be("HMGET");
        result.Fields.Should().BeEquivalentTo(fieldName, fieldName2);
        await _database.Received(1).HashGetAsync(redisKey,
            Arg.Is<RedisValue[]>(s => s.IsTheSameAs(new RedisValue[] { new(fieldName), new(fieldName2) })));
    }

    [Fact]
    public async Task Get_with_one_field_causes_dataBase_HashGetAsync_call()
    {
        var redisKey = new RedisKey("test");
        var fieldName = "field1";


        var result = _database.Get<FakeRedisTestModel>(new RedisCacheKey(redisKey), fieldName);
        await result.ExecuteAsync();


        result.CacheKey.ToString().Should().Be(redisKey);
        result.Command.Should().Be("HGET");
        result.Fields.Should().BeEquivalentTo(fieldName);
        await _database.Received(1).HashGetAsync(redisKey,
            Arg.Is<RedisValue>(s => s.IsTheSameAs(new RedisValue(fieldName))));
    }

    [Fact]
    public async Task Get_with_one_field_return_dataBase_HashGetAsync_response()
    {
        var expectedValue = "f1";
        _database.HashGetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>()).Returns(new RedisValue(expectedValue));
        var redisKey = new RedisKey("test");
        var fieldName = "field1";


        var reader = _database.Get<FakeRedisTestModel>(new RedisCacheKey(redisKey), fieldName);
        await reader.ExecuteAsync();
        var result = reader.GetResult();


        result.Length.Should().Be(1);
        result.First().Should().Be(expectedValue);
        reader.CacheKey.ToString().Should().Be(redisKey);
        reader.Command.Should().Be("HGET");
        reader.Fields.Should().BeEquivalentTo(fieldName);
        await _database.Received(1).HashGetAsync(redisKey,
            Arg.Is<RedisValue>(s => s.IsTheSameAs(new RedisValue(fieldName))));
    }

    [Fact]
    public async Task Get_object_with_one_field_return_dataBase_HashGetAsync_response()
    {
        var expectedValue = "f1";
        _database.HashGetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>()).Returns(new RedisValue(expectedValue));
        var redisKey = new RedisKey("test");
        var fieldName = "field1";


        var reader = _database.Get(new RedisCacheKey(redisKey), fieldName);
        await reader.ExecuteAsync();
        var result = reader.GetResult();


        result.Length.Should().Be(1);
        result.First().Should().Be(expectedValue);
        reader.CacheKey.ToString().Should().Be(redisKey);
        reader.Command.Should().Be("HGET");
        reader.Fields.Should().BeEquivalentTo(fieldName);
        await _database.Received(1).HashGetAsync(redisKey,
            Arg.Is<RedisValue>(s => s.IsTheSameAs(new RedisValue(fieldName))));
    }

    [Fact]
    public async Task Get_with_more_than_one_field_return_dataBase_HashGetAsync_response()
    {
        var expectedValue = new RedisValue[] { new("f1"), new("f2"), };
        _database.HashGetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue[]>()).Returns(expectedValue);
        var redisKey = new RedisKey("test");
        var fieldName = "field1";
        var fieldName2 = "field2";


        var reader = _database.Get<FakeRedisTestModel>(new RedisCacheKey(redisKey), fieldName, fieldName2);
        await reader.ExecuteAsync();
        var result = reader.GetResult();


        result.Length.Should().Be(2);
        result.First().Should().Be(expectedValue.First());
        result.Last().Should().Be(expectedValue.Last());
        reader.CacheKey.ToString().Should().Be(redisKey);
        reader.Command.Should().Be("HMGET");
        reader.Fields.Should().BeEquivalentTo(fieldName, fieldName2);
        await _database.Received(1).HashGetAsync(redisKey,
            Arg.Is<RedisValue[]>(s => s.IsTheSameAs(new RedisValue[] { new(fieldName), new(fieldName2) })));
    }

    [Fact]
    public async Task Get_object_with_more_than_one_field_return_dataBase_HashGetAsync_response()
    {
        var expectedValue = new RedisValue[] { new("f1"), new("f2"), };
        _database.HashGetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue[]>()).Returns(expectedValue);
        var redisKey = new RedisKey("test");
        var fieldName = "field1";
        var fieldName2 = "field2";


        var reader = _database.Get(new RedisCacheKey(redisKey), fieldName, fieldName2);
        await reader.ExecuteAsync();
        var result = reader.GetResult();


        result.Length.Should().Be(2);
        result.First().Should().Be(expectedValue.First());
        result.Last().Should().Be(expectedValue.Last());
        reader.CacheKey.ToString().Should().Be(redisKey);
        reader.Command.Should().Be("HMGET");
        reader.Fields.Should().BeEquivalentTo(fieldName, fieldName2);
        await _database.Received(1).HashGetAsync(redisKey,
            Arg.Is<RedisValue[]>(s => s.IsTheSameAs(new RedisValue[] { new(fieldName), new(fieldName2) })));
    }
}