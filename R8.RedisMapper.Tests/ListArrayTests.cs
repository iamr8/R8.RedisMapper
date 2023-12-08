using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using FluentAssertions;
using StackExchange.Redis;
using Xunit;

namespace R8.RedisMapper.Tests
{
    public class ListArrayTests
    {
        [Fact]
        public void should_return_list_of_string()
        {
            var value = new List<string> { "1", "2" };

            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var readerContext = new RedisValueReaderContext(writerContext, value.GetType());

            var parsed = RedisValueFormatter.Read(actual, readerContext) as List<string>;
            parsed.Should().BeEquivalentTo(value);
        }

        [Fact]
        public void should_return_list_of_bool()
        {
            var value = new List<bool> { true, false };

            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var readerContext = new RedisValueReaderContext(writerContext, value.GetType());

            var parsed = RedisValueFormatter.Read(actual, readerContext) as List<bool>;
            parsed.Should().BeEquivalentTo(value);
        }

        [Fact]
        public void should_return_list_of_int()
        {
            var value = new List<int> { 1, 2 };

            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var readerContext = new RedisValueReaderContext(writerContext, value.GetType());

            var parsed = RedisValueFormatter.Read(actual, readerContext) as List<int>;
            parsed.Should().BeEquivalentTo(value);
        }

        [Fact]
        public void should_return_list_of_long()
        {
            var value = new List<long> { 1, 2 };

            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var readerContext = new RedisValueReaderContext(writerContext, value.GetType());

            var parsed = RedisValueFormatter.Read(actual, readerContext) as List<long>;
            parsed.Should().BeEquivalentTo(value);
        }

        [Fact]
        public void should_return_list_of_double()
        {
            var value = new List<double> { 1.2d, 2.4d };

            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var readerContext = new RedisValueReaderContext(writerContext, value.GetType());

            var parsed = RedisValueFormatter.Read(actual, readerContext) as List<double>;
            parsed.Should().BeEquivalentTo(value);
        }

        [Fact]
        public void should_return_list_of_datetime()
        {
            var value = new List<DateTime> { DateTime.UtcNow, DateTime.UtcNow.AddSeconds(10) };

            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var readerContext = new RedisValueReaderContext(writerContext, value.GetType());

            var parsed = RedisValueFormatter.Read(actual, readerContext) as List<DateTime>;
            parsed.Should().BeEquivalentTo(value);
        }

        [Fact]
        public void should_return_empty_list_of_string()
        {
            var value = new List<string>();

            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = false };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var readerContext = new RedisValueReaderContext(writerContext, value.GetType());

            var parsed = RedisValueFormatter.Read(actual, readerContext) as List<string>;
            parsed.Should().BeEquivalentTo(value);
        }

        [Fact]
        public void should_return_default_list_of_string()
        {
            var value = new List<string>();

            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var readerContext = new RedisValueReaderContext(writerContext, value.GetType());

            var parsed = RedisValueFormatter.Read(actual, readerContext) as List<string>;
            parsed.Should().BeNull();
        }

        [Fact]
        public void should_return_array_of_string()
        {
            var value = new[] { "1", "2" };

            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var readerContext = new RedisValueReaderContext(writerContext, value.GetType());

            var parsed = RedisValueFormatter.Read(actual, readerContext) as string[];
            parsed.Should().BeEquivalentTo(value);
        }

        [Fact]
        public void should_return_array_of_int()
        {
            var value = new[] { 1, 2 };

            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var readerContext = new RedisValueReaderContext(writerContext, value.GetType());

            var parsed = RedisValueFormatter.Read(actual, readerContext) as int[];
            parsed.Should().BeEquivalentTo(value);
        }

        [Fact]
        public void should_return_array_of_long()
        {
            var value = new long[] { 1, 2 };

            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var readerContext = new RedisValueReaderContext(writerContext, value.GetType());

            var parsed = RedisValueFormatter.Read(actual, readerContext) as long[];
            parsed.Should().BeEquivalentTo(value);
        }

        [Fact]
        public void should_return_array_of_double()
        {
            var value = new[] { 1.3d, 2.4d };

            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var readerContext = new RedisValueReaderContext(writerContext, value.GetType());

            var parsed = RedisValueFormatter.Read(actual, readerContext) as double[];
            parsed.Should().BeEquivalentTo(value);
        }

        [Fact]
        public void should_return_array_of_bool()
        {
            var value = new[] { true, false };

            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var readerContext = new RedisValueReaderContext(writerContext, value.GetType());

            var parsed = RedisValueFormatter.Read(actual, readerContext) as bool[];
            parsed.Should().BeEquivalentTo(value);
        }

