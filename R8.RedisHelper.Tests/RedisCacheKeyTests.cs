using R8.RedisHelper.Utils;
using StackExchange.Redis;

namespace R8.RedisHelper.Tests;

public class RedisCacheKeyTests
{
    [Fact]
    public void Should_Create()
    {
        // Act
        var key = new RedisKey("user:1");
        var parts = key.GetParts();

        // Assert
        Assert.Equal(1, parts.Count);
        Assert.Contains(parts, x => x.Key == "user");
        Assert.Equal("1", parts["user"]);
    }

    [Fact]
    public void Should_Append2()
    {
        // Act
        var key = new RedisKey("user:1:connection:-ljn_LJNLjbLHJbl_HBlb");
        var parts = key.GetParts();

        // Assert
        Assert.Equal("1", parts["user"]);
        Assert.Equal("-ljn_LJNLjbLHJbl_HBlb", parts["connection"]);
    }
}