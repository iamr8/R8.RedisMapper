using System;
using System.Reflection;
using FluentAssertions;
using StackExchange.Redis;
using Xunit;

namespace R8.RedisMapper.Tests
{
    public class BooleanTests
    {
        public class DummyBoolean
        {
            public bool Value { get; set; }
            public bool? ValueNullable { get; set; }
        }

        [Theory]
        [InlineData(nameof(DummyBoolean.Value), true)]
        public void should_set_get(string propName, bool value)
        {
            var type = typeof(DummyBoolean);
            var instance = new DummyBoolean();

            var prop = (CachedPropertyInfo)type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, value, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (bool?)prop.Property.GetValue(instance);
            v.Should().Be(value);
        }

        [Theory]
        [InlineData(nameof(DummyBoolean.Value), true)]
        public void should_set_get_by_common_method(string propName, bool value)
        {
            var type = typeof(DummyBoolean);
            var instance = new DummyBoolean();

            var prop = (CachedPropertyInfo)type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            var set = prop.SetValue(instance, value, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));
            set.Should().BeTrue();

            var v = prop.GetValue(instance, Configuration.ValueFormatters, new RedisValueWriterContext { IgnoreDefaultValue = true });
            v.Should().Be(value);
        }

        [Theory]
        [InlineData(nameof(DummyBoolean.ValueNullable), true)]
        [InlineData(nameof(DummyBoolean.ValueNullable), null)]
        public void should_set_get_nullable(string propName, bool? value)
        {
            var type = typeof(DummyBoolean);
            var instance = new DummyBoolean();

            var prop = (CachedPropertyInfo)type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, value, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (bool?)prop.Property.GetValue(instance);
            v.Should().Be(value);
        }

        [Fact]
        public void should_set_get_nullable_2_by_redis_value()
        {
            var type = typeof(DummyBoolean);
            var instance = new DummyBoolean();
            var redisValue = new RedisValue();

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyBoolean.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (bool?)prop.Property.GetValue(instance);
            v.Should().BeNull();
        }

        [Fact]
        public void should_set_get_by_redis_value_nonmatching_type()
        {
            var type = typeof(DummyBoolean);
            var instance = new DummyBoolean();
            var redisValue = (RedisValue)"ghgh";

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyBoolean.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
            Assert.Throws<ArgumentException>(() => prop.Property.SetValue(instance, redisValue));
        }

        [Fact]
        public void should_set_get_by_redis_value()
        {
            var type = typeof(DummyBoolean);
            var instance = new DummyBoolean();
            var value = true;
            var redisValue = (RedisValue)value;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyBoolean.Value), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (bool?)prop.Property.GetValue(instance);
            v.Should().Be(value);
        }

        [Fact]
        public void should_set_get_nullable_by_redis_value()
        {
            var type = typeof(DummyBoolean);
            var instance = new DummyBoolean();
            var value = true;
            var redisValue = (RedisValue)value;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyBoolean.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (bool?)prop.Property.GetValue(instance);
            v.Should().Be(value);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void should_return_value(bool value, bool ignoreDefaultValue)
        {
            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = ignoreDefaultValue };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var parsed = RedisValueFormatter.Read(actual, new RedisValueReaderContext(writerContext, typeof(bool))) as bool?;
            parsed!.Value.Should().Be(value);
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData(null, false)]
        public void should_return_value_nullable(bool? value, bool ignoreDefaultValue)
        {
            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = ignoreDefaultValue };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var parsed = RedisValueFormatter.Read(actual, new RedisValueReaderContext(writerContext, typeof(bool?))) as bool?;
            parsed.Should().BeNull();
        }
    }
}