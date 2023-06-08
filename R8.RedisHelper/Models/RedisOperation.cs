using System.Text;
using R8.RedisHelper.Interfaces;

namespace R8.RedisHelper.Models
{
    internal abstract class RedisOperation : IRedisOperation
    {
        public RedisCacheKey CacheKey { get; init; }
        public string Command { get; set; }
        public string[] Fields { get; set; }
        public RedisValue[] Values { get; set; }
        public string GetCommandText()
        {
            var sb = new StringBuilder(this.Command.Length + 1);
        
            sb.Append(this.Command);
            sb.Append(' ');
            sb.Append(this.CacheKey.Value);

            if (this.Fields?.Any() == true && this.Values?.Any() == true)
            {
                if (this.Fields.Length == this.Values.Length)
                {
                    for (int i = 0; i < this.Fields.Length; i++)
                    {
                        sb.Append(' ');
                        sb.Append(this.Fields[i]);
                        sb.Append(" \"");
                        sb.Append(this.Values[i]);
                        sb.Append('"');
                    }
                }
                else
                {
                    throw new InvalidOperationException("Fields and Values must have the same length.");
                }
            }

            if (this.Fields?.Any() == true && (this.Values == null || !this.Values.Any()))
            {
                sb.Append(' ');
                sb.Append(string.Join(" ", this.Fields));
            }
        
            if (this.Values?.Any() == true && (this.Fields == null || !this.Fields.Any()))
            {
                sb.Append(' ');
                sb.Append(string.Join(" ", this.Values));
            }
        
            return sb.ToString();
        }
    }
}