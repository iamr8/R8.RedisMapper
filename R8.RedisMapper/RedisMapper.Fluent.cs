// using System;
// using System.Linq.Expressions;
// using System.Threading.Tasks;
// using StackExchange.Redis;
//
// namespace R8.RedisMapper
// {
//     public static class FluentApi
//     {
//         public static IRedisSet<T> Fluently<T>(this IDatabaseAsync database) where T : class, IRedisCacheObject, new()
//         {
//             return new RedisSet<T>(database);
//         }
//     }
//
//     public interface IRedisSet<T> : IRedisSetIdentifier<T> where T : class, IRedisCacheObject, new()
//     {
//         IRedisSetIdentifier<T> ById<TId>(TId id) where TId : unmanaged;
//     }
//
//     public interface IRedisSetIdentifier<T> where T : class, IRedisCacheObject, new()
//     {
//         Task<T> FindAsync();
//         Task RemoveAsync();
//         Task<int> CountAsync();
//         Task<bool> AnyAsync();
//         IRedisHashSet<T> AsHashSet();
//     }
//
//     internal class RedisSet<T> : IRedisSet<T> where T : class, IRedisCacheObject, new()
//     {
//         private readonly IDatabaseAsync _database;
//         private readonly T _instance;
//
//         public RedisSet(IDatabaseAsync database)
//         {
//             _database = database;
//             _instance = new T();
//         }
//
//         public IRedisSetIdentifier<T> ById<TId>(TId id) where TId : unmanaged
//         {
//             return new RedisSetIdentifier<T, TId>(_database, id);
//         }
//     }
//
//     internal class RedisSetIdentifier<T, TId> : IRedisSetIdentifier<T> where T : class, IRedisCacheObject, new() where TId : unmanaged
//     {
//         private readonly IDatabaseAsync _database;
//         private readonly TId _id;
//
//         public RedisSetIdentifier(IDatabaseAsync database, TId id)
//         {
//             _database = database;
//             _id = id;
//         }
//
//         public Task<T> FindAsync()
//         {
//             return Task.CompletedTask;
//         }
//
//         public Task RemoveAsync()
//         {
//             // return _database.KeyDeleteAsync($"{typeof(T).Name}:{_id}");
//             return Task.CompletedTask;
//         }
//
//         public Task<int> CountAsync()
//         {
//             return Task.FromResult(1);
//         }
//
//         public Task<bool> AnyAsync()
//         {
//             return Task.FromResult(false);
//         }
//
//         public IRedisHashSet<T> AsHashSet()
//         {
//             return new RedisHashSet<T>(_database, _id);
//         }
//     }
//
//     internal interface IRedisHashSet<T> where T : class, IRedisCacheObject, new()
//     {
//         Task<T> FindAsync();
//         Task<T> GetAsync();
//         Task RemoveAsync();
//         Task RemoveAsync(Expression<Func<T, object>> predicate);
//         IRedisHashSetSelector<TModel> Select<TModel>(Func<T, TModel> predicate);
//     }
//
//     internal interface IRedisHashSetSelector<T>
//     {
//         Task<T> FindAsync();
//         Task<T> AddAsync();
//     }
//
//     internal class RedisHashSet<T> : IRedisHashSet<T> where T : class, IRedisCacheObject, new()
//     {
//         private readonly IDatabaseAsync _database;
//         private readonly string _key;
//
//         public RedisHashSet(IDatabaseAsync database, string key)
//         {
//             _database = database;
//             _key = key;
//         }
//     }
// }