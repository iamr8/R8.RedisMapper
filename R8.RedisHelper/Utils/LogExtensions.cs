using System.Text;
using Microsoft.Extensions.Logging;
using R8.RedisHelper.Interfaces;

namespace R8.RedisHelper.Utils
{
    public static class LogExtensions
    {
        public static void WriteToLog(this IRedisOperation operation, ILogger logger)
        {
            if (!logger.IsEnabled(LogLevel.Debug))
                return;

            logger.LogDebug(operation.GetCommandText());
        }

        public static void WriteToLog(this IEnumerable<IRedisOperation> operations, ILogger logger)
        {
            if (!logger.IsEnabled(LogLevel.Debug))
                return;

            if (operations.Count() == 1)
            {
                logger.LogDebug(operations.First().GetCommandText());
            }
            else
            {
                foreach (var cmdGroup in operations.GroupBy(x => x.Command).ToArray())
                {
                    var cmd = cmdGroup.Key;

                    foreach (var keyGroup in cmdGroup.GroupBy(x => x.CacheKey.GetParts().Keys.First()).ToArray())
                    {
                        var cacheKeys = keyGroup.Select(x => x.CacheKey).Distinct().ToArray();
                        if (cacheKeys.Length == 1)
                        {
                            logger.LogDebug(keyGroup.First().GetCommandText());
                        }
                        else
                        {
                            var sb = new StringBuilder(cmd.Length + 1);
        
                            sb.Append(cmd);
                            sb.Append(" (");
                            sb.Append(string.Join(" ", cacheKeys));
                            sb.Append(')');

                            var ops = keyGroup.ToArray();
                            if (ops.Any())
                            {
                                var fields = new List<string>();
                                foreach (var op in ops)
                                {
                                    foreach (var opField in op.Fields)
                                    {
                                        if (fields.Contains(opField))
                                            continue;
                                        fields.Add(opField);

                                        sb.Append(' ');
                                        sb.Append(opField);
                                    }
                                }
                                fields.Clear();
                            }
                        
                            logger.LogDebug(sb.ToString());
                        }
                    }
                }
            }
        }
    }
}