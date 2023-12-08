using System;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using R8.XunitLogger;
using StackExchange.Redis;
using Xunit;
using Xunit.Abstractions;

namespace R8.RedisMapper.Tests
{
    public class CommandsTests
    {
        public class DummyHashGetByGenericType
        {
            private readonly IConnectionMultiplexer _connectionMultiplexer;
            private readonly IDatabase _database;
            private readonly IBatch _batch;

            private readonly ILoggerFactory _loggerFactory;

            public DummyHashGetByGenericType(ITestOutputHelper outputHelper)
            {
                (_connectionMultiplexer, _database, _batch) = Utils.CreateMock();

                _loggerFactory = new LoggerFactory().AddXunit(outputHelper, o => o.MinLevel = LogLevel.Debug);
            }

            public class Dummy
            {
                public string Name { get; set; }
                public string Surname { get; set; }
            }

            [Fact]
            public async Task should_return_null_values_by_all_fields()
            {
                var key = new RedisKey("foo");
                var fields = Array.Empty<string>();

                _database.HashGetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue[]>(), Arg.Any<CommandFlags>()).Returns(Array.Empty<RedisValue>());

                var actual = await _database.HashGetAsync<Dummy>(key, null);

                await _database.Received(1).HashGetAsync(key, Arg.Is<RedisValue[]>(x => x[0] == (RedisValue)"name" && x[1] == (RedisValue)"surname"), Arg.Is<CommandFlags>(x => x == CommandFlags.None));
                actual.Should().BeNull();
            }

            [Fact]
            public async Task should_return_null_values_by_all_fields_by_interface()
            {
                var key = new RedisKey("foo");

                _database.HashGetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue[]>(), Arg.Any<CommandFlags>()).Returns(Array.Empty<RedisValue>());

                var actual = await _database.HashGetAsync<Dummy>(key, options =>
                {
                    options.AllProperties = true;
                });

                await _database.Received(1).HashGetAsync(key, Arg.Is<RedisValue[]>(x => x[0] == (RedisValue)"name" && x[1] == (RedisValue)"surname"), Arg.Is<CommandFlags>(x => x == CommandFlags.None));
                actual.Should().BeNull();
            }

            [Fact]
            public async Task should_return_values_by_all_fields()
            {
                var key = new RedisKey("foo");

                _database.HashGetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue[]>(), Arg.Any<CommandFlags>()).Returns(new RedisValue[] { "foo", "bar" });

                var actual = await _database.HashGetAsync<Dummy>(key);

