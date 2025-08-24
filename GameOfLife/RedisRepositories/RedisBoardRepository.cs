using GameOfLife.Models;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace GameOfLife.RedisRepositories
{
    public class RedisBoardRepository : IRedisBoardRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly ILogger<RedisBoardRepository> _logger;

        private readonly TimeSpan _expiry = TimeSpan.FromHours(1);

        public RedisBoardRepository(IConnectionMultiplexer redis, ILogger<RedisBoardRepository> logger, IOptions<RedisSettings> redisOptions)
        {
            _redis = redis;
            _database = _redis.GetDatabase();
            _logger = logger;
            var settings = redisOptions.Value;
            _expiry = TimeSpan.FromHours(settings.ExpiryHours);
        }
        public List<string> GetAllBoardIds()
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var keys = server.Keys(pattern: "gameoflife:board:*");

                var boardIds = keys
                    .Select(k => k.ToString().Replace("gameoflife:board:", string.Empty))
                    .ToList();

                _logger.LogInformation("Retrieved {Count} board IDs from Redis", boardIds.Count);
                return boardIds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all board IDs from Redis");
                throw new ApplicationException("An error occurred while retrieving board IDs.", ex);
            }
        }
        public async Task<Board> GetBoardAsync(string boardId)
        {
            try
            {
                string key = GetKey(boardId);
                var value = await _database.StringGetAsync(key);

                if (!value.HasValue)
                {
                    _logger.LogWarning($"Board {boardId} not found in Redis");                    
                    return null;
                }

                var board = JsonSerializer.Deserialize<Board>(value.ToString());
                _logger.LogInformation($"Successfully retrieved board {boardId} from Redis");
                return board!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving board {boardId} from Redis");
                throw;
            }
        }

        public async Task SaveBoardAsync(Board board)
        {
            try
            {
                string key = GetKey(board.Id);
                var json = JsonSerializer.Serialize(board);

                await _database.StringSetAsync(key, json, _expiry);
                _logger.LogInformation($"Successfully saved board {board.Id} to Redis with {_expiry.TotalHours} hour expiry");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving board {board.Id} to Redis");
                throw;
            }
        }

        public async Task<bool> ExistsAsync(string boardId)
        {
            try
            {
                string key = GetKey(boardId);
                return await _database.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking existence of board {boardId} in Redis");
                throw;
            }
        }
        public async Task<bool> DeleteBoard(string boardId)
        {
            if (string.IsNullOrWhiteSpace(boardId))
                throw new ArgumentException("Board ID cannot be null or empty", nameof(boardId));

            try
            {
                string key = GetKey(boardId);
                bool deleted = await _database.KeyDeleteAsync(key);

                if (deleted)
                    _logger.LogInformation("Successfully deleted board {BoardId} from Redis", boardId);
                else
                    _logger.LogWarning("Board {BoardId} not found in Redis when attempting delete", boardId);

                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting board {BoardId} from Redis", boardId);
                throw new ApplicationException($"An error occurred while deleting board {boardId}.", ex);
            }
        }

        private string GetKey(string boardId) => $"gameoflife:board:{boardId}";
    }
}
