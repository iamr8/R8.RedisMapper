using System;
using System.Reflection;
using FluentAssertions;
using StackExchange.Redis;
using Xunit;

namespace R8.RedisMapper.Tests
{
    public class Int64Tests
    {
        public class DummyInt64
        {
            public long Value { get; set; }
            public long? ValueNullable { get; set; }
        }

        [Theory]
        [InlineData(nameof(DummyInt64.Value), 5)]
        public void should_set_get(string propName, long value)
        {
            var type = typeof(DummyInt64);
            var instance = new DummyInt64();

            var prop = (CachedPropertyInfo)type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, value, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (long?)prop.Property.GetValue(instance);
            v.Should().Be(value);
        }

        [Theory]
        [InlineData(nameof(DummyInt64.Value), 5)]
        public void should_set_get_by_common_method(string propName, long value)
        {
            var type = typeof(DummyInt64);
            var instance = new DummyInt64();

            var prop = (CachedPropertyInfo)type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            var set = prop.SetValue(instance, value, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));
            set.Should().BeTrue();

            var v = prop.GetValue(instance, Configuration.ValueFormatters, new RedisValueWriterContext { IgnoreDefaultValue = true });
            v.Should().Be(value);
        }

        [Theory]
        [InlineData(nameof(DummyInt64.ValueNullable), (long)3)]
        [InlineData(nameof(DummyInt64.ValueNullable), null)]
        public void should_set_get_nullable(string propName, long? value)
        {
            var type = typeof(DummyInt64);
            var instance = new DummyInt64();

            var prop = (CachedPropertyInfo)type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, value, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (long?)prop.Property.GetValue(instance);
            v.Should().Be(value);
        }

        [Fact]
        public void should_set_get_nullable_2_by_redis_value()
        {
            var type = typeof(DummyInt64);
            var instance = new DummyInt64();
            var redisValue = new RedisValue();

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyInt64.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (long?)prop.Property.GetValue(instance);
            v.Should().BeNull();
        }

        [Fact]
        public void should_set_get_by_redis_value_nonmatching_type()
        {
            var type = typeof(DummyInt64);
            var instance = new DummyInt64();
            var redisValue = (RedisValue)"ghgh";

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyInt64.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
            Assert.Throws<ArgumentException>(() => prop.Property.SetValue(instance, redisValue));
        }

        [Fact]
        public void should_set_get_by_redis_value()
        {
            var type = typeof(DummyInt64);
            var instance = new DummyInt64();
            long value = 3;
            var redisValue = (RedisValue)value;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyInt64.Value), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (long?)prop.Property.GetValue(instance);
            v.Should().Be(value);
        }

        [Fact]
        public void should_set_get_nullable_by_redis_value()
        {
            var type = typeof(DummyInt64);
            var instance = new DummyInt64();
            long value = 3;
            var redisValue = (RedisValue)value;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyInt64.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (long?)prop.Property.GetValue(instance);
            v.Should().Be(value);
        }

        [Theory]
        [InlineData((long)1, false)]
        [InlineData((long)0, true)]
        [InlineData((long)0, false)]
        public void should_return_value(long value, bool ignoreDefaultValue)
        {
            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = ignoreDefaultValue };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var parsed = RedisValueFormatter.Read(actual, new RedisValueReaderContext(writerContext, value.GetType()));
            parsed!.Should().Be(value);
        }

        [Theory]
        [InlineData((long)1, false, false)]
        [InlineData(null, true, true)]
        [InlineData((long)0, true, true)]
        public void should_return_value_nullable(long? value, bool ignoreDefaultValue, bool nullExpected)
        {
            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = ignoreDefaultValue };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var parsed = RedisValueFormatter.Read(actual, new RedisValueReaderContext(writerContext, typeof(int?)));
            if (nullExpected)
            {
                parsed!.Should().BeNull();
            }
            else
            {
                parsed!.Should().Be(value);
            }
        }
    }
}