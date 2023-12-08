using System;
using System.Collections.Generic;

namespace R8.RedisMapper.Tests
{
    public class Types
    {
        public int INT { get; set; }
        public int? INT_NULLABLE { get; set; }
        public long LONG { get; set; }
        public long? LONG_NULLABLE { get; set; }
        public double DOUBLE { get; set; }
        public double? DOUBLE_NULLABLE { get; set; }
        public bool BOOLEAN { get; set; }
        public bool? BOOLEAN_NULLABLE { get; set; }
        public DateTime DATETIME { get; set; }
        public DateTime? DATETIME_NULLABLE { get; set; }
        public string STRING { get; set; }
        public string? STRING_NULLABLE { get; set; }
        public Dictionary<int, bool> DICTIONARY { get; set; }
        public Dictionary<int, bool>? DICTIONARY_NULLABLE { get; set; }
        public List<bool> LIST { get; set; }
        public List<bool>? LIST_NULLABLE { get; set; }
        public int[] ARRAY { get; set; }
        public int[]? ARRAY_NULLABLE { get; set; }
        public FakeEnum ENUM { get; set; }
        public FakeEnum? ENUM_NULLABLE { get; set; }
        public FakeObject OBJECT { get; set; }
        public FakeObject? OBJECT_NULLABLE { get; set; }
    }

    public class FakeObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class FakeRedisTestModel
    {
        public string field1 { get; set; }
        public string field2 { get; set; }
        public string field3 { get; set; }
        public string field4 { get; set; }
        public string field5 { get; set; }

        public string Calculate()
        {
            return "Calculated data";
        }

        private string PrivateItem { get; set; }
    }

    public enum FakeEnum
    {
        E1 = 0,
        E2 = 1,
    }
}