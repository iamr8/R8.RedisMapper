using R8.RedisHelper.Interfaces;

namespace R8.RedisHelper.Models
{
    internal abstract class RedisReaderTaskBase : RedisOperation, IRedisReader
    {
        private volatile bool _isCompleted;
        private Memory<RedisValue> _memory = null;

        public Func<Task<RedisValue[]>> ActionWithPluralReturnType { private get; init; }
        public Func<Task<RedisValue>> Action { private get; init; }

        public async Task ExecuteAsync()
        {
            if (this.ActionWithPluralReturnType != null)
            {
                var results = await this.ActionWithPluralReturnType();
                _memory = new Memory<RedisValue>(results);
            }
            else
            {
                var result = await this.Action();
                _memory = new Memory<RedisValue>(new[] {result});
            }

            _isCompleted = true;
        }

        public RedisValue[] GetResult()
        {
            if (!_isCompleted)
                throw new InvalidOperationException("Task is not completed yet.");

            if (_memory.IsEmpty)
                return default;

            var res = _memory.Span.ToArray();
            return res;
        }
    }
}