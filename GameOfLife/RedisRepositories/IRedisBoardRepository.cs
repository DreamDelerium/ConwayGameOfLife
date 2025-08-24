using GameOfLife.Models;

namespace GameOfLife.RedisRepositories
{
    public interface IRedisBoardRepository
    {
        Task<Board> GetBoardAsync(string boardId);
        Task SaveBoardAsync(Board board);
        Task<bool> ExistsAsync(string boardId);
        Task<bool> DeleteBoard(string boardId);
        List<string> GetAllBoardIds();
    }
}
