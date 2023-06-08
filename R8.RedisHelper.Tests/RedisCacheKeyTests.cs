using FluentAssertions;
using NSubstitute.ExceptionExtensions;
using R8.RedisHelper.Models;

namespace R8.RedisHelper.Tests;

public class RedisCacheKeyTests
{
    [Fact]
    public void Should_Create()
    {
        // Act
        var f = RedisCacheKey.Create("user", 1.ToString());

        // Assert
        Assert.Equal("user:1", f.ToString());
        Assert.Equal(1, f.AsInt());
        Assert.Equal("1", f.AsString());
        Assert.Equal("1", f.All["user"]);
    }

    [Fact]
    public void Should_Append()
    {
        // Act
        var f = RedisCacheKey
            .Create("user", 1.ToString())
            .Append("avatar", 1.ToString());

        // Assert
        Assert.Equal("user:1:avatar:1", f.ToString());
        Assert.Equal("1", f.All["user"]);
        Assert.Equal("1", f.All["avatar"]);
    }

    [Fact]
    public void Should_Append2()
    {
        // Act
        var f = RedisCacheKey
            .Create("user", 1.ToString())
            .Append("connection", "-ljn_LJNLjbLHJbl_HBlb");

        // Assert
        Assert.Equal("user:1:connection:-ljn_LJNLjbLHJbl_HBlb", f.ToString());
        Assert.Equal("1", f.All["user"]);
        Assert.Equal("-ljn_LJNLjbLHJbl_HBlb", f.All["connection"]);
    }

    [Theory]
    [InlineData(null, null, true)]
    [InlineData(null, "key", true)]
    [InlineData("prx", null, true)]
    [InlineData("prx", "key", false)]
    public void CacheKey_should_have_prefix_and_valid_key(string prefix, string key, bool expectedError)
    {
        var act = () => RedisCacheKey.Create(prefix, key);
        if (expectedError)
        {
            act.Should().Throw<ArgumentNullException>();
        }
        else
        {
            act.Should().NotThrow<ArgumentNullException>();
        }
    }


    [Theory]
    [InlineData("test", "_A2", "test:_A2")]
    [InlineData("user", "2", "user:2")]
    [InlineData("prx", "key", "prx:key")]
    public void C1acheKey_should_have_prefix_and_valid_key(string prefix, string key, string expectedResult)
    {
        var result = RedisCacheKey.Create(prefix, key);


        result.ToString().Should().Be(expectedResult);
    }

    [Fact]
    public void CacheKey_can_not_be_null()
    {
        var act = () => RedisCacheKey.Create(null);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_CacheKey_return_cacheKey()
    {
        var result = RedisCacheKey.Create("test");


        result.ToString().Should().Be("test");
    }

    [Theory]
    [InlineData(null, null, true)]
    [InlineData(null, "key", true)]
    [InlineData("prx", null, true)]
    [InlineData("prx", "key", false)]
    public void Passing_null_arguments_to_append_causes_ArgumentNullException(string prefix, string key, bool expectedError)
    {
        var act = () => RedisCacheKey.Create("prefix", "key").Append(prefix, key);
        if (expectedError)
        {
            act.Should().Throw<ArgumentNullException>();
        }
        else
        {
            act.Should().NotThrow<ArgumentNullException>();
        }
    }

    [Fact]
    public void Append_concat_2_cacheKeys()
    {
        var cacheKey = RedisCacheKey.Create("prefix", "key");
        var expectedValue = "prefix:key:prefix2:key2";


        var result = cacheKey.Append("prefix2", "key2");


        result.ToString().Should().Be(expectedValue);
    }

    [Fact]
    public void AsInt_return_second_part_of_cacheKey_as_numeric()
    {
        var expectedValue = 3;
        var cacheKey = RedisCacheKey.Create("user", expectedValue.ToString());


        var result = cacheKey.AsInt();


        result.Should().Be(expectedValue);
    }

    [Fact]
    public void AsInt_can_not_parse_a_key_with_more_than_one_part()
    {
        var cacheKey = RedisCacheKey.Create("user", "3").Append("role", "4");

        var act = () => cacheKey.AsInt();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AsString_can_not_parse_a_key_with_more_than_one_part()
    {
        var cacheKey = RedisCacheKey.Create("user", "3").Append("role", "4");

        var act = () => cacheKey.AsString();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AsString_return_second_part_of_the_cacheKey()
    {
        var expectedValue = "3";
        var cacheKey = RedisCacheKey.Create("user", expectedValue);


        var result = cacheKey.AsString();


        result.Should().Be(expectedValue);
    }
}