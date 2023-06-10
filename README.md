# RedisHelper
An `StackExchange.Redis` assistant class for .NET Core.

### Installation
#### Step 1
```csharp
Install-Package StackExchange.Redis
```

#### Step 2
```csharp
// ... other services

services.AddRedisHelper(options =>
{
    options.DatabaseId = 0; // Your Redis database id
    options.Configurations = new ConfigurationOptions() // Your Redis configurations
    options.Configurations.EndPoints.Add("localhost", 6379); // Your Redis server
})
```

#### Step 3
Inject `ICacheProvider` to your class constructor and use it.

---
### Supported Commands
- [GetAsync](#getasync)
- [SetAsync](#setasync)
- [ExistsAsync](#existsasync)
- [DeleteAsync](#deleteasync)
- [IncrementAsync](#incrementasync)
- [ExpireAsync](#expireasync)
- [Scan](#scan)
- [FlushAsync](#flushasync)
- [BatchAsync](#batchasync)
- `PublishAsync` [Experimental]
- `SubscribeAsync` [Experimental]

---
### Usage
#### GetAsync
**1)** When you want to get a cached data, and settle it to a certain model:
```csharp
public async Task<FooModel> FooAsync()
{
    var cacheKey = new RedisKey("foo:bar");
    var cache = await _cacheProvider.GetAsync<FooModel>(cacheKey);
    if (cache.IsNull)
        return null;
        
    return cache.Value;
}
```
**1.1)** But if need a certain field:
```csharp
public async Task<string> FooAsync()
{
    var cacheKey = new RedisKey("foo:bar");
    var cache = await _cacheProvider.GetAsync<FooModel>(cacheKey, "name");
    if (cache.IsNull)
        return null;
        
    return cache.Value.Name;
}
```
_`fields` param is a `params string[]` type, so you can get multiple fields at once (case-insensitive)_

**3)** When you want to get a cached data, without knowing its type:
```csharp
public async Task<Dictionary<string, RedisCacheValue>> FooAsync()
{
    var cacheKey = new RedisKey("foo:bar");
    var cache = await _cacheProvider.GetAsync(cacheKey);
    if (cache.IsNull)
        return null;
        
    return cache.Value;
}
```

---
#### SetAsync
**1)** When you want to set cache, from a certain model:
```csharp
public class FooModel
{
    public string Name { get; set; }
}

public async Task FooAsync()
{
    var cacheKey = new RedisKey("foo:bar");
    var model = new FooModel { Name = "Arash" };
    await _cacheProvider.SetAsync<FooModel>(cacheKey, model);
}
```
**2)** When you want to set cache, without knowing its type:
```csharp
public async Task FooAsync()
{
    var cacheKey = new RedisKey("foo:bar");
    var cached = await _cacheProvider.SetAsync(cacheKey, new { name = "Arash" });
    if (cached.IsNull || !cached.Value)
        throw new Exception("Something went wrong!");
        
    // Operation is done successfully
}
```
**3)** When you want to set cache for a certain field:
```csharp
public async Task FooAsync()
{
    var cacheKey = new RedisKey("foo:bar");
    var cached = await _cacheProvider.SetAsync(cacheKey, "name", "Arash");
    if (cached.IsNull || !cached.Value)
        throw new Exception("Something went wrong!");
        
    // Operation is done successfully
}
```

---
#### ExistsAsync
When you want to check if a cache key exists:
```csharp
public async Task<bool> FooAsync()
{
    var cacheKey = new RedisKey("foo:bar");
    var exists = await _cacheProvider.ExistsAsync(cacheKey);
    return exists;
}
```

---
#### DeleteAsync
**1)** When you want to delete a cached key:
```csharp
public async Task<bool> FooAsync()
{
    var cacheKey = new RedisKey("foo:bar");
    var deleted = await _cacheProvider.DeleteAsync(cacheKey);
    if (deleted.IsNull)
        throw new Exception("Something went wrong!");
        
    return deleted.Value;
}
```
**2)** When you want to delete a cached hash key:
```csharp
public async Task<bool> FooAsync()
{
    var cacheKey = new RedisKey("foo:bar");
    var deleted = await _cacheProvider.DeleteAsync(cacheKey, "name");
    if (deleted.IsNull)
        throw new Exception("Something went wrong!");
        
    return deleted.Value;
}
```

---
#### IncrementAsync
**1)** When you want to increment a cached key:
```csharp
public async Task<long> FooAsync()
{
    var cacheKey = new RedisKey("foo");
    var updatedValue = await _cacheProvider.IncrementAsync(cacheKey, 1);
    if (updatedValue.IsNull)
        throw new Exception("Something went wrong!");
        
    return updatedValue.Value;
}
```
**2)** When you want to increment a cached hash key:
```csharp
public async Task<long> FooAsync()
{
    var cacheKey = new RedisKey("foo:bar");
    var updatedValue = await _cacheProvider.IncrementAsync(cacheKey, "retry", 1);
    if (updatedValue.IsNull)
        throw new Exception("Something went wrong!");
        
    return updatedValue.Value;
}
```

----
#### ExpireAsync
When you want to set expire time for a cached key:
```csharp
public async Task FooAsync()
{
    var cacheKey = new RedisKey("foo:bar");
    var set = await _cacheProvider.ExpireAsync(cacheKey, TimeSpan.FromSeconds(10));
    if (set.IsNull || !set.Value)
        throw new Exception("Something went wrong!");
        
    // Operation is done successfully
}
```
---
#### Scan
When you want to scan a pattern: _[Not Recommended]_
```csharp
public RedisCacheKey[] FooAsync()
{
    var paginatedCachedData = _cacheProvider.Scan("foo:*", 100);
    return paginatedCachedData;
}
```

---
#### FlushAsync
_* Be careful while using this method. It is only recommended for development *_

When you want to flush all cached data:
```csharp
public async Task FooAsync()
{
    await _cacheProvider.FlushAsync();
}
```
---

#### BatchAsync
**1)** When you want to execute multiple commands at once:
```csharp
public async Task FooAsync()
{
    var cacheKey = new RedisKey("foo:bar");
    await _cacheProvider.BatchAsync(b => 
    {
        b.Set(cacheKey, "name", "Arash");
        b.Set(cacheKey, "age", 33);
    });
}
```
**2)** When you want to read multiple cached data at once:
```csharp
public class FooModel
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public async Task<FooModel[]> FooAsync()
{
    var readCached = await _cacheProvider.BatchAsync<FooModel>(b => 
    {
        b.Get(new RedisKey("foo:bar"));
        b.Get(new RedisKey("foo:bar2"));
    });
    if (!readCached.Any() || readCached.All(x => x.IsNull))
        return Array.Empty<FooModel>();
        
    return readCached.Select(x => x.Value).ToArray();
}
```

---
### Must Read
The last param of most of the methods is `fireAndForget`, so you can set it to `true` if you don't want to wait for the result.
_the default value is `true`._

---
**Thanks to [Ali Meshkini](https://github.com/meshkini) for writing unit tests:v:**
