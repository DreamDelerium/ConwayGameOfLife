using GameOfLife.Models;
using Microsoft.Extensions.Options;

namespace GameOfLife.Validators
{
    public class BoardValidator : IBoardValidator
    {
        public int MaxBoardSize = 100;
        public int MinBoardSize = 3;
        private int MaxIterations = 100;
        private readonly GameSettings _settings;

        public BoardValidator(IOptions<GameSettings> settings)
        {
            _settings = settings.Value;
            MaxBoardSize = _settings.BoardMax;
            MaxBoardSize = _settings.BoardMin;
            MaxIterations = _settings.IterationMax;

        }
        public (bool isValid, string errorMessage) ValidateBoard(bool[][] grid)
        {
            if (grid == null)
                return (false, "Board grid cannot be null");

            if (grid.Length == 0 || grid.Any(r => r == null || r.Length == 0))
                return (false, "Board must not contain empty rows");

            int height = grid.Length;
            int width = grid[0].Length;

            if (grid.Any(row => row.Length != width))
                return (false, "All rows must have the same number of columns");

            if (height < MinBoardSize || width < MinBoardSize)
                return (false, $"Board dimensions must be at least {MinBoardSize}x{MinBoardSize}");

            if (height > MaxBoardSize || width > MaxBoardSize)
                return (false, $"Board dimensions cannot exceed {MaxBoardSize}x{MaxBoardSize}");

            return (true, string.Empty);
        }

        public (bool isValid, string errorMessage) ValidateIterations(int iterations)
        {
            if (iterations < 1)
                return (false, "Number of iterations must be at least 1");

            if (iterations > MaxIterations)
                return (false, $"Number of iterations cannot exceed {MaxIterations}");

            return (true, string.Empty);
        }
    }
}
