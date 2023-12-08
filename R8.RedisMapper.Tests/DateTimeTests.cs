using System;
using System.Reflection;
using FluentAssertions;
using StackExchange.Redis;
using Xunit;

namespace R8.RedisMapper.Tests
{
    public class DateTimeTests
    {
        public class DummyDateTime
        {
            public DateTime Value { get; set; }
            public DateTime? ValueNullable { get; set; }
        }

        [Fact]
        public void should_set_get()
        {
            var type = typeof(DummyDateTime);
            var instance = new DummyDateTime();
            var value = DateTime.UtcNow;
            var redisValue = (RedisValue)value.Ticks;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyDateTime.Value), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (DateTime?)prop.Property.GetValue(instance);
            v.Should().Be(value);
        }

        [Fact]
        public void should_set_get_by_common_method()
        {
            var type = typeof(DummyDateTime);
            var instance = new DummyDateTime();
            var value = DateTime.UtcNow;
            var redisValue = (RedisValue)value.Ticks;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyDateTime.Value), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType) { IgnoreDefaultValue = true });

            var v = prop.GetValue(instance, Configuration.ValueFormatters, new RedisValueWriterContext { IgnoreDefaultValue = true });
            v.Should().Be(redisValue);
        }

        [Fact]
        public void should_set_get_nullable()
        {
            var type = typeof(DummyDateTime);
            var instance = new DummyDateTime();
            var value = RedisValue.Null;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyDateTime.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, value, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (DateTime?)prop.Property.GetValue(instance);
            v.Should().BeNull();
        }

        [Fact]
        public void should_set_get_nullable_2()
        {
            var type = typeof(DummyDateTime);
            var instance = new DummyDateTime();
            var value = (DateTime?)DateTime.UtcNow;
            var redisValue = (RedisValue)value.Value.Ticks;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyDateTime.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (DateTime?)prop.Property.GetValue(instance);
            v.Should().Be(value);
        }

        [Fact]
        public void should_set_get_nullable_2_by_redis_value()
        {
            var type = typeof(DummyDateTime);
            var instance = new DummyDateTime();
            var redisValue = new RedisValue();

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyDateTime.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (DateTime?)prop.Property.GetValue(instance);
            v.Should().BeNull();
        }

        [Fact]
        public void should_set_get_by_redis_value_nonmatching_type()
        {
            var type = typeof(DummyDateTime);
            var instance = new DummyDateTime();
            var redisValue = (RedisValue)"ghgh";

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyDateTime.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
            Assert.Throws<ArgumentException>(() => prop.Property.SetValue(instance, redisValue));
        }

        [Fact]
        public void should_set_get_nullable_by_redis_value()
        {
            var type = typeof(DummyDateTime);
            var instance = new DummyDateTime();
            var value = DateTime.UtcNow;
            var redisValue = (RedisValue)value.Ticks;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyDateTime.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (DateTime?)prop.Property.GetValue(instance);
            v.Should().BeCloseTo(value, TimeSpan.FromMilliseconds(1));
        }

        [Fact]
        public void should_set_get_by_redis_value()
        {
            var type = typeof(DummyDateTime);
            var instance = new DummyDateTime();
            var value = DateTime.UtcNow;
            var redisValue = (RedisValue)value.Ticks;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyDateTime.Value), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (DateTime?)prop.Property.GetValue(instance);
            v.Should().BeCloseTo(value, TimeSpan.FromMilliseconds(1));
        }

        [Fact]
        public void should_return_value()
        {
            var value = DateTime.UtcNow;
            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var parsed = RedisValueFormatter.Read(actual, new RedisValueReaderContext(writerContext, value.GetType())) as DateTime?;
            parsed!.Value.Should().BeCloseTo(value, TimeSpan.FromMilliseconds(1));
        }

        [Fact]
        public void should_return_default_value_with_null_value_and_ignore_default_values()
        {
            DateTime? value = null;
            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var parsed = RedisValueFormatter.Read(actual, new RedisValueReaderContext(typeof(DateTime?)) { IgnoreDefaultValue = true }) as DateTime?;
            parsed.Should().BeNull();
        }

        [Fact]
        public void should_return_default_value_with_default_value_and_ignore_default_values()
        {
            var value = DateTime.MinValue;
            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var parsed = RedisValueFormatter.Read(actual, new RedisValueReaderContext(writerContext, value.GetType())) as DateTime?;
            parsed!.Should().Be(value);
        }

        [Fact]
        public void should_return_default_value_with_default_value_and_without_ignore_default_values()
        {
            var value = DateTime.MinValue;
            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = false };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var parsed = RedisValueFormatter.Read(actual, new RedisValueReaderContext(writerContext, value.GetType())) as DateTime?;
            parsed!.Value.Should().BeCloseTo(DateTime.MinValue, TimeSpan.FromMilliseconds(1));
        }
    }
}