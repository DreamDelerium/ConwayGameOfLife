namespace GameOfLife.Models
{
    public class RedisSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public int ExpiryHours { get; set; }
    }
}