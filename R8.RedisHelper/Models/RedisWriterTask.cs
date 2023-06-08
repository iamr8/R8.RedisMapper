using R8.RedisHelper.Interfaces;

namespace R8.RedisHelper.Models
{
    internal class RedisWriterTask<T> : RedisOperation, IRedisWriter where T : unmanaged
    {
        private volatile bool _isCompleted;
        private Memory<T> _memory = null;

        public Func<Task<T>> ActionWithReturnType { private get; set; }
        public Func<Task> Action { private get; set; }

        public async Task ExecuteAsync()
        {
            if (this.ActionWithReturnType != null)
            {
                var result = await this.ActionWithReturnType();
                _memory = new Memory<T>(new[] {result});
            }
            else
            {
                await this.Action();
                _memory = new Memory<T>(Array.Empty<T>());
            }

            _isCompleted = true;
        }

        public TResult GetResult<TResult>() where TResult : unmanaged
        {
            if (!_isCompleted)
                throw new InvalidOperationException("Task is not completed yet.");

            if (_memory.IsEmpty)
                return default;

            var res = _memory.Span[0];
            return (TResult) (object) res;
        }
    }
}