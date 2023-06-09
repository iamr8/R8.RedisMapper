using FluentAssertions;
using NSubstitute;
using R8.RedisHelper.Handlers;
using StackExchange.Redis;

namespace R8.RedisHelper.Tests;

public class RedisCacheProviderTests
{
    public RedisCacheProviderTests()
    {
        options = new RedisHelperOptions
        {
            DatabaseId = _databaseId,
            Configurations = new ConfigurationOptions
            {
                AllowAdmin = true,
                IncludeDetailInExceptions = true,
                IncludePerformanceCountersInExceptions = true,
                ConnectRetry = 3,
            }
        };
        options.Configurations.EndPoints.Add("localhost", 6379);
    }

    private readonly DummyLogger<RedisCacheProvider> _logger = new();
    private RedisCacheProvider _sut;
    private const int _databaseId = 3;

    private RedisHelperOptions options;

    private RedisCacheProvider Sut => _sut ??= new RedisCacheProvider(options, _logger);

    [Theory]
    [InlineData(true, CommandFlags.FireAndForget)]
    [InlineData(false, CommandFlags.None)]
    public async Task FlushAsync_causes_server_FlushDatabaseAsync_call(bool fireAndForget, CommandFlags expectedFlag)
    {
        var mockServer = Substitute.For<IServer>();
        Sut.SetPrivateField("_server", mockServer);

        await Sut.FlushAsync(fireAndForget);

        await mockServer.Received(1).FlushDatabaseAsync(Sut.DatabaseId, expectedFlag);
    }

    [Fact]
    public async Task ExistsAsync_causes_dataBase_KeyExistsAsync_call()
    {
        var cacheKey = new RedisKey("test");
        var mockDatabase = Substitute.For<IDatabase>();
        Sut.SetPrivateField("_database", mockDatabase);

        await Sut.ExistsAsync(cacheKey);

        await mockDatabase.Received(1).KeyExistsAsync(cacheKey);
    }

    [Theory]
    [InlineData("test", true)]
    [InlineData("other", false)]
    public async Task ExistsAsync_should_return_dataBase_KeyExistsAsync_result(string redisKey, bool expectedResult)
    {
        var cacheKey = new RedisKey("test");
        var mockDatabase = Substitute.For<IDatabase>();
        mockDatabase.KeyExistsAsync(cacheKey).Returns(true);
        Sut.SetPrivateField("_database", mockDatabase);

        var result = await Sut.ExistsAsync(new RedisKey(redisKey));

        result.Should().Be(expectedResult);
        await mockDatabase.Received(1).KeyExistsAsync(new RedisKey(redisKey));
    }

    [Fact]
    public void Having_more_than_One_pageSize_items_causes_multiple_server_Keys_call()
    {
        var pattern = new RedisValue("test");
        var pageSize = 3;
        var expectedResult = new string[] { new("1"), new("2"), new("3"), new("4"), new("5") };
        var mockServer = Substitute.For<IServer>();
        mockServer.Keys(
            Arg.Any<int>(),
            Arg.Any<RedisValue>(),
            Arg.Any<int>(),
            0L).Returns(new RedisKey[] { expectedResult[0], expectedResult[1], expectedResult[2] });
        mockServer.Keys(
            Arg.Any<int>(),
            Arg.Any<RedisValue>(),
            Arg.Any<int>(),
            3L).Returns(new RedisKey[] { expectedResult[3], expectedResult[4] });
        Sut.SetPrivateField("_server", mockServer);

        var result = Sut.Scan(pattern, pageSize);

        mockServer.Received(1).Keys(_databaseId, pattern, pageSize, 0L);
        mockServer.Received(1).Keys(_databaseId, pattern, pageSize, 3L);
        result.Should().BeEquivalentTo(expectedResult.Select(x => new RedisKey(x)).ToArray());
    }

    [Fact]
    public void Scan_causes_server_Keys_call()
    {
        var pattern = new RedisValue("test");
        var pageSize = 1;
        var mockServer = Substitute.For<IServer>();
        Sut.SetPrivateField("_server", mockServer);

        Sut.Scan(pattern, pageSize);

        mockServer.Received(1).Keys(_databaseId,
            Arg.Is<RedisValue>(s => s == pattern),
            Arg.Is<int>(s => s == pageSize),
            Arg.Is<int>(s => s == 0));
    }

    [Fact]
    public void Passing_null_pattern_to_Scan_causes_ArgumentNullException()
    {
        Sut.SetPrivateField("_server", Substitute.For<IServer>());

        var act = () => Sut.Scan(null, 1);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(true, CommandFlags.FireAndForget)]
    [InlineData(false, CommandFlags.None)]
    public async Task PublishAsync_causes_subscriber_PublishAsync_call(bool fireAndForget, CommandFlags expectedFlag)
    {
        var channelName = "testChannel";
        var message = "TestMessage";
        var mockSubscriber = Substitute.For<ISubscriber>();
        Sut.SetPrivateField("_subscriber", mockSubscriber);


        await Sut.PublishAsync(channelName, message, fireAndForget);


        await mockSubscriber.Received(1).PublishAsync(new RedisChannel(channelName, RedisChannel.PatternMode.Literal),
            new RedisValue(message), expectedFlag);
    }

