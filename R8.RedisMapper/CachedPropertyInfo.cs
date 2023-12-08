using System;
using System.Collections.Generic;
using System.Reflection;

namespace R8.RedisMapper
{
    internal readonly struct CachedPropertyInfo : IEquatable<CachedPropertyInfo>
    {
        public CachedPropertyInfo(string formattedName, PropertyInfo property)
        {
            this.FormattedName = formattedName;
            this.Property = property;
            var nullableUnderlyingType = Nullable.GetUnderlyingType(property.PropertyType);
            this.PropertyType = nullableUnderlyingType ?? property.PropertyType;
            this.IsNullable = nullableUnderlyingType != null;
        }

        public string FormattedName { get; }
        public PropertyInfo Property { get; }
        public Type PropertyType { get; }
        public bool IsNullable { get; }

        public bool Equals(CachedPropertyInfo other)
        {
            return Equals(Property, other.Property);
        }

        public override bool Equals(object obj)
        {
            return obj is CachedPropertyInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Property != null ? Property.GetHashCode() : 0);
        }

        public static bool operator ==(CachedPropertyInfo left, CachedPropertyInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CachedPropertyInfo left, CachedPropertyInfo right)
        {
            return !left.Equals(right);
        }

        public static CachedPropertyInfo Empty => new CachedPropertyInfo();

        public static explicit operator CachedPropertyInfo(PropertyInfo propertyInfo)
        {
            return new CachedPropertyInfo(propertyInfo.Name, propertyInfo);
        }
    }
}