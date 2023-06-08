#nullable enable
using FluentAssertions;
using FluentAssertions.Common;
using R8.RedisHelper.Utils;
using StackExchange.Redis;

namespace R8.RedisHelper.Tests;

public class Optimization_Tests
{
    [Fact]
    public void Should_Return_Long_Value()
    {
        const long actual = 1234567890123456789;

        var optimized = actual.ToOptimizedObject();
        var redisValue = RedisValue.Unbox(optimized);

        var prop = typeof(Types).GetProperty(nameof(Types.LONG));
        var model = new Types();

        var isSet = prop.TrySetFromRedisValue(model, redisValue);

        isSet.Should().BeTrue();
        model.LONG.Should().Be(actual);
    }

    [Fact]
    public void Should_Return_Nullable_Long_Value()
    {
        const long actual = 1234567890123456789;

        var optimized = actual.ToOptimizedObject();
        var redisValue = RedisValue.Unbox(optimized);

        var prop = typeof(Types).GetProperty(nameof(Types.LONG_NULLABLE));
        var model = new Types();

        var isSet = prop.TrySetFromRedisValue(model, redisValue);

        isSet.Should().BeTrue();
        model.LONG_NULLABLE.Should().Be(actual);
    }

    [Fact]
    public void Should_Return_Int_Value()
    {
        const int actual = 123456789;

        var optimized = actual.ToOptimizedObject();
        var redisValue = RedisValue.Unbox(optimized);

        var prop = typeof(Types).GetProperty(nameof(Types.INT));
        var model = new Types();

        var isSet = prop.TrySetFromRedisValue(model, redisValue);

        isSet.Should().BeTrue();
        model.INT.Should().Be(actual);
    }

    [Fact]
    public void Should_Return_Nullable_Int_Value()
    {
        const int actual = 123456789;

        var optimized = actual.ToOptimizedObject();
        var redisValue = RedisValue.Unbox(optimized);

        var prop = typeof(Types).GetProperty(nameof(Types.INT_NULLABLE));
        var model = new Types();

        var isSet = prop.TrySetFromRedisValue(model, redisValue);

        isSet.Should().BeTrue();
        model.INT_NULLABLE.Should().Be(actual);
    }

    [Fact]
    public void Should_Return_Double_Value()
    {
        const double actual = 12345.69;

        var optimized = actual.ToOptimizedObject();
        var redisValue = RedisValue.Unbox(optimized);

        var prop = typeof(Types).GetProperty(nameof(Types.DOUBLE));
        var model = new Types();

        var isSet = prop.TrySetFromRedisValue(model, redisValue);

        isSet.Should().BeTrue();
        model.DOUBLE.Should().Be(actual);
    }

    [Fact]
    public void Should_Return_Nullable_Double_Value()
    {
        const double actual = 12345.69;

        var optimized = actual.ToOptimizedObject();
        var redisValue = RedisValue.Unbox(optimized);

        var prop = typeof(Types).GetProperty(nameof(Types.DOUBLE_NULLABLE));
        var model = new Types();

        var isSet = prop.TrySetFromRedisValue(model, redisValue);

        isSet.Should().BeTrue();
        model.DOUBLE_NULLABLE.Should().Be(actual);
    }

    [Fact]
    public void Should_Return_Boolean_Value()
    {
        const bool actual = true;

        var optimized = actual.ToOptimizedObject();
        var redisValue = RedisValue.Unbox(optimized);

        var prop = typeof(Types).GetProperty(nameof(Types.BOOLEAN));
        var model = new Types();

        var isSet = prop.TrySetFromRedisValue(model, redisValue);

        isSet.Should().BeTrue();
        model.BOOLEAN.Should().Be(actual);
    }

    [Fact]
    public void Should_Return_Nullable_Boolean_Value()
    {
        const bool actual = true;

        var optimized = actual.ToOptimizedObject();
        var redisValue = RedisValue.Unbox(optimized);

        var prop = typeof(Types).GetProperty(nameof(Types.BOOLEAN_NULLABLE));
        var model = new Types();

        var isSet = prop.TrySetFromRedisValue(model, redisValue);

        isSet.Should().BeTrue();
        model.BOOLEAN_NULLABLE.Should().Be(actual);
    }

    [Fact]
    public void Should_Return_DateTime_Value()
    {
        var actual = DateTime.UtcNow;

        var optimized = actual.ToOptimizedObject();
        var redisValue = RedisValue.Unbox(optimized);

        var prop = typeof(Types).GetProperty(nameof(Types.DATETIME));
        var model = new Types();

        var isSet = prop.TrySetFromRedisValue(model, redisValue);

        isSet.Should().BeTrue();
        model.DATETIME.Should().Be(Helpers.FromUnixTimeSeconds(actual.ToUnixTimeSeconds()));
    }

    [Fact]
    public void Should_Return_Nullable_DateTime_Value()
    {
        var actual = DateTime.UtcNow;

        var optimized = actual.ToOptimizedObject();
        var redisValue = RedisValue.Unbox(optimized);

        var prop = typeof(Types).GetProperty(nameof(Types.DATETIME_NULLABLE));
        var model = new Types();

        var isSet = prop.TrySetFromRedisValue(model, redisValue);

        isSet.Should().BeTrue();
        model.DATETIME_NULLABLE.Should().Be(Helpers.FromUnixTimeSeconds(actual.ToUnixTimeSeconds()));
    }

    [Fact]
    public void Should_Return_String_Value()
    {
        const string actual = "unix";

        var optimized = actual.ToOptimizedObject();
        var redisValue = RedisValue.Unbox(optimized);

        var prop = typeof(Types).GetProperty(nameof(Types.STRING));
        var model = new Types();

        var isSet = prop.TrySetFromRedisValue(model, redisValue);

        isSet.Should().BeTrue();
        model.STRING.Should().Be(actual);
    }