    [Fact]
    public async Task GetAsync_causes_database_HashGetAsync_and_return_correct_result()
    {
        var cacheKey = new RedisKey("test");
        var fields = new[] { "field1", "field2" };
        var expectedResult = new FakeRedisTestModel() { field1 = "f1", field2 = "f2" };
        var mockDatabase = Substitute.For<IDatabase>();
        Sut.SetPrivateField("_database", mockDatabase);
        mockDatabase.HashGetAsync(
                Arg.Any<RedisKey>(),
                Arg.Any<RedisValue[]>())
            .Returns(new[] { "f1", "f2" }.Select(s => new RedisValue(s)).ToArray());

        var result = await Sut.GetAsync<FakeRedisTestModel>(cacheKey, fields);

        result.Value.Should().BeEquivalentTo(expectedResult);
        await mockDatabase.Received(1).HashGetAsync(cacheKey,
            Arg.Is<RedisValue[]>(s => s.IsTheSameAs(fields.Select(w => new RedisValue(w)).ToArray())));
    }

    [Fact]
    public async Task Non_generic_GetAsync_causes_database_HashGetAsync_and_return_correct_result()
    {
        var cacheKey = new RedisKey("test");
        var fields = new[] { "field1", "field2" };
        var expectedResult = new Dictionary<string, RedisValue>()
        {
            { "field1", new RedisValue("f1") }, { "field2", new RedisValue("f2") }
        };
        var mockDatabase = Substitute.For<IDatabase>();
        Sut.SetPrivateField("_database", mockDatabase);
        mockDatabase.HashGetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue[]>())
            .Returns(new[] { "f1", "f2" }.Select(s => new RedisValue(s)).ToArray());

        var result = await Sut.GetAsync(cacheKey, "field1", "field2");

