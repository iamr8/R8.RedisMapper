using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using FluentAssertions;
using StackExchange.Redis;
using Xunit;

namespace R8.RedisMapper.Tests
{
    public class DictionaryTests
    {
        public class NestedDummyObj
        {
            public string Name { get; set; }
        }

        public class DummyArray
        {
            public Dictionary<string, string> ValueStringString { get; set; }
            public Dictionary<int, NestedDummyObj> ValueInt32Object { get; set; }
        }

        [Fact]
        public void should_set_get_dictionary_of_string_string()
        {
            var type = typeof(DummyArray);
            var instance = new DummyArray();
            var value = new Dictionary<string, string> { { "1", "2" }, { "3", "4" } };
            var valueJson = JsonSerializer.Serialize(value);
            var redisValue = (RedisValue)valueJson;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyArray.ValueStringString), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            instance.ValueStringString.Should().BeEquivalentTo(value);
        }

        [Fact]
        public void should_set_get_dictionary_of_string_string_by_common_method()
        {
            var type = typeof(DummyArray);
            var instance = new DummyArray();
            var value = new Dictionary<string, string> { { "1", "2" }, { "3", "4" } };
            var valueJson = JsonSerializer.Serialize(value);
            var redisValue = (RedisValue)valueJson;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyArray.ValueStringString), BindingFlags.Public | BindingFlags.Instance);
            var set = prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));
            set.Should().BeTrue();

            var v = prop.GetValue(instance, Configuration.ValueFormatters, new RedisValueWriterContext { IgnoreDefaultValue = true });
            v.Should().Be(redisValue);
        }

        [Fact]
        public void should_set_get_dictionary_of_string_object()
        {
            var type = typeof(DummyArray);
            var instance = new DummyArray();
            var value = new Dictionary<int, NestedDummyObj> { { 1, new NestedDummyObj { Name = "Foo" } }, { 3, new NestedDummyObj { Name = "Bar" } } };
            var valueJson = JsonSerializer.Serialize(value);
            var redisValue = (RedisValue)valueJson;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyArray.ValueInt32Object), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            instance.ValueInt32Object.Should().BeEquivalentTo(value);
        }

        [Fact]
        public void should_return_value()
        {
            var value = new Dictionary<string, string> { { "1", "2" }, { "3", "4" } };
            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var parsed = RedisValueFormatter.Read(actual, new RedisValueReaderContext(writerContext, value.GetType())) as Dictionary<string, string>;
            parsed!.Should().BeEquivalentTo(value);
        }

        [Fact]
        public void should_return_empty_value()
        {
            var value = new Dictionary<string, string>();
            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = false };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var parsed = RedisValueFormatter.Read(actual, new RedisValueReaderContext(writerContext, value.GetType())) as Dictionary<string, string>;
            parsed!.Should().BeEquivalentTo(value);
        }

        [Fact]
        public void should_return_default_value()
        {
            var value = new Dictionary<string, string>();
            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var parsed = (Dictionary<string, string>?) RedisValueFormatter.Read(actual, new RedisValueReaderContext(writerContext, value.GetType()));
            parsed.Should().BeNull();
        }
    }
}