    [Fact]
    public void Should_Return_Nullable_String_Value()
    {
        const string actual = "unix";

        var optimized = actual.ToOptimizedObject();
        var redisValue = RedisValue.Unbox(optimized);

        var prop = typeof(Types).GetProperty(nameof(Types.STRING_NULLABLE));
        var model = new Types();

        var isSet = prop.TrySetFromRedisValue(model, redisValue);

        isSet.Should().BeTrue();
        model.STRING_NULLABLE.Should().Be(actual);
    }

    [Fact]
    public void Should_Return_Dictionary_Value()
    {
        var actual = new Dictionary<int, bool>{{1, true}, {2, false}};

        var optimized = actual.ToOptimizedObject();
        var redisValue = RedisValue.Unbox(optimized);

        var prop = typeof(Types).GetProperty(nameof(Types.DICTIONARY));
        var model = new Types();

        var isSet = prop.TrySetFromRedisValue(model, redisValue);

        isSet.Should().BeTrue();
        model.DICTIONARY.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void Should_Return_Nullable_Dictionary_Value()
    {
        var actual = new Dictionary<int, bool>{{1, true}, {2, false}};

        var optimized = actual.ToOptimizedObject();
        var redisValue = RedisValue.Unbox(optimized);

        var prop = typeof(Types).GetProperty(nameof(Types.DICTIONARY_NULLABLE));
        var model = new Types();

        var isSet = prop.TrySetFromRedisValue(model, redisValue);

        isSet.Should().BeTrue();
        model.DICTIONARY_NULLABLE.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void Should_Return_List_Value()
    {
        var actual = new List<bool>{true, false};

        var optimized = actual.ToOptimizedObject();
        var redisValue = RedisValue.Unbox(optimized);

        var prop = typeof(Types).GetProperty(nameof(Types.LIST));
        var model = new Types();

        var isSet = prop.TrySetFromRedisValue(model, redisValue);

        isSet.Should().BeTrue();
        model.LIST.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void Should_Return_Nullable_List_Value()
    {
        var actual = new List<bool>{true, false};

        var optimized = actual.ToOptimizedObject();
        var redisValue = RedisValue.Unbox(optimized);

        var prop = typeof(Types).GetProperty(nameof(Types.LIST_NULLABLE));
        var model = new Types();

        var isSet = prop.TrySetFromRedisValue(model, redisValue);

        isSet.Should().BeTrue();
        model.LIST_NULLABLE.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void Should_Return_Array_Value()
    {
        var actual = new[] {1, 2, 3, 4, 5};

        var optimized = actual.ToOptimizedObject();
        var redisValue = RedisValue.Unbox(optimized);

        var prop = typeof(Types).GetProperty(nameof(Types.ARRAY));
        var model = new Types();

        var isSet = prop.TrySetFromRedisValue(model, redisValue);

        isSet.Should().BeTrue();
        model.ARRAY.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void Should_Return_Nullable_Array_Value()
    {
        var actual = new[] {1, 2, 3, 4, 5};

        var optimized = actual.ToOptimizedObject();
        var redisValue = RedisValue.Unbox(optimized);

        var prop = typeof(Types).GetProperty(nameof(Types.ARRAY_NULLABLE));
        var model = new Types();

        var isSet = prop.TrySetFromRedisValue(model, redisValue);

        isSet.Should().BeTrue();
        model.ARRAY_NULLABLE.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void Should_Return_Enum_Value()
    {
        const FakeEnum actual = FakeEnum.E2;

        var optimized = actual.ToOptimizedObject();
        var redisValue = RedisValue.Unbox(optimized);

        var prop = typeof(Types).GetProperty(nameof(Types.ENUM));
        var model = new Types();

        var isSet = prop.TrySetFromRedisValue(model, redisValue);

        isSet.Should().BeTrue();
        model.ENUM.Should().Be(actual);
    }

    [Fact]
    public void Should_Return_Nullable_Enum_Value()
    {
        const FakeEnum actual = FakeEnum.E2;

        var optimized = actual.ToOptimizedObject();
        var redisValue = RedisValue.Unbox(optimized);

        var prop = typeof(Types).GetProperty(nameof(Types.ENUM_NULLABLE));
        var model = new Types();

        var isSet = prop.TrySetFromRedisValue(model, redisValue);

        isSet.Should().BeTrue();
        model.ENUM_NULLABLE.Should().Be(actual);
    }

    [Fact]
    public void Should_Return_Object_Value()
    {
        var actual = new FakeObject{Id = 1, Name = "unix"};

        var optimized = actual.ToOptimizedObject();
        var redisValue = RedisValue.Unbox(optimized);

        var prop = typeof(Types).GetProperty(nameof(Types.OBJECT));
        var model = new Types();

        var isSet = prop.TrySetFromRedisValue(model, redisValue);

        isSet.Should().BeTrue();
        model.OBJECT.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void Should_Return_Nullable_Object_Value()
    {
        var actual = new FakeObject{Id = 1, Name = "unix"};

        var optimized = actual.ToOptimizedObject();
        var redisValue = RedisValue.Unbox(optimized);

        var prop = typeof(Types).GetProperty(nameof(Types.OBJECT_NULLABLE));
        var model = new Types();

        var isSet = prop.TrySetFromRedisValue(model, redisValue);

        isSet.Should().BeTrue();
        model.OBJECT_NULLABLE.Should().BeEquivalentTo(actual);
    }
}