                await _database.Received(1).HashGetAsync(key, Arg.Is<RedisValue[]>(x => x[0] == (RedisValue)"name" && x[1] == (RedisValue)"surname"), Arg.Is<CommandFlags>(x => x == CommandFlags.None));
                actual.Should().NotBeNull();
                actual.Name.Should().Be("foo");
                actual.Surname.Should().Be("bar");
            }

            [Fact]
            public async Task should_return_null_values_by_specific_fields()
            {
                var key = new RedisKey("foo");
                var fields = new[] { "name" };

                _database.HashGetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue[]>(), Arg.Any<CommandFlags>()).Returns(Array.Empty<RedisValue>());

                var actual = await _database.HashGetAsync<Dummy>(key, options =>
                {
                    options.AllProperties = false;
                    foreach (var field in fields)
                        options.Properties.Add(field);
                });

                await _database.Received(1).HashGetAsync(key, Arg.Is<RedisValue[]>(x => x[0] == (RedisValue)fields[0]), Arg.Is<CommandFlags>(x => x == CommandFlags.None));
                actual.Should().BeNull();
            }

            [Fact]
            public async Task should_return_values_by_specific_fields()
            {
                var key = new RedisKey("foo");
                var fields = new[] { "name" };

                _database.HashGetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue[]>(), Arg.Any<CommandFlags>()).Returns(new RedisValue[] { "foo" });

                var actual = await _database.HashGetAsync<Dummy>(key, options =>
                {
                    options.AllProperties = false;
                    foreach (var field in fields)
                        options.Properties.Add(field);
                });

                await _database.Received(1).HashGetAsync(key, Arg.Is<RedisValue[]>(x => x[0] == (RedisValue)fields[0]), Arg.Is<CommandFlags>(x => x == CommandFlags.None));
                actual.Should().NotBeNull();
                actual.Name.Should().Be("foo");
                actual.Surname.Should().BeNull();
            }

            public class Dummy2
            {
                public int Age { get; set; }
            }
        }

        public class DummyHashGetByDictionary
        {
            private readonly IDatabaseAsync _database = Substitute.For<IDatabaseAsync>();

            [Fact]
            public async Task should_throw_exception_with_no_fields_given()
            {
                var key = new RedisKey("foo");
                var fields = Array.Empty<string>();

                _database.HashGetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue[]>(), Arg.Any<CommandFlags>()).Returns(new RedisValue[] { "foo", "bar" });

                await Assert.ThrowsAsync<RedisMapperException>(() => _database.HashGetAsync(key, options =>
                {
                    options.Fields.Clear();
                }));

                await _database.DidNotReceive().HashGetAsync(key, Arg.Is<RedisValue[]>(x => x[0] == (RedisValue)"name" && x[1] == (RedisValue)"surname"), Arg.Is<CommandFlags>(x => x == CommandFlags.None));
            }

            [Fact]
            public async Task should_return_null_values_by_specific_fields()
            {
                var key = new RedisKey("foo");
                var fields = new[] { "name" };

                _database.HashGetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<CommandFlags>()).Returns(RedisValue.Null);

                var actual = await _database.HashGetAsync(key, options =>
                {
                    foreach (var field in fields)
                        options.Fields.Add(field);
                }, CommandFlags.None);

                await _database.Received(1).HashGetAsync(key, Arg.Is<RedisValue[]>(x => x[0] == (RedisValue)fields[0]), Arg.Is<CommandFlags>(x => x == CommandFlags.None));
                actual.Should().BeEmpty();
            }

            [Fact]
            public async Task should_return_values_by_specific_fields()
            {
                var key = new RedisKey("foo");
                var fields = new[] { "name", "surname" };

                _database.HashGetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue[]>(), Arg.Any<CommandFlags>()).Returns(new RedisValue[] { "foo", "bar" });

                var actual = await _database.HashGetAsync(key, options =>
                {
                    foreach (var field in fields)
                        options.Fields.Add(field);
                }, CommandFlags.None);

                await _database.Received(1).HashGetAsync(key, Arg.Is<RedisValue[]>(x => x[0] == (RedisValue)"name" && x[1] == (RedisValue)"surname"), Arg.Is<CommandFlags>(x => x == CommandFlags.None));
                actual.Should().NotBeNull();
                actual["name"].Should().Be("foo");
                actual["surname"].Should().Be("bar");
            }
        }

        public class DummySetByGenericType
        {
            private readonly IDatabaseAsync _database = Substitute.For<IDatabaseAsync>();

            [Fact]
            public async Task should_set_value_by_specific_fields()
            {
                var key = new RedisKey("foo");
                var value = false;

                _database.StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<TimeSpan?>(), Arg.Any<When>(), Arg.Any<CommandFlags>()).Returns(true);

                var actual = await _database.SetAsync(key, (RedisValue)value, null, When.Always, CommandFlags.None);
                actual.Should().BeTrue();

                await _database.Received(1).StringSetAsync(key, Arg.Is<RedisValue>(x => x == (RedisValue)value), Arg.Is<TimeSpan?>(x => x == null), Arg.Is<When>(x => x == When.Always), Arg.Is<CommandFlags>(x => x == CommandFlags.None));
            }

            [Fact]
            public async Task should_delete_value_by_specific_fields_when_value_is_null()
            {
                var key = new RedisKey("foo");
                var value = false;

                Configuration.IgnoreDefaultValues = true;

                _database.KeyDeleteAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(true);

                var actual = await _database.SetAsync(key, value, null, When.Always, CommandFlags.None);
                actual.Should().BeTrue();

                await _database.DidNotReceive().StringSetAsync(key, Arg.Is<RedisValue>(x => x == (RedisValue)value), Arg.Is<TimeSpan?>(x => x == null), Arg.Is<When>(x => x == When.Always), Arg.Is<CommandFlags>(x => x == CommandFlags.None));
                await _database.Received(1).KeyDeleteAsync(key, Arg.Is<CommandFlags>(x => x == CommandFlags.None));
            }
        }

        public class DummyHashSetByRedisValues
        {
            private readonly IDatabaseAsync _database = Substitute.For<IDatabaseAsync>();

            [Fact]
            public async Task should_set_value_by_specific_field()
            {
                var key = new RedisKey("foo");
                var field = (RedisValue)"name";
                var value = (RedisValue)"foo";

                _database.HashSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<RedisValue>(), Arg.Any<When>(), Arg.Any<CommandFlags>()).Returns(true);

                var actual = await _database.HashSetAsync(key, field, value, When.Always, CommandFlags.None);

                actual.Should().BeTrue();

                await _database.Received(1).HashSetAsync(key, Arg.Is<RedisValue>(x => x == field), Arg.Is<RedisValue>(x => x == value), Arg.Is<When>(x => x == When.Always), Arg.Is<CommandFlags>(x => x == CommandFlags.None));
                await _database.DidNotReceive().HashDeleteAsync(key, Arg.Is<RedisValue>(x => x == field), Arg.Is<CommandFlags>(x => x == CommandFlags.None));
            }
        }

        public class DummyHashSetByModel
        {
            private readonly IDatabaseAsync _database = Substitute.For<IDatabaseAsync>();

            public class Dummy
            {
                public string Name { get; set; }
                public string Surname { get; set; }
            }
            //
            // [Fact]
            // public async Task should_set_value_by_specific_field()
            // {
            //     var key = new RedisKey("foo");
            //     var model = new Dummy
            //     {
            //         Name = "Arash",
            //         Surname = "Shabbeh"
            //     };
            //
            //     _database.HashSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<RedisValue>(), Arg.Any<When>(), Arg.Any<CommandFlags>()).Returns(true);
            //
            //     await _database.HashSetAsync(key, model);
            //
            //     await _database.Received(1).HashSetAsync(key, Arg.Is<RedisValue>(x => x == field), Arg.Is<RedisValue>(x => x == value), Arg.Is<When>(x => x == When.Always), Arg.Is<CommandFlags>(x => x == CommandFlags.None));
            //     await _database.DidNotReceive().HashDeleteAsync(key, Arg.Is<RedisValue>(x => x == field), Arg.Is<CommandFlags>(x => x == CommandFlags.None));
            // }
        }
    }
}