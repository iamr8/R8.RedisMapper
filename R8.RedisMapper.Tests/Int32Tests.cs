using System;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using StackExchange.Redis;
using Xunit;

namespace R8.RedisMapper.Tests
{
    public class Int32Tests
    {
        public class DummyInt32
        {
            public int Value { get; set; }
            public int? ValueNullable { get; set; }
        }

        [Theory]
        [InlineData(nameof(DummyInt32.ValueNullable), 3)]
        [InlineData(nameof(DummyInt32.ValueNullable), null)]
        public void should_set_get_nullable(string propName, int? value)
        {
            var type = typeof(DummyInt32);
            var instance = new DummyInt32();

            var prop = (CachedPropertyInfo)type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, value, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (int?)prop.Property.GetValue(instance);
            v.Should().Be(value);
        }

        [Theory]
        [InlineData(nameof(DummyInt32.Value), 5)]
        public void should_set_get(string propName, int value)
        {
            var type = typeof(DummyInt32);
            var instance = new DummyInt32();

            var prop = (CachedPropertyInfo)type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, value, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (int?)prop.Property.GetValue(instance);
            v.Should().Be(value);
        }

        [Theory]
        [InlineData(nameof(DummyInt32.Value), 5)]
        public void should_set_get_by_common_method(string propName, int value)
        {
            var type = typeof(DummyInt32);
            var instance = new DummyInt32();

            var prop = (CachedPropertyInfo)type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);

            var thread1 = new ThreadStart(() =>
            {
                var set = prop.SetValue(instance, value, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));
                set.Should().BeTrue();
            });
            thread1.Invoke();

            var thread2 = new ThreadStart(() =>
            {
                var v = prop.GetValue(instance, Configuration.ValueFormatters, new RedisValueWriterContext { IgnoreDefaultValue = true });
                v.Should().Be(value);
            });
            thread2.Invoke();
        }

        [Fact]
        public void should_set_get_nullable_2_by_redis_value()
        {
            var type = typeof(DummyInt32);
            var instance = new DummyInt32();
            var redisValue = new RedisValue();

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyInt32.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (int?)prop.Property.GetValue(instance);
            v.Should().BeNull();
        }

        [Fact]
        public void should_set_get_by_redis_value_nonmatching_type()
        {
            var type = typeof(DummyInt32);
            var instance = new DummyInt32();
            var redisValue = (RedisValue)"ghgh";

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyInt32.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
            Assert.Throws<ArgumentException>(() => prop.Property.SetValue(instance, redisValue));
        }

        [Fact]
        public void should_set_get_by_redis_value()
        {
            var type = typeof(DummyInt32);
            var instance = new DummyInt32();
            var value = 3;
            var redisValue = (RedisValue)value;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyInt32.Value), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (int?)prop.Property.GetValue(instance);
            v.Should().Be(value);
        }

        [Fact]
        public void should_set_get_nullable_by_redis_value()
        {
            var type = typeof(DummyInt32);
            var instance = new DummyInt32();
            var value = 3;
            var redisValue = (RedisValue)value;

            var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyInt32.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
            prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

            var v = (int?)prop.Property.GetValue(instance);
            v.Should().Be(value);
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(0, true)]
        [InlineData(0, false)]
        public void should_return_value(int value, bool ignoreDefaultValue)
        {
            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = ignoreDefaultValue };
            var actual = RedisValueFormatter.Write(value, writerContext);

            var readerContext = new RedisValueReaderContext(writerContext, value.GetType());
            var parsed = RedisValueFormatter.Read(actual, readerContext);
            parsed!.Should().Be(value);
        }

        [Theory]
        [InlineData(1, false, false)]
        [InlineData(null, true, true)]
        [InlineData(0, true, true)]
        public void should_return_value_nullable(int? value, bool ignoreDefaultValue, bool nullExpected)
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