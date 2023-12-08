using System;
using System.Reflection;
using FluentAssertions;
using StackExchange.Redis;
using Xunit;

namespace R8.RedisMapper.Tests
{
    public class DoubleTests
    {
        public class DummyDouble
        {
            public double Value { get; set; }
            public double? ValueNullable { get; set; }
        }

        [Theory]
        [InlineData(nameof(DummyDouble.Value), (double)5.4)]
        public void should_set_get(string propName, double value)
        {
            var type = typeof(DummyDouble);
            var instance = new DummyDouble();

            var prop = (CachedPropertyInfo)type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, value, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (double?)prop.Property.GetValue(instance);
            v.Should().Be(value);
        }

        [Theory]
        [InlineData(nameof(DummyDouble.Value), (double)5.4)]
        public void should_set_get_by_common_method(string propName, double value)
        {
            var type = typeof(DummyDouble);
            var instance = new DummyDouble();

            var prop = (CachedPropertyInfo)type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            var set = prop.SetValue(instance, value, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = prop.GetValue(instance, Configuration.ValueFormatters, new RedisValueWriterContext { IgnoreDefaultValue = true });
            v.Should().Be(value);
        }

        [Theory]
        [InlineData(nameof(DummyDouble.ValueNullable), (double)3.4)]
        [InlineData(nameof(DummyDouble.ValueNullable), null)]
        public void should_set_get_nullable(string propName, double? value)
        {
            var type = typeof(DummyDouble);
            var instance = new DummyDouble();

            var prop = (CachedPropertyInfo)type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, value, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (double?)prop.Property.GetValue(instance);
            v.Should().Be(value);
        }

        [Fact]
        public void should_set_get_nullable_2_by_redis_value()
        {
            var type = typeof(DummyDouble);
            var instance = new DummyDouble();
            var redisValue = new RedisValue();

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyDouble.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (double?)prop.Property.GetValue(instance);
            v.Should().BeNull();
        }

        [Fact]
        public void should_set_get_by_redis_value_nonmatching_type()
        {
            var type = typeof(DummyDouble);
            var instance = new DummyDouble();
            var redisValue = (RedisValue)"ghgh";

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyDouble.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
            Assert.Throws<ArgumentException>(() => prop.Property.SetValue(instance, redisValue));
        }

        [Fact]
        public void should_set_get_by_redis_value()
        {
            var type = typeof(DummyDouble);
            var instance = new DummyDouble();
            var value = 3.4d;
            var redisValue = (RedisValue)value;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyDouble.Value), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (double?)prop.Property.GetValue(instance);
            v.Should().Be(value);
        }

        [Fact]
        public void should_set_get_nullable_by_redis_value()
        {
            var type = typeof(DummyDouble);
            var instance = new DummyDouble();
            var value = 3.4d;
            var redisValue = (RedisValue)value;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyDouble.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (double?)prop.Property.GetValue(instance);
            v.Should().Be(value);
        }

        [Theory]
        [InlineData(1.3d, false)]
        [InlineData((double)0, true)]
        [InlineData((double)0, false)]
        public void should_return_value(double value, bool ignoreDefaultValue)
        {
            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = ignoreDefaultValue };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var parsed = RedisValueFormatter.Read(actual, new RedisValueReaderContext(writerContext, value.GetType()));
            parsed!.Should().Be(value);
        }

        [Theory]
        [InlineData(1.3d, false, false)]
        [InlineData(null, true, true)]
        [InlineData(0.0d, true, true)]
        public void should_return_value_nullable(double? value, bool ignoreDefaultValue, bool nullExpected)
        {
            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = ignoreDefaultValue };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var parsed = RedisValueFormatter.Read(actual, new RedisValueReaderContext(writerContext, typeof(double?)));
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