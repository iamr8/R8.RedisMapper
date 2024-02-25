// using System;
// using System.Reflection;
// using FluentAssertions;
// using Xunit;
//
// namespace R8.RedisMapper.Tests
// {
//     public class TypeReflectionsTests
//     {
//         public class PublicPropertiesTests
//         {
//             private PropertyInfo GetPropertyInfo()
//             {
//                 return typeof(TestClass).GetProperty("TestProperty");
//             }
//
//             private class TestClass
//             {
//                 public int TestProperty { get; set; }
//             }
//
//             private class NullTestClass
//             {
//                 public int? NullableTestProperty { get; set; }
//             }
//
//             [Fact]
//             public void should_check_all_properties_of_cached_property()
//             {
//                 var propertyInfo = GetPropertyInfo();
//                 var cachedPropertyInfo = new CachedPropertyInfo("TestProperty", propertyInfo);
//
//                 cachedPropertyInfo.FormattedName.Should().Be("TestProperty");
//                 cachedPropertyInfo.Property.Should().BeSameAs(propertyInfo);
//                 cachedPropertyInfo.PropertyType.Should().Be(typeof(int));
//                 cachedPropertyInfo.IsNullable.Should().BeFalse();
//             }
//
//             [Fact]
//             public void should_not_be_equal_to_null()
//             {
//                 var propertyInfo = GetPropertyInfo();
//                 var cachedPropertyInfo = new CachedPropertyInfo("TestProperty", propertyInfo);
//
//                 var isEquals = cachedPropertyInfo.Equals(null);
//
//                 isEquals.Should().BeFalse();
//             }
//
//             [Fact]
//             public void should_be_equal_by_Equals_when_properties_are_same()
//             {
//                 var propertyInfo = GetPropertyInfo();
//                 var cachedPropertyInfo1 = new CachedPropertyInfo("TestProperty", propertyInfo);
//                 var cachedPropertyInfo2 = new CachedPropertyInfo("TestProperty", propertyInfo);
//
//                 var isEquals = cachedPropertyInfo1.Equals((object)cachedPropertyInfo2);
//
//                 isEquals.Should().BeTrue();
//             }
//
//             [Fact]
//             public void should_not_be_equal_by_Equals_when_properties_are_not_same()
//             {
//                 var propertyInfo1 = GetPropertyInfo();
//                 var propertyInfo2 = typeof(NullTestClass).GetProperty("NullableTestProperty");
//                 var cachedPropertyInfo1 = new CachedPropertyInfo("TestProperty", propertyInfo1);
//                 var cachedPropertyInfo2 = new CachedPropertyInfo("NullableTestProperty", propertyInfo2);
//
//                 var isEquals = cachedPropertyInfo1.Equals((object)cachedPropertyInfo2);
//
//                 isEquals.Should().BeFalse();
//             }
//
//             [Fact]
//             public void should_return_true_by_EQUAL_operator_when_properties_are_same()
//             {
//                 var propertyInfo = GetPropertyInfo();
//                 var cachedPropertyInfo1 = new CachedPropertyInfo("TestProperty", propertyInfo);
//                 var cachedPropertyInfo2 = new CachedPropertyInfo("TestProperty", propertyInfo);
//
//                 (cachedPropertyInfo1 == cachedPropertyInfo2).Should().BeTrue();
//             }
//
//             [Fact]
//             public void should_return_false_by_NOT_EQUAL_operator_when_properties_are_same()
//             {
//                 var propertyInfo = GetPropertyInfo();
//                 var cachedPropertyInfo1 = new CachedPropertyInfo("TestProperty", propertyInfo);
//                 var cachedPropertyInfo2 = new CachedPropertyInfo("TestProperty", propertyInfo);
//
//                 (cachedPropertyInfo1 != cachedPropertyInfo2).Should().BeFalse();
//             }
//
//             [Fact]
//             public void should_return_false_by_EQUAL_operator_when_properties_are_not_same()
//             {
//                 var propertyInfo1 = GetPropertyInfo();
//                 var propertyInfo2 = typeof(NullTestClass).GetProperty("NullableTestProperty");
//                 var cachedPropertyInfo1 = new CachedPropertyInfo("TestProperty", propertyInfo1);
//                 var cachedPropertyInfo2 = new CachedPropertyInfo("NullableTestProperty", propertyInfo2);
//
//                 (cachedPropertyInfo1 == cachedPropertyInfo2).Should().BeFalse();
//             }
//
//             [Fact]
//             public void should_return_true_by_NOT_EQUAL_operator_when_properties_are_not_same()
//             {
//                 var propertyInfo1 = GetPropertyInfo();
//                 var propertyInfo2 = typeof(NullTestClass).GetProperty("NullableTestProperty");
//                 var cachedPropertyInfo1 = new CachedPropertyInfo("TestProperty", propertyInfo1);
//                 var cachedPropertyInfo2 = new CachedPropertyInfo("NullableTestProperty", propertyInfo2);
//
//                 (cachedPropertyInfo1 != cachedPropertyInfo2).Should().BeTrue();
//             }
//
//             [Fact]
//             public void should_cast_to_cachedpropertyinfo_explicitly()
//             {
//                 var propertyInfo = GetPropertyInfo();
//                 var cachedPropertyInfo = (CachedPropertyInfo)propertyInfo;
//
//                 cachedPropertyInfo.FormattedName.Should().Be("TestProperty");
//                 cachedPropertyInfo.Property.Should().BeSameAs(propertyInfo);
//                 cachedPropertyInfo.PropertyType.Should().Be(typeof(int));
//                 cachedPropertyInfo.IsNullable.Should().BeFalse();
//             }
//
//             [Fact]
//             public void should_return_an_empty_cached_property()
//             {
//                 var empty = CachedPropertyInfo.Empty;
//
//                 empty.FormattedName.Should().BeNull();
//                 empty.Property.Should().BeNull();
//                 empty.PropertyType.Should().BeNull();
//                 empty.IsNullable.Should().BeFalse();
//             }
//
//             public class Dummy1
//             {
//                 public string Name { get; set; }
//             }
//             //
//             // [Fact]
//             // public void should_return_public_properties()
//             // {
//             //     // Assert
//             //     var type = typeof(Dummy1);
//             //
//             //     // Act
//             //     var props = type.GetProperties(Array.Empty<string>(), new RedisFieldCamelCaseFormatter()).ToArray();
//             //
//             //     // Assert
//             //     props.Should().ContainSingle();
//             //     props[0].Property.Name.Should().Be(nameof(Dummy1.Name));
//             //     props[0].FormattedName.Should().Be("name");
//             // }
//             //
//             // [Fact]
//             // public void should_return_public_properties_from_cache()
//             // {
//             //     // Assert
//             //     var type = typeof(Dummy1);
//             //
//             //     // Act
//             //     var props = type.GetProperties(Array.Empty<string>(), new RedisFieldCamelCaseFormatter()).ToArray();
//             //
//             //     var props2 = type.GetProperties(Array.Empty<string>(), new RedisFieldCamelCaseFormatter()).ToArray();
//             //
//             //     // Assert
//             //     props2.Should().ContainSingle();
//             //     props2[0].Property.Name.Should().Be(nameof(Dummy1.Name));
//             //     props2[0].FormattedName.Should().Be("name");
//             // }
//
//             public class Dummy2 : Dummy1
//             {
//                 public string LastName { get; set; }
//             }
//             //
//             // [Fact]
//             // public void should_return_public_properties_plus_superclass_type_public_properties()
//             // {
//             //     // Assert
//             //     var type = typeof(Dummy2);
//             //
//             //     // Act
//             //     var props = type.GetProperties(Array.Empty<string>(), new RedisFieldCamelCaseFormatter()).ToArray();
//             //
//             //     // Assert
//             //     props.Should().HaveCount(2);
//             //     props.Should().Contain(x => x.Property.Name == nameof(Dummy1.Name) && x.FormattedName == "name");
//             //     props.Should().Contain(x => x.Property.Name == nameof(Dummy2.LastName) && x.FormattedName == "lastName");
//             // }
//
//             public interface IDummy1
//             {
//                 string Name { get; set; }
//             }
//
//             public interface IDummy2 : IDummy1
//             {
//                 string Name { get; set; }
//                 string LastName { get; set; }
//             }
//
//             public interface IDummy3 : IDummy2
//             {
//                 int Age { get; set; }
//             }
//
//             [Fact]
//             public void should_return_public_properties_plus_superclass_type_public_properties_by_interface()
//             {
//                 // Assert
//                 var type = typeof(IDummy2);
//
//                 // Act
//                 var props = type.GetProperties(Array.Empty<string>(), new RedisFieldCamelCaseFormatter()).ToArray();
//
//                 // Assert
//                 props.Should().HaveCount(2);
//                 props.Should().Contain(x => x.Property.Name == nameof(IDummy1.Name) && x.FormattedName == "name");
//                 props.Should().Contain(x => x.Property.Name == nameof(IDummy2.LastName) && x.FormattedName == "lastName");
//             }
//
//             [Fact]
//             public void should_return_public_properties_plus_superclass_type_public_properties_by_nested_interface()
//             {
//                 // Assert
//                 var type = typeof(IDummy3);
//
//                 // Act
//                 var props = type.GetProperties(Array.Empty<string>(), new RedisFieldCamelCaseFormatter()).ToArray();
//
//                 // Assert
//                 props.Should().HaveCount(3);
//                 props.Should().Contain(x => x.Property.Name == nameof(IDummy1.Name) && x.FormattedName == "name");
//                 props.Should().Contain(x => x.Property.Name == nameof(IDummy2.LastName) && x.FormattedName == "lastName");
//                 props.Should().Contain(x => x.Property.Name == nameof(IDummy3.Age) && x.FormattedName == "age");
//             }
//
//             [Fact]
//             public void should_return_public_properties_plus_superclass_type_public_properties_by_nested_interface_with_property_filtering()
//             {
//                 // Assert
//                 var type = typeof(IDummy3);
//                 var propertyNames = new[] { "name", "lastName" };
//
//                 // Act
//                 var props = type.GetProperties(propertyNames, new RedisFieldCamelCaseFormatter()).ToArray();
//
//                 // Assert
//                 props.Should().HaveCount(2);
//                 props.Should().Contain(x => x.Property.Name == nameof(IDummy1.Name) && x.FormattedName == "name");
//                 props.Should().Contain(x => x.Property.Name == nameof(IDummy2.LastName) && x.FormattedName == "lastName");
//             }
//
//             [Fact]
//             public void should_return_public_properties_plus_superclass_type_public_properties_by_nested_interface_with_property_filtering_from_cached()
//             {
//                 // Assert
//                 var type = typeof(IDummy3);
//                 var propertyNames = new[] { "name", "lastName" };
//
//                 // Act
//                 var props = type.GetProperties(Array.Empty<string>(), new RedisFieldCamelCaseFormatter()).ToArray();
//                 props = type.GetProperties(propertyNames, new RedisFieldCamelCaseFormatter()).ToArray();
//
//                 // Assert
//                 props.Should().HaveCount(2);
//                 props.Should().Contain(x => x.Property.Name == nameof(IDummy1.Name) && x.FormattedName == "name");
//                 props.Should().Contain(x => x.Property.Name == nameof(IDummy2.LastName) && x.FormattedName == "lastName");
//             }
//         }
//     }
// }