        public class Dummy
        {
            public string Name { get; set; }
        }

        [Fact]
        public void should_return_array_of_object()
        {
            var value = new[] { new Dummy { Name = "foo" }, new Dummy { Name = "bar" } };

            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var readerContext = new RedisValueReaderContext(writerContext, value.GetType());

            var parsed = RedisValueFormatter.Read(actual, readerContext) as Dummy[];
            parsed.Should().BeEquivalentTo(value);
        }

        [Fact]
        public void should_return_list_of_object()
        {
            var value = new List<Dummy> { new Dummy { Name = "foo" }, new Dummy { Name = "bar" } };

            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var readerContext = new RedisValueReaderContext(writerContext, value.GetType());

            var parsed = RedisValueFormatter.Read(actual, readerContext) as List<Dummy>;
            parsed.Should().BeEquivalentTo(value);
        }

        [Fact]
        public void should_return_array_of_datetime()
        {
            var value = new[] { DateTime.UtcNow, DateTime.UtcNow.AddSeconds(10) };

            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var readerContext = new RedisValueReaderContext(writerContext, value.GetType());

            var parsed = RedisValueFormatter.Read(actual, readerContext) as DateTime[];
            parsed.Should().BeEquivalentTo(value);
        }

        [Fact]
        public void should_return_empty_array_of_string()
        {
            var value = Array.Empty<string>();

            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = false };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var readerContext = new RedisValueReaderContext(writerContext, value.GetType());

            var parsed = RedisValueFormatter.Read(actual, readerContext) as string[];
            parsed.Should().BeEquivalentTo(value);
        }

        [Fact]
        public void should_return_default_array_of_string()
        {
            var value = Array.Empty<string>();

            var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
            var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);

            var readerContext = new RedisValueReaderContext(writerContext, value.GetType());

            var parsed = RedisValueFormatter.Read(actual, readerContext) as string[];
            parsed.Should().BeNull();
        }

        public class ListTests
        {
            public class NestedDummyObj
            {
                public string Name { get; set; }
            }

            public class DummyList
            {
                public List<string> ValueString { get; set; }
                public List<int> ValueInt32 { get; set; }
                public List<int?> ValueInt32Nullable { get; set; }
                public List<long> ValueInt64 { get; set; }
                public List<long?> ValueInt64Nullable { get; set; }
                public List<double> ValueDouble { get; set; }
                public List<double?> ValueDoubleNullable { get; set; }
                public List<bool> ValueBoolean { get; set; }
                public List<bool?> ValueBooleanNullable { get; set; }
                public List<DateTime> ValueDateTime { get; set; }
                public List<DateTime?> ValueDateTimeNullable { get; set; }
                public List<NestedDummyObj> ValueObject { get; set; }
            }

            [Fact]
            public void should_set_get_list_of_string()
            {
                var type = typeof(DummyList);
                var instance = new DummyList();
                var value = new List<string> { "1", "2" };
                var valueJson = JsonSerializer.Serialize(value);
                var redisValue = (RedisValue)valueJson;

                var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyList.ValueString), BindingFlags.Public | BindingFlags.Instance);
                prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

                instance.ValueString.Should().BeEquivalentTo(value);
            }

            [Fact]
            public void should_set_get_list_of_string_by_common_method()
            {
                var type = typeof(DummyList);
                var instance = new DummyList();
                var value = new List<string> { "1", "2" };
                var valueJson = JsonSerializer.Serialize(value);
                var redisValue = (RedisValue)valueJson;

                var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyList.ValueString), BindingFlags.Public | BindingFlags.Instance);
                var set = prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));
                set.Should().BeTrue();

                var v = prop.GetValue(instance, Configuration.ValueFormatters, new RedisValueWriterContext { IgnoreDefaultValue = true });
                v.Should().Be(redisValue);
            }

            [Fact]
            public void should_set_get_list_of_int()
            {
                var type = typeof(DummyList);
                var instance = new DummyList();
                var value = new List<int> { 1, 2 };
                var valueJson = JsonSerializer.Serialize(value);
                var redisValue = (RedisValue)valueJson;

                var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyList.ValueInt32), BindingFlags.Public | BindingFlags.Instance);
                prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

                instance.ValueInt32.Should().BeEquivalentTo(value);
            }

            [Fact]
            public void should_set_get_list_of_int_nullable()
            {
                var type = typeof(DummyList);
                var instance = new DummyList();
                var value = new List<int?> { 1, null, 2 };
                var valueJson = JsonSerializer.Serialize(value);
                var redisValue = (RedisValue)valueJson;

                var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyList.ValueInt32Nullable), BindingFlags.Public | BindingFlags.Instance);
                prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

                instance.ValueInt32Nullable.Should().BeEquivalentTo(value);
            }

            [Fact]
            public void should_set_get_list_of_double()
            {
                var type = typeof(DummyList);
                var instance = new DummyList();
                var value = new List<double> { 1.3d, 24.4d };
                var valueJson = JsonSerializer.Serialize(value);
                var redisValue = (RedisValue)valueJson;

                var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyList.ValueDouble), BindingFlags.Public | BindingFlags.Instance);
                prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

                instance.ValueDouble.Should().BeEquivalentTo(value);
            }

            [Fact]
            public void should_set_get_list_of_double_nullable()
            {
                var type = typeof(DummyList);
                var instance = new DummyList();
                var value = new List<double?> { 1.3d, null, 24.4d };
                var valueJson = JsonSerializer.Serialize(value);
                var redisValue = (RedisValue)valueJson;

                var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyList.ValueDoubleNullable), BindingFlags.Public | BindingFlags.Instance);
                prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

                instance.ValueDoubleNullable.Should().BeEquivalentTo(value);
            }

            [Fact]
            public void should_set_get_list_of_long()
            {
                var type = typeof(DummyList);
                var instance = new DummyList();
                var value = new List<long> { 1, 24 };
                var valueJson = JsonSerializer.Serialize(value);
                var redisValue = (RedisValue)valueJson;

                var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyList.ValueInt64), BindingFlags.Public | BindingFlags.Instance);
                prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

                instance.ValueInt64.Should().BeEquivalentTo(value);
            }

            [Fact]
            public void should_set_get_list_of_long_nullable()
            {
                var type = typeof(DummyList);
                var instance = new DummyList();
                var value = new List<long?> { 1, null, 24 };
                var valueJson = JsonSerializer.Serialize(value);
                var redisValue = (RedisValue)valueJson;

                var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyList.ValueInt64Nullable), BindingFlags.Public | BindingFlags.Instance);
                prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

                instance.ValueInt64Nullable.Should().BeEquivalentTo(value);
            }

            [Fact]
            public void should_set_get_list_of_bool()
            {
                var type = typeof(DummyList);
                var instance = new DummyList();
                var value = new List<bool> { true, false };
                var valueJson = JsonSerializer.Serialize(value);
                var redisValue = (RedisValue)valueJson;

                var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyList.ValueBoolean), BindingFlags.Public | BindingFlags.Instance);
                prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

                instance.ValueBoolean.Should().BeEquivalentTo(value);
            }

            [Fact]
            public void should_set_get_list_of_bool_nullable()
            {
                var type = typeof(DummyList);
                var instance = new DummyList();
                var value = new List<bool?> { true, null, false };
                var valueJson = JsonSerializer.Serialize(value);
                var redisValue = (RedisValue)valueJson;

                var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyList.ValueBooleanNullable), BindingFlags.Public | BindingFlags.Instance);
                prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

                instance.ValueBooleanNullable.Should().BeEquivalentTo(value);
            }

            [Fact]
            public void should_set_get_list_of_datetime()
            {
                var type = typeof(DummyList);
                var instance = new DummyList();
                var value = new List<DateTime> { DateTime.Now, DateTime.Now.AddMinutes(10) };
                var valueJson = JsonSerializer.Serialize(value);
                var redisValue = (RedisValue)valueJson;

                var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyList.ValueDateTime), BindingFlags.Public | BindingFlags.Instance);
                prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

                instance.ValueDateTime.Should().BeEquivalentTo(value);
            }

            [Fact]
            public void should_set_get_list_of_datetime_nullable()
            {
                var type = typeof(DummyList);
                var instance = new DummyList();
                var value = new List<DateTime?> { DateTime.Now, null, DateTime.Now.AddMinutes(10) };
                var valueJson = JsonSerializer.Serialize(value);
                var redisValue = (RedisValue)valueJson;

                var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyList.ValueDateTimeNullable), BindingFlags.Public | BindingFlags.Instance);
                prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

                instance.ValueDateTimeNullable.Should().BeEquivalentTo(value);
            }

            [Fact]
            public void should_set_get_list_of_object()
            {
                var type = typeof(DummyList);
                var instance = new DummyList();
                var value = new List<NestedDummyObj> { new NestedDummyObj { Name = "Foo" }, new NestedDummyObj { Name = "Bar" } };
                var valueJson = JsonSerializer.Serialize(value);
                var redisValue = (RedisValue)valueJson;

                var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyList.ValueObject), BindingFlags.Public | BindingFlags.Instance);
                prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

                instance.ValueObject.Should().BeEquivalentTo(value);
            }
        }

        public class ArrayTests
        {
            public class NestedDummyObj
            {
                public string Name { get; set; }
            }

            public class DummyArray
            {
                public string[] ValueString { get; set; }
                public int[] ValueInt32 { get; set; }
                public long[] ValueInt64 { get; set; }
                public double[] ValueDouble { get; set; }
                public bool[] ValueBoolean { get; set; }
                public DateTime[] ValueDateTime { get; set; }

                public NestedDummyObj[] ValueObject { get; set; }
            }

            [Fact]
            public void should_set_get_array_of_string()
            {
                var type = typeof(DummyArray);
                var instance = new DummyArray();
                var value = new[] { "1", "2" };
                var valueJson = JsonSerializer.Serialize(value);
                var redisValue = (RedisValue)valueJson;

                var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyArray.ValueString), BindingFlags.Public | BindingFlags.Instance);
                prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

                instance.ValueString.Should().BeEquivalentTo(value);
            }

            [Fact]
            public void should_set_get_array_of_int()
            {
                var type = typeof(DummyArray);
                var instance = new DummyArray();
                var value = new[] { 1, 2 };
                var valueJson = JsonSerializer.Serialize(value);
                var redisValue = (RedisValue)valueJson;

                var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyArray.ValueInt32), BindingFlags.Public | BindingFlags.Instance);
                prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

                instance.ValueInt32.Should().BeEquivalentTo(value);
            }

            [Fact]
            public void should_set_get_array_of_double()
            {
                var type = typeof(DummyArray);
                var instance = new DummyArray();
                var value = new[] { 1.3d, 24.4d };
                var valueJson = JsonSerializer.Serialize(value);
                var redisValue = (RedisValue)valueJson;

                var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyArray.ValueDouble), BindingFlags.Public | BindingFlags.Instance);
                prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

                instance.ValueDouble.Should().BeEquivalentTo(value);
            }

            [Fact]
            public void should_set_get_array_of_long()
            {
                var type = typeof(DummyArray);
                var instance = new DummyArray();
                var value = new long[] { 1, 24 };
                var valueJson = JsonSerializer.Serialize(value);
                var redisValue = (RedisValue)valueJson;

                var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyArray.ValueInt64), BindingFlags.Public | BindingFlags.Instance);
                prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

                instance.ValueInt64.Should().BeEquivalentTo(value);
            }

            [Fact]
            public void should_set_get_array_of_bool()
            {
                var type = typeof(DummyArray);
                var instance = new DummyArray();
                var value = new bool[] { true, false };
                var valueJson = JsonSerializer.Serialize(value);
                var redisValue = (RedisValue)valueJson;

                var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyArray.ValueBoolean), BindingFlags.Public | BindingFlags.Instance);
                prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

                instance.ValueBoolean.Should().BeEquivalentTo(value);
            }

            [Fact]
            public void should_set_get_array_of_bool_by_common_method()
            {
                var type = typeof(DummyArray);
                var instance = new DummyArray();
                var value = new bool[] { true, false };
                var valueJson = JsonSerializer.Serialize(value);
                var redisValue = (RedisValue)valueJson;

                var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyArray.ValueBoolean), BindingFlags.Public | BindingFlags.Instance);
                var set = prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));
                set.Should().BeTrue();

                var v = prop.GetValue(instance, Configuration.ValueFormatters, new RedisValueWriterContext { IgnoreDefaultValue = true });
                v.Should().Be(redisValue);
            }

            [Fact]
            public void should_set_get_array_of_datetime()
            {
                var type = typeof(DummyArray);
                var instance = new DummyArray();
                var value = new DateTime[] { DateTime.Now, DateTime.Now.AddMinutes(10) };
                var valueJson = JsonSerializer.Serialize(value);
                var redisValue = (RedisValue)valueJson;

                var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyArray.ValueDateTime), BindingFlags.Public | BindingFlags.Instance);
                prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

                instance.ValueDateTime.Should().BeEquivalentTo(value);
            }

            [Fact]
            public void should_set_get_array_of_object()
            {
                var type = typeof(DummyArray);
                var instance = new DummyArray();
                var value = new NestedDummyObj[] { new NestedDummyObj { Name = "Foo" }, new NestedDummyObj { Name = "Bar" } };
                var valueJson = JsonSerializer.Serialize(value);
                var redisValue = (RedisValue)valueJson;

                var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyArray.ValueObject), BindingFlags.Public | BindingFlags.Instance);
                prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));

                instance.ValueObject.Should().BeEquivalentTo(value);
            }
        }
    }
}