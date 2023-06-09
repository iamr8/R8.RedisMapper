namespace R8.RedisHelper.Utils
{
    public static class RedisKetExtensions
    {
        public static IReadOnlyDictionary<string, string> GetParts(this RedisKey redisKey)
        {
            var dict = new Dictionary<string, string>();
            var span = redisKey.ToString().AsSpan();
            while (true)
            {
                var delimiterIndex = span.IndexOf(':');
                if (delimiterIndex == -1)
                {
                    break;
                }
            
                var key = span[..delimiterIndex].ToString();
                span = span[(delimiterIndex + 1)..];
                delimiterIndex = span.IndexOf(':');
            
                string value;
                if (delimiterIndex == -1)
                {
                    value = span.ToString();
                    span = span[span.Length..];
                }
                else
                {
                    value = span[..delimiterIndex].ToString();
                    span = span[(delimiterIndex + 1)..];
                }
            
                dict[key] = value;
            }
        
            return dict;
        }

    }
}