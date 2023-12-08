using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace R8.RedisMapper.Tests
{
    public class HashEntriesTests
    {
        [Fact]
        public void should_return_hash_entries_of_string_value()
        {
            var dict = new Dictionary<string, string>
            {
                { "name", "foo" },
                { "surname", "bar" },
            };
            var hashEntries = dict.ToHashEntries(new RedisFieldCamelCaseFormatter(), Configuration.ValueFormatters, new RedisValueWriterContext { IgnoreDefaultValue = true });

            var hashEntriesArray = hashEntries.ToArray();
            hashEntriesArray.Should().HaveCount(2);

            hashEntriesArray[0].Name.Should().Be("name");
            hashEntriesArray[0].Value.Should().Be("foo");

            hashEntriesArray[1].Name.Should().Be("surname");
            hashEntriesArray[1].Value.Should().Be("bar");
        }

        [Fact]
        public void should_return_hash_entries_of_int_value()
        {
            var dict = new Dictionary<string, int>
            {
                { "name", 1 },
                { "surname", 2 },
            };
            var hashEntries = dict.ToHashEntries(new RedisFieldCamelCaseFormatter(), Configuration.ValueFormatters, new RedisValueWriterContext { IgnoreDefaultValue = true });

            var hashEntriesArray = hashEntries.ToArray();
            hashEntriesArray.Should().HaveCount(2);

            hashEntriesArray[0].Name.Should().Be("name");
            hashEntriesArray[0].Value.Should().Be(1);

            hashEntriesArray[1].Name.Should().Be("surname");
            hashEntriesArray[1].Value.Should().Be(2);
        }

        public class DummyObj
        {
            public string Name { get; set; }
            public string Surname { get; set; }
        }

        [Fact]
        public void should_return_hash_entries_of_object()
        {
            var obj = new DummyObj
            {
                Name = "foo",
                Surname = "bar",
            };
            var hashEntries = obj.GetHashEntries(new RedisFieldCamelCaseFormatter(), Configuration.ValueFormatters, new RedisValueWriterContext());

            var hashEntriesArray = hashEntries.ToArray();
            hashEntriesArray.Should().HaveCount(2);

            hashEntriesArray[0].Name.Should().Be("name");
            hashEntriesArray[0].Value.Should().Be(obj.Name);

            hashEntriesArray[1].Name.Should().Be("surname");
            hashEntriesArray[1].Value.Should().Be(obj.Surname);
        }

        [Fact]
        public void should_return_hash_entries_of_anonymous_object()
        {
            var obj = new
            {
                Name = "foo",
                Surname = "bar",
            };
            var hashEntries = obj.GetHashEntries(new RedisFieldCamelCaseFormatter(), Configuration.ValueFormatters, new RedisValueWriterContext());

            var hashEntriesArray = hashEntries.ToArray();
            hashEntriesArray.Should().HaveCount(2);

            hashEntriesArray[0].Name.Should().Be("name");
            hashEntriesArray[0].Value.Should().Be(obj.Name);

            hashEntriesArray[1].Name.Should().Be("surname");
            hashEntriesArray[1].Value.Should().Be(obj.Surname);
        }
    }
}