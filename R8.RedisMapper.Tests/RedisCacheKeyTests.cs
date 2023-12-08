using System;
using System.Threading.Tasks;
using FluentAssertions;
using StackExchange.Redis;
using Xunit;

namespace R8.RedisMapper.Tests
{
    public class RedisCacheKeyTests
    {
        [Fact]
        public void should_get_parts_of_existing_key()
        {
            // Act
            var actual = RedisKeyFactory.Create($"user:{1}:usernames");

            // Assert
            actual.Get(0).Should().Be("user");
            actual.Get(1).Should().Be("1");
            actual.Get(2).Should().Be("usernames");
        }

        [Theory]
        [InlineData("test", "_A2", "test:_A2")]
        [InlineData("user", "2", "user:2")]
        [InlineData("prx", "key", "prx:key")]
        public void should_create_key(string prefix, string key, string expectedResult)
        {
            var actual = RedisKeyFactory.Create(prefix, key);

            actual.ToString().Should().Be(expectedResult);
        }

        [Fact]
        public void should_key_using_formattable_string()
        {
            // Act
            var actual = RedisKeyFactory.Create($"user:{1}");

            // Assert
            actual.ToString().Should().Be("user:1");
            actual.Get(0).Should().Be("user");
            actual.Get(1).Should().Be("1");
        }

        [Theory]
        [InlineData("avatar", "1", false)]
        [InlineData("connection", "-ljn_LJNLjbLHJbl_HBlb", false)]
        [InlineData(null, null, true)]
        [InlineData(null, "key", true)]
        [InlineData("prx", null, true)]
        [InlineData("prx", "key", false)]
        public async Task should_append_more_parts_to_existing_key(string appendingPrefix, string appendingKey, bool expectedError)
        {
            Func<Task<RedisKey>> act = async () => RedisKeyFactory.Create("user", 1.ToString()).Append(appendingPrefix, appendingKey);

            if (expectedError)
            {
                await Assert.ThrowsAsync<ArgumentNullException>(act);
            }
            else
            {
                var actual = await act();
                actual.ToString().Should().Be($"user:1:{appendingPrefix}:{appendingKey}");
                actual.Get(0).Should().Be("user");
                actual.Get(1).Should().Be("1");
                actual.Get(2).Should().Be(appendingPrefix);
                actual.Get(3).Should().Be(appendingKey);
            }
        }

        [Theory]
        [InlineData(null, null, true)]
        [InlineData(null, "key", true)]
        [InlineData("prx", null, true)]
        [InlineData("prx", "key", false)]
        public async Task should_validate_given_prefix_key(string prefix, string key, bool expectedError)
        {
            Func<Task<RedisKey>> act = async () => RedisKeyFactory.Create(prefix, key);

            if (expectedError)
            {
                await Assert.ThrowsAsync<ArgumentNullException>(act);
            }
            else
            {
                var actual = await act();
                actual.ToString().Should().Be($"{prefix}:{key}");
            }
        }

        [Fact]
        public void should_return_specific_part_of_key()
        {
            var redisKey = RedisKeyFactory.Create("user", 1.ToString());

            redisKey.Get(0).Should().Be("user");
            redisKey.Get(1).Should().Be("1");
        }
    }
}