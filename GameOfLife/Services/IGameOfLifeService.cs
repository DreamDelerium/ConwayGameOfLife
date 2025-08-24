using GameOfLife.Models;
using static GameOfLife.Models.Board;

namespace GameOfLife.Services
{
    public interface IGameOfLifeService
    {
        Board GetNextGeneration(Board board);
        Board GetNthGeneration(Board board, int n);
        BoardState GetFinalState(Board board, int maxIterations = 10000);
        Task<string> SaveBoard(Board board);
        Task<Board> LoadBoard(string boardId);
        Task<Board> CreateInitialBoard(int rows = 10, int cols = 10);
        Task<bool> DeleteBoard(string boardId);
        List<string> GetAllBoardIds();
    }
}
