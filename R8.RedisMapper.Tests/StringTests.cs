using System.Reflection;
using FluentAssertions;
using StackExchange.Redis;
using Xunit;

namespace R8.RedisMapper.Tests
{
    public class StringTests
    {
        public class DummyString
        {
            public string Value { get; set; }
        }

        [Theory]
        [InlineData(nameof(DummyString.Value), "5")]
        [InlineData(nameof(DummyString.Value), null)]
        public void should_set_get(string propName, string value)
        {
            var type = typeof(DummyString);
            var instance = new DummyString();

            var prop = (CachedPropertyInfo)type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, value, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (string?)prop.Property.GetValue(instance);
            v.Should().Be(value);
        }

        [Fact]
        public void should_set_get_by_common_method()
        {
            var type = typeof(DummyString);
            var instance = new DummyString();
            var value = "5";

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyString.Value), BindingFlags.Public | BindingFlags.Instance);
            var set = prop.SetValue(instance, value, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));
            set.Should().BeTrue();

            var v = prop.GetValue(instance, Configuration.ValueFormatters, new RedisValueWriterContext { IgnoreDefaultValue = true });
            v.Should().Be(value);
        }

        [Fact]
        public void should_set_get_by_redis_value()
        {
            var type = typeof(DummyString);
            var instance = new DummyString();
            var value = "3";
            var redisValue = (RedisValue)value;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyString.Value), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (string?)prop.Property.GetValue(instance);
            v.Should().Be(value);
        }

        [Fact]
        public void should_set_get_nullable_by_redis_value()
        {
            var type = typeof(DummyString);
            var instance = new DummyString();
            var value = (string)null;
            var redisValue = (RedisValue)value;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyString.Value), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (string?)prop.Property.GetValue(instance);
            v.Should().Be(value);
        }

        [Theory]
        [InlineData("true", true, false)]
        [InlineData("true", false, false)]
        [InlineData("", true, true)]
        [InlineData("", false, true)]
        [InlineData(null, true, true)]
        [InlineData(null, false, true)]
        public void should_return_value(string value, bool ignoreDefaultValue, bool nullExpected)
        {
            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = ignoreDefaultValue };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var parsed = RedisValueFormatter.Read(actual, new RedisValueReaderContext(writerContext, typeof(string)));
            if (nullExpected)
            {
                parsed.Should().BeNull();
            }
            else
            {
                parsed!.Should().Be(value);
            }
        }
    }
}