// using System;
// using System.Reflection;
// using FluentAssertions;
// using StackExchange.Redis;
// using Xunit;
//
// namespace R8.RedisMapper.Tests
// {
//     public class EnumTests
//     {
//         public enum EDummyEnum
//         {
//             None = 0,
//             Chosen = 1,
//         }
//
//         public enum EDummyEnum2 : long
//         {
//             None = 2,
//             Chosen = 3,
//         }
//
//         public class DummyEnum
//         {
//             public EDummyEnum Value { get; set; }
//             public EDummyEnum? ValueNullable { get; set; }
//         }
//
//         [Theory]
//         [InlineData(nameof(DummyEnum.ValueNullable), EDummyEnum.Chosen)]
//         [InlineData(nameof(DummyEnum.ValueNullable), null)]
//         public void should_set_get_nullable(string propName, EDummyEnum? value)
//         {
//             var type = typeof(DummyEnum);
//             var instance = new DummyEnum();
//             var redisValue = (RedisValue)(int?)value;
//
//             var prop = (CachedPropertyInfo)type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
//             prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));
//
//             var v = prop.Property.GetValue(instance) as EDummyEnum?;
//             v.Should().Be(value);
//         }
//
//         [Theory]
//         [InlineData(nameof(DummyEnum.Value), EDummyEnum.None)]
//         public void should_set_get(string propName, EDummyEnum value)
//         {
//             var type = typeof(DummyEnum);
//             var instance = new DummyEnum();
//             var redisValue = (RedisValue)(int?)value;
//
//             var prop = (CachedPropertyInfo)type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
//             prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));
//
//             var v = prop.Property.GetValue(instance) as EDummyEnum?;
//             v.Should().Be(value);
//         }
//
//         [Fact]
//         public void should_set_get_by_common_method()
//         {
//             var type = typeof(DummyEnum);
//             var instance = new DummyEnum();
//             var value = EDummyEnum.Chosen;
//             var redisValue = (RedisValue)(int)value;
//
//             var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyEnum.Value), BindingFlags.Public | BindingFlags.Instance);
//             prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));
//
//             var v = prop.GetValue(instance, Configuration.ValueFormatters, new RedisValueWriterContext { IgnoreDefaultValue = true });
//             v.Should().Be(redisValue);
//         }
//
//         [Fact]
//         public void should_set_get_2_by_redis_value()
//         {
//             var type = typeof(DummyEnum);
//             var instance = new DummyEnum();
//             var redisValue = new RedisValue();
//
//             var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyEnum.Value), BindingFlags.Public | BindingFlags.Instance);
//             prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));
//
//             var v = prop.Property.GetValue(instance) as EDummyEnum?;
//             v.Should().Be(EDummyEnum.None);
//         }
//
//         [Fact]
//         public void should_set_get_nullable_2_by_redis_value()
//         {
//             var type = typeof(DummyEnum);
//             var instance = new DummyEnum();
//             var redisValue = new RedisValue();
//
//             var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyEnum.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
//             prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));
//
//             var v = prop.Property.GetValue(instance) as EDummyEnum?;
//             v.Should().BeNull();
//         }
//
//         [Fact]
//         public void should_set_get_by_redis_value_nonmatching_type()
//         {
//             var type = typeof(DummyEnum);
//             var instance = new DummyEnum();
//             var redisValue = (RedisValue)"ghgh";
//
//             var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyEnum.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
//             Assert.Throws<ArgumentException>(() => prop.Property.SetValue(instance, redisValue));
//         }
//
//         [Fact]
//         public void should_set_get_by_redis_value()
//         {
//             var type = typeof(DummyEnum);
//             var instance = new DummyEnum();
//             var value = EDummyEnum.Chosen;
//             var redisValue = (RedisValue)(int)value;
//
//             var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyEnum.Value), BindingFlags.Public | BindingFlags.Instance);
//             prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));
//
//             var v = prop.Property.GetValue(instance) as EDummyEnum?;
//             v.Should().Be(value);
//         }
//
//         [Fact]
//         public void should_set_get_nullable_by_redis_value()
//         {
//             var type = typeof(DummyEnum);
//             var instance = new DummyEnum();
//             var value = EDummyEnum.Chosen;
//             var redisValue = (RedisValue)(int)value;
//
//             var prop = (CachedPropertyInfo)type.GetProperty(nameof(DummyEnum.ValueNullable), BindingFlags.Public | BindingFlags.Instance);
//             prop.SetValue(instance, redisValue, Configuration.ValueFormatters, new RedisValueReaderContext(prop.PropertyType));
//
//             var v = prop.Property.GetValue(instance) as EDummyEnum?;
//             v.Should().Be(value);
//         }
//
//         [Theory]
//         [InlineData(EDummyEnum.None)]
//         [InlineData(EDummyEnum.Chosen)]
//         public void should_return_value_from_enumint(EDummyEnum value)
//         {
//             var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = true };
//             var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);
//
//             var parsed = RedisValueFormatter.Read(actual, new RedisValueReaderContext(writerContext, value.GetType())) as EDummyEnum?;
//             parsed!.Should().Be(value);
//         }
//
//         [Theory]
//         [InlineData(null, true, true)]
//         [InlineData(EDummyEnum.None, true, true)]
//         [InlineData(EDummyEnum.None, false, false)]
//         [InlineData(EDummyEnum.Chosen, false, false)]
//         public void should_return_value_nullable(EDummyEnum? value, bool ignoreDefaultValue, bool nullExpected)
//         {
//             var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = ignoreDefaultValue };
//             var actual = RedisValueExtensions.GetRedisValue(value, Configuration.ValueFormatters, writerContext);
//
//             var reader = new RedisValueReaderContext(writerContext, typeof(EDummyEnum?));
//             var parsed = RedisValueFormatter.Read(actual, reader) as EDummyEnum?;
//             if (nullExpected)
//             {
//                 parsed.Should().BeNull();
//             }
//             else
//             {
//                 parsed.Should().Be(value);
//             }
//         }
//         
//         public enum DummyEnumE
//         {
//             Value1 = 0,
//             Value2 = 1
//         }
//
//         public class DummyEnumClass
//         {
//             public DummyEnum Value { get; set; }
//         }
//
//         public class DummyEnumValueFormatter : RedisValueFormatter<DummyEnumE>
//         {
//             public override RedisValue Write(DummyEnumE? value, RedisValueWriterContext context)
//             {
//                 if (value.HasValue)
//                 {
//                     if (context.IgnoreDefaultValue && value == 0)
//                         return RedisValue.Null;
//
//                     return value.ToString();
//                 }
//
//                 return RedisValue.Null;
//             }
//
//             public override DummyEnumE? Read(RedisValue value, RedisValueReaderContext context)
//             {
//                 if (value.IsNullOrEmpty)
//                 {
//                     if (context.IgnoreDefaultValue) return null;
//                     else return default(DummyEnumE);
//                 }
//
//                 var str = value.ToString();
//                 if (Enum.TryParse<DummyEnumE>(str, out var result))
//                     return result;
//
//                 return null;
//             }
//         }
//
//         [Theory]
//         [InlineData(DummyEnumE.Value2, false, false)]
//         [InlineData(null, true, true)]
//         [InlineData(DummyEnumE.Value1, true, true)]
//         public void should_return_value_with_formatter(DummyEnumE? value, bool ignoreDefaultValue, bool nullExpected)
//         {
//             var writerContext = new RedisValueWriterContext { IgnoreDefaultValue = ignoreDefaultValue };
//             var valueType = typeof(DummyEnumE);
//             var context = new RedisValueWriterContext(writerContext, valueType);
//             var formatter = new DummyEnumValueFormatter();
//             var rv = formatter.WriteCore(value, context);
//
//             if (nullExpected)
//             {
//                 rv.IsNullOrEmpty.Should().BeTrue();
//             }
//             else
//             { 
//                 rv.HasValue.Should().BeTrue();
//                 var p = (string)rv;
//                 p.Should().Be(value.ToString());
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