        result.Value.Should().BeEquivalentTo(expectedResult);
        await mockDatabase.Received(1).HashGetAsync(cacheKey,
            Arg.Is<RedisValue[]>(s => s.IsTheSameAs(fields.Select(w => new RedisValue(w)).ToArray()))
        );
    }

    [Theory]
    [InlineData(When.Always, true, false)]
    [InlineData(When.Always, false, true)]
    public async Task SetAsync_causes_database_HashSetAsync_and_return_correct_result(When when, bool expectedResult, bool fireAndForget)
    {
        var cacheKey = new RedisKey("test");
        var mockDatabase = Substitute.For<IDatabase>();
        var fieldName = "field1";
        var expectedValue = "f1";
        Sut.SetPrivateField("_database", mockDatabase);
        mockDatabase.HashSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<RedisValue>())
            .Returns(expectedResult);

        var result = await Sut.SetAsync(cacheKey, fieldName, expectedValue, when, fireAndForget);

        result.Value.Should().Be(expectedResult);
        await mockDatabase.Received(1).HashSetAsync(cacheKey, new RedisValue(fieldName), new RedisValue(expectedValue),
            when, (fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Generic_SetAsync_causes_database_HashSetAsync_call(bool fireAndForget)
    {
        var cacheKey = new RedisKey("test");
        var mockDatabase = Substitute.For<IDatabase>();
        var value = new FakeRedisTestModel() { field1 = "f1", field3 = "f3" };
        var expectedValue = new HashEntry[] { new("field1", "f1"), new("field3", "f3") };
        Sut.SetPrivateField("_database", mockDatabase);

        await Sut.SetAsync(cacheKey, value, fireAndForget);

        await mockDatabase.Received(1).HashSetAsync(cacheKey,
            Arg.Is<HashEntry[]>(s => s.IsTheSameAs(expectedValue)),
            (fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None));
    }


    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Calling_SetAsync_with_passing_object_causes_database_HashSetAsync_call(bool fireAndForget)
    {
        var cacheKey = new RedisKey("test");
        var mockDatabase = Substitute.For<IDatabase>();
        var value = new { field1 = "f1", field3 = "f3" };
        var expectedValue = new HashEntry[] { new("field1", "f1"), new("field3", "f3") };
        Sut.SetPrivateField("_database", mockDatabase);

        await Sut.SetAsync(cacheKey, value, fireAndForget);

        await mockDatabase.Received(1).HashSetAsync(cacheKey,
            Arg.Is<HashEntry[]>(s => s.IsTheSameAs(expectedValue)),
            (fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DeleteAsync_causes_database_KeyDeleteAsync_call(bool fireAndForget)
    {
        var cacheKey = new RedisKey("test");
        var mockDatabase = Substitute.For<IDatabase>();
        Sut.SetPrivateField("_database", mockDatabase);

        await Sut.DeleteAsync(cacheKey, fireAndForget);

        await mockDatabase.Received(1).KeyDeleteAsync(cacheKey, fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DeleteAsync_by_fieldName_causes_database_HashDeleteAsync_call(bool fireAndForget)
    {
        var cacheKey = new RedisKey("test");
        var mockDatabase = Substitute.For<IDatabase>();
        var fieldName = "SampleName";
        Sut.SetPrivateField("_database", mockDatabase);

        await Sut.DeleteAsync(cacheKey, fieldName, fireAndForget);

        await mockDatabase.Received(1).HashDeleteAsync(cacheKey,
            Arg.Is<RedisValue>(s => s.IsTheSameAs(new RedisValue(fieldName))),
            fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None);
    }

    [Theory]
    [InlineData(2, false)]
    [InlineData(4, true)]
    public async Task IncrementAsync_causes_database_StringIncrementAsync_call(long expectedValue, bool fireAndForget)
    {
        var cacheKey = new RedisKey("test");
        var mockDatabase = Substitute.For<IDatabase>();
        Sut.SetPrivateField("_database", mockDatabase);

        await Sut.IncrementAsync(cacheKey, expectedValue, fireAndForget);

        await mockDatabase.Received(1).StringIncrementAsync(cacheKey, expectedValue, fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None);
    }

    [Theory]
    [InlineData(2, false)]
    [InlineData(4, true)]
    public async Task IncrementAsync_by_fieldName_causes_database_HashIncrementAsync_call(long expectedValue, bool fireAndForget)
    {
        var cacheKey = new RedisKey("test");
        var mockDatabase = Substitute.For<IDatabase>();
        var fieldName = "SampleName";
        Sut.SetPrivateField("_database", mockDatabase);

        await Sut.IncrementAsync(cacheKey, fieldName, expectedValue, fireAndForget);

        await mockDatabase.Received(1).HashIncrementAsync(cacheKey,
            Arg.Is<RedisValue>(s => s.IsTheSameAs(new RedisValue(fieldName))),
            expectedValue, (fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None));
    }

    [Theory]
    [InlineData("10:00:04:03", false)]
    [InlineData("00:11:14:03", true)]
    public async Task ExpireAsync_causes_database_KeyExpireAsync_call(string time, bool fireAndForget)
    {
        var cacheKey = new RedisKey("test");
        var mockDatabase = Substitute.For<IDatabase>();
        var expectedTime = TimeSpan.Parse(time);
        Sut.SetPrivateField("_database", mockDatabase);

        await Sut.ExpireAsync(cacheKey, expectedTime, fireAndForget);

        await mockDatabase.Received(1).KeyExpireAsync(cacheKey,
            expectedTime, (fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None));
    }

    [Fact]
    public async Task Passing_null_RedisWrite_action_to_BatchAsync_causes_ArgumentNullException()
    {
        Sut.SetPrivateField("_database", Substitute.For<IDatabase>());

        var act = async () => { await Sut.BatchAsync(null); };

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task BatchWriteAsync_causes_dataBase_batch_call(bool fireAndForget)
    {
        var cacheKey = new RedisKey("test");
        var mockDatabase = Substitute.For<IDatabase>();
        var mockBatch = Substitute.For<IBatch>();
        mockDatabase.CreateBatch().Returns(mockBatch);
        Sut.SetPrivateField("_database", mockDatabase);

        await Sut.BatchAsync(b => { b.Delete(cacheKey); }, fireAndForget);

        await mockBatch.Received(1).KeyDeleteAsync(cacheKey, fireAndForget ? CommandFlags.FireAndForget : CommandFlags.None);
    }

    [Fact]
    public async Task Passing_null_RedisRead_action_to_BatchAsync_causes_ArgumentNullException()
    {
        var mockDatabase = Substitute.For<IDatabase>();
        Sut.SetPrivateField("_database", mockDatabase);

        var act = async () => await Sut.BatchAsync<FakeRedisTestModel>(null);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task BatchReadAsync_causes_dataBase_batch_call()
    {
        var cacheKey = new RedisKey("test");
        var expectedValue = new FakeRedisTestModel() { field1 = "f1" };
        var fieldName = "field1";
        var mockDatabase = Substitute.For<IDatabase>();
        var mockBatch = Substitute.For<IBatch>();
        mockBatch.HashGetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>()).Returns(new RedisValue("f1"));
        mockDatabase.CreateBatch().Returns(mockBatch);
        Sut.SetPrivateField("_database", mockDatabase);

        var result = await Sut.BatchAsync<FakeRedisTestModel>(b => { b.Get(cacheKey, fieldName); });

        result.Count.Should().Be(1);
        result.First().Value.Should().BeEquivalentTo(expectedValue);
        await mockBatch.Received(1).HashGetAsync(cacheKey, new RedisValue(fieldName));
    }
}