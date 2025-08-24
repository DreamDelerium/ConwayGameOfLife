using GameOfLife.Models;
using GameOfLife.RedisRepositories;
using static GameOfLife.Models.Board;

namespace GameOfLife.Services
{
    public class GameOfLifeService : IGameOfLifeService
    {
        private readonly IRedisBoardRepository _repository;
        private readonly ILogger<GameOfLifeService> _logger;

        public GameOfLifeService(IRedisBoardRepository repository, ILogger<GameOfLifeService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Board> CreateInitialBoard(int rows = 10, int cols = 10)
        {
            if (rows < 1 || cols < 1 || rows > 1000 || cols > 1000)
                throw new ArgumentOutOfRangeException(nameof(rows), "Rows and columns must be between 1 and 1000");

            _logger.LogInformation("Creating new initial board ({Rows}x{Cols})", rows, cols);

            var grid = new bool[rows][];
            var rand = new Random();

            for (int y = 0; y < rows; y++)
            {
                grid[y] = new bool[cols];
                for (int x = 0; x < cols; x++)
                    grid[y][x] = rand.Next(2) == 1;
            }

            var board = new Board(grid);
            await SaveBoard(board);

            return board;
        }

        public async Task<bool> DeleteBoard(string boardId)
        {
            if (string.IsNullOrWhiteSpace(boardId))
                throw new ArgumentException("Board ID cannot be null or empty", nameof(boardId));

            return await _repository.DeleteBoard(boardId);
        }
        public List<string> GetAllBoardIds()
        {
            _logger.LogInformation("Getting all board ID's");
            var response = _repository.GetAllBoardIds();

            if(response == null || response.Count < 1)
            {
                _logger.LogInformation("No Boards exist");
            }
            return response ?? new List<string>();
        }


        public Board GetNextGeneration(Board board)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));

            _logger.LogInformation("Calculating next generation for board {BoardId}", board.Id);

            var newBoard = new Board(board.Height, board.Width)
            {
                Id = board.Id,
                CreatedAt = board.CreatedAt,
                Generation = board.Generation + 1
            };

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    int aliveNeighbors = CountAliveNeighbors(board, x, y);
                    bool currentCell = board.Grid[y][x];

                    newBoard.Grid[y][x] = currentCell
                        ? aliveNeighbors == 2 || aliveNeighbors == 3
                        : aliveNeighbors == 3;
                }
            }

            return newBoard;
        }

        public Board GetNthGeneration(Board board, int n)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            if (n < 0) throw new ArgumentOutOfRangeException(nameof(n), "Number of generations must be non-negative");

            _logger.LogInformation("Calculating {N} generations ahead for board {BoardId}", n, board.Id);

            var currentBoard = board.Clone();
            for (int i = 0; i < n; i++)
                currentBoard = GetNextGeneration(currentBoard);

            return currentBoard;
        }

        public BoardState GetFinalState(Board board, int maxIterations = 10000)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            if (maxIterations <= 0) throw new ArgumentOutOfRangeException(nameof(maxIterations));

            _logger.LogInformation("Finding final state for board {BoardId}", board.Id);

            var seenStates = new Dictionary<string, int>();
            var currentBoard = board.Clone();

            for (int i = 0; i < maxIterations; i++)
            {
                string boardHash = currentBoard.GetHash();

                if (seenStates.ContainsKey(boardHash))
                {
                    int cycleStart = seenStates[boardHash];
                    int cycleLength = i - cycleStart;

                    _logger.LogInformation(
                        "Board {BoardId} found cycle of length {CycleLength} at generation {Gen}",
                        board.Id, cycleLength, i
                    );

                    return new BoardState
                    {
                        Board = currentBoard,
                        IsStable = cycleLength == 1,
                        IsCyclic = cycleLength > 1,
                        CycleLength = cycleLength,
                        Message = cycleLength == 1
                            ? "Board reached stable state"
                            : $"Board has a cycle of length {cycleLength}"
                    };
                }

                seenStates[boardHash] = i;

                var nextBoard = GetNextGeneration(currentBoard);

                if (IsEmpty(nextBoard))
                {
                    _logger.LogInformation("Board {BoardId} reached empty state at generation {Gen}", board.Id, i + 1);
                    return new BoardState
                    {
                        Board = nextBoard,
                        IsStable = true,
                        IsCyclic = false,
                        CycleLength = 0,
                        Message = "Board reached empty state"
                    };
                }

                currentBoard = nextBoard;
            }

            _logger.LogWarning(
                "Board {BoardId} did not stabilize within {MaxIterations} iterations",
                board.Id, maxIterations
            );

            return new BoardState
            {
                Board = currentBoard,
                IsStable = false,
                IsCyclic = false,
                CycleLength = 0,
                Message = $"Board did not stabilize within {maxIterations} iterations"
            };
        }

        public async Task<string> SaveBoard(Board board)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));

            _logger.LogInformation("Saving board {BoardId} to repository", board.Id);
            await _repository.SaveBoardAsync(board);
            return board.Id;
        }

        public async Task<Board> LoadBoard(string boardId)
        {
            if (string.IsNullOrWhiteSpace(boardId))
                throw new ArgumentException("Board ID cannot be null or empty", nameof(boardId));

            _logger.LogInformation("Loading board {BoardId} from repository", boardId);

            var board = await _repository.GetBoardAsync(boardId);

            if (board == null)
                throw new KeyNotFoundException($"Board with ID {boardId} not found");

            return board;
        }

        private int CountAliveNeighbors(Board board, int x, int y)
        {
            int count = 0;
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int newX = x + dx;
                    int newY = y + dy;

                    if (newY >= 0 && newY < board.Height &&
                        newX >= 0 && newX < board.Width &&
                        board.Grid[newY][newX])
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        private bool IsEmpty(Board board)
        {
            for (int y = 0; y < board.Height; y++)
                for (int x = 0; x < board.Width; x++)
                    if (board.Grid[y][x])
                        return false;
            return true;
        }
    }
}
