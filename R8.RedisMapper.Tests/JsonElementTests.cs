using System;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace R8.RedisMapper.Tests
{
    public class JsonElementTests
    {
        [Fact]
        public void should_return_redis_value_from_object()
        {
            const string json = "{\"1\":\"2\",\"3\":\"4\"}";
            var jsonByte = Encoding.UTF8.GetBytes(json);
            var utf8JsonReader = new Utf8JsonReader(jsonByte.AsSpan(), true, new JsonReaderState());
            var value = JsonElement.ParseValue(ref utf8JsonReader);
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, new RedisValueWriterContext { IgnoreDefaultValue = true });

            actual.HasValue.Should().BeTrue();
            actual.IsNullOrEmpty.Should().BeFalse();
            var actualJson = (string)actual;
            actualJson.Should().Be(json);
        }

        [Fact]
        public void should_return_redis_value_from_array()
        {
            const string json = "[\"1\",\"2\"]";
            var jsonByte = Encoding.UTF8.GetBytes(json);
            var utf8JsonReader = new Utf8JsonReader(jsonByte.AsSpan(), true, new JsonReaderState());
            var value = JsonElement.ParseValue(ref utf8JsonReader);
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, new RedisValueWriterContext { IgnoreDefaultValue = true });

            actual.HasValue.Should().BeTrue();
            actual.IsNullOrEmpty.Should().BeFalse();
            var actualJson = (string)actual;
            actualJson.Should().Be(json);
        }

        [Theory]
        [InlineData("true", true, false)]
        [InlineData("false", false, true)]
        public void should_return_redis_value_from_bool(string json, bool expected, bool nullExpected)
        {
            var jsonByte = Encoding.UTF8.GetBytes(json);
            var utf8JsonReader = new Utf8JsonReader(jsonByte.AsSpan(), true, new JsonReaderState());
            var value = JsonElement.ParseValue(ref utf8JsonReader);
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, new RedisValueWriterContext { IgnoreDefaultValue = true });

            if (nullExpected)
            {
                actual.HasValue.Should().BeFalse();
                actual.IsNullOrEmpty.Should().BeTrue();
            }
            else
            {
                actual.HasValue.Should().BeTrue();
                actual.IsNullOrEmpty.Should().BeFalse();
                var actualJson = (bool)actual;
                actualJson.Should().Be(expected);
            }
        }

        [Theory]
        [InlineData("1", 1, false)]
        [InlineData("1.1", 1.1d, false)]
        [InlineData("0", 0, true)]
        public void should_return_redis_value_from_number(string json, double expected, bool nullExpected)
        {
            var jsonByte = Encoding.UTF8.GetBytes(json);
            var utf8JsonReader = new Utf8JsonReader(jsonByte.AsSpan(), true, new JsonReaderState());
            var value = JsonElement.ParseValue(ref utf8JsonReader);
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, new RedisValueWriterContext { IgnoreDefaultValue = true });

            if (nullExpected)
            {
                actual.HasValue.Should().BeFalse();
                actual.IsNullOrEmpty.Should().BeTrue();
            }
            else
            {
                actual.HasValue.Should().BeTrue();
                actual.IsNullOrEmpty.Should().BeFalse();
                var actualJson = (double)actual;
                actualJson.Should().Be(expected);
            }
        }
    }
}