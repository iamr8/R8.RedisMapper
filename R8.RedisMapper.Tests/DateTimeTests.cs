// using System;
// using System.Collections.Generic;
// using System.Reflection;
// using FluentAssertions;
// using StackExchange.Redis;
// using Xunit;
//
// namespace R8.RedisMapper.Tests
// {
//     public class DateTimeTests
//     {
//         public class DummyDateTime
//         {
//             public DateTime Value { get; set; }
//             public DateTime? ValueNullable { get; set; }
//         }
//
//         [Fact]
//         public void should_set_get()
//         {
//             var type = typeof(DummyDateTime);
//             var instance = new DummyDateTime();
//             var value = DateTime.UtcNow;
//             var redisValue = (RedisValue)value.Ticks;
//
//             var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyDateTime.Value), BindingFlags.Public | BindingFlags.Instance);
//             prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));
//
//             var v = (DateTime?)prop.Property.GetValue(instance);
//             v.Should().Be(value);
//         }
//
//         [Fact]
//         public void should_set_get_by_common_method()
//         {
//             var type = typeof(DummyDateTime);
//             var instance = new DummyDateTime();
//             var value = DateTime.UtcNow;
//             var redisValue = (RedisValue)value.Ticks;
//
//             var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyDateTime.Value), BindingFlags.Public | BindingFlags.Instance);
//             prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType) { IgnoreDefaultValue = true });
//
//             var v = prop.GetValue(instance, Configuration.ValueFormatters, new RedisValueWriterContext { IgnoreDefaultValue = true });
//             v.Should().Be(redisValue);
//         }
//
//         [Fact]
//         public void should_set_get_nullable()
//         {
//             var type = typeof(DummyDateTime);
//             var instance = new DummyDateTime();
//             var value = RedisValue.Null;
//
//             var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyDateTime.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
//             prop.SetValue(instance, value, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));
//
//             var v = (DateTime?)prop.Property.GetValue(instance);
//             v.Should().BeNull();
//         }
//
//         [Fact]
//         public void should_set_get_nullable_2()
//         {
//             var type = typeof(DummyDateTime);
//             var instance = new DummyDateTime();
//             var value = (DateTime?)DateTime.UtcNow;
//             var redisValue = (RedisValue)value.Value.Ticks;
//
//             var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyDateTime.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
//             prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));
//
//             var v = (DateTime?)prop.Property.GetValue(instance);
//             v.Should().Be(value);
//         }
//
//         [Fact]
//         public void should_set_get_nullable_2_by_redis_value()
//         {
//             var type = typeof(DummyDateTime);
//             var instance = new DummyDateTime();
//             var redisValue = new RedisValue();
//
//             var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyDateTime.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
//             prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));
//
//             var v = (DateTime?)prop.Property.GetValue(instance);
//             v.Should().BeNull();
//         }
//
//         [Fact]
//         public void should_set_get_by_redis_value_nonmatching_type()
//         {
//             var type = typeof(DummyDateTime);
//             var instance = new DummyDateTime();
//             var redisValue = (RedisValue)"ghgh";
//
//             var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyDateTime.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
//             Assert.Throws<ArgumentException>(() => prop.Property.SetValue(instance, redisValue));
//         }
//
//         [Fact]
//         public void should_set_get_nullable_by_redis_value()
//         {
//             var type = typeof(DummyDateTime);
//             var instance = new DummyDateTime();
//             var value = DateTime.UtcNow;
//             var redisValue = (RedisValue)value.Ticks;
//
//             var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyDateTime.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
//             prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));
//
//             var v = (DateTime?)prop.Property.GetValue(instance);
//             v.Should().BeCloseTo(value, TimeSpan.FromMilliseconds(1));
//         }
//
//         [Fact]
//         public void should_set_get_by_redis_value()
//         {
//             var type = typeof(DummyDateTime);
//             var instance = new DummyDateTime();
//             var value = DateTime.UtcNow;
//             var redisValue = (RedisValue)value.Ticks;
//
//             var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyDateTime.Value), BindingFlags.Public | BindingFlags.Instance);
//             prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));
//
//             var v = (DateTime?)prop.Property.GetValue(instance);
//             v.Should().BeCloseTo(value, TimeSpan.FromMilliseconds(1));
//         }
//
//         [Fact]
//         public void should_return_value()
//         {
//             var value = DateTime.UtcNow;
//             var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
//             var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);
//
//             var parsed = RedisValueFormatter.Read(actual, new RedisValueReaderContext(writerContext, value.GetType())) as DateTime?;
//             parsed!.Value.Should().BeCloseTo(value, TimeSpan.FromMilliseconds(1));
//         }
//
//         [Fact]
//         public void should_return_default_value_with_null_value_and_ignore_default_values()
//         {
//             DateTime? value = null;
//             var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
//             var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);
//
//             var parsed = RedisValueFormatter.Read(actual, new RedisValueReaderContext(typeof(DateTime?)) { IgnoreDefaultValue = true }) as DateTime?;
//             parsed.Should().BeNull();
//         }
//
//         [Fact]
//         public void should_return_default_value_with_default_value_and_ignore_default_values()
//         {
//             var value = DateTime.MinValue;
//             var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
//             var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);
//
//             var parsed = RedisValueFormatter.Read(actual, new RedisValueReaderContext(writerContext, value.GetType())) as DateTime?;
//             parsed!.Should().Be(value);
//         }
//
//         [Fact]
//         public void should_return_default_value_with_default_value_and_without_ignore_default_values()
//         {
//             var value = DateTime.MinValue;
//             var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = false };
//             var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);
//
//             var parsed = RedisValueFormatter.Read(actual, new RedisValueReaderContext(writerContext, value.GetType())) as DateTime?;
//             parsed!.Value.Should().BeCloseTo(DateTime.MinValue, TimeSpan.FromMilliseconds(1));
//         }
//         
//         public class DateTimeValueFormatter : RedisValueFormatter<DateTime>
//         {
//             public override RedisValue Write(DateTime? value, RedisValueWriterContext context)
//             {
//                 if (context.IgnoreDefaultValue && (value is null || value == DateTime.MinValue))
//                     return default;
//
//                 if (value is null)
//                     return default;
//                 
//                 return value.Value.Ticks;
//             }
//
//             public override DateTime? Read(RedisValue value, RedisValueReaderContext context)
//             {
//                 if (value.IsNullOrEmpty)
//                 {
//                     if (context.IgnoreDefaultValue) return null;
//                     else return DateTime.MinValue;
//                 }
//                 
//                 if (!value.TryParse(out long ticks))
//                     throw new InvalidCastException($"Cannot cast {value} to {typeof(long)}");
//
//                 return new DateTime(ticks);
//             }
//         }
//
//         public static TheoryData<DateTime?, bool, bool> should_return_value_with_formatter_memberdata()
//         {
//             return new TheoryData<DateTime?, bool, bool>
//             {
//                 { DateTime.UtcNow, true, false },
//                 { DateTime.UtcNow, false, false },
//                 { null, true, true },
//                 { DateTime.MinValue, true, true },
//                 { DateTime.MinValue, false, false }
//             };
//         }
//         
//         [Theory]
//         [MemberData(nameof(should_return_value_with_formatter_memberdata))]
//         public void should_return_value_with_formatter(DateTime? value, bool ignoreDefaultValue, bool nullExpected)
//         {
//             var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = ignoreDefaultValue };
//             var valueType = typeof(DateTime);
//             var context = new RedisValueWriterContext(writerContext, valueType);
//             var formatter = new DateTimeValueFormatter();
//             var rv = formatter.WriteCore(value, context);
//
//             var p = (long)rv;
//             if (nullExpected)
//             {
//                 rv.IsNullOrEmpty.Should().BeTrue();
//             }
//             else
//             {
//                 rv.IsInteger.Should().BeTrue();
//                 rv.HasValue.Should().BeTrue();
//                 rv.TryParse(out long i).Should().BeTrue();
//                 i.Should().Be(p);
//             }
//             
//             var readerContext = new RedisValueReaderContext(writerContext, valueType);
//             var parsed = formatter.ReadCore(rv, readerContext);
//             if (nullExpected)
//             {
//                 parsed.Should().BeNull();
//             }
//             else
//             {
//                 parsed.Should().Be(value);
//             }
//         }
//     }
// }