using GameOfLife.Models;
using GameOfLife.Models.DTO;
using GameOfLife.Services;
using GameOfLife.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GameOfLife.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameOfLifeController : ControllerBase
    {
        private readonly IGameOfLifeService _service;
        private readonly ILogger<GameOfLifeController> _logger;
        private readonly IBoardValidator _validator;
        private readonly IOptions<GameSettings> _settings;
        public GameOfLifeController(IGameOfLifeService service, ILogger<GameOfLifeController> logger, IBoardValidator boardValidator, IOptions<GameSettings> settings)
        {
            _service = service;
            _logger = logger;
            _validator = boardValidator;
            _settings = settings;
        }
        /// <summary>
        /// Create a new initial board
        /// </summary>
        /// <param name="rows">Number of rows (default 10)</param>
        /// <param name="cols">Number of columns (default 10)</param>
        /// <returns>Returns the newly created board ID and grid</returns>
        /// <response code="200">Successfully created the initial board</response>
        /// <response code="400">Invalid rows or columns</response>        
        [HttpPost("board/create")]
        [ProducesResponseType(typeof(ApiResponse<BoardIdResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<BoardIdResponseDto>), 400)]
        public async Task<ActionResult<ApiResponse<BoardIdResponseDto>>> CreateInitialBoard(int rows = 10, int cols = 10)
        {
            var board = await _service.CreateInitialBoard(rows, cols);

            return Ok(ApiResponse<BoardIdResponseDto>.Ok(
                new BoardIdResponseDto { BoardId = board.Id, Board = board.Grid },
                "Initial board successfully created"));
        }

        /// <summary>
        /// Upload a custom board
        /// </summary>
        /// <param name="request">Grid data for the board</param>
        /// <returns>Board ID and grid</returns>
        /// <response code="200">Board successfully uploaded</response>
        /// <response code="400">Invalid board format</response>
        [HttpPost("board")]
        [ProducesResponseType(typeof(ApiResponse<BoardIdResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<BoardIdResponseDto>), 400)]
        public async Task<ActionResult<ApiResponse<BoardIdResponseDto>>> UploadBoard([FromBody] UploadBoardRequestDto request)
        {
            var (isValid, errorMessage) = _validator.ValidateBoard(request.Grid);
            if (!isValid)
                return BadRequest(ApiResponse<BoardIdResponseDto>.Fail(errorMessage));

            var board = new Board(request.Grid);
            var boardId = await _service.SaveBoard(board);

            return Ok(ApiResponse<BoardIdResponseDto>.Ok(
                new BoardIdResponseDto { BoardId = boardId, Board = board.Grid },
                "Board successfully uploaded"));
        }

        /// <summary>
        /// Get all board IDs
        /// </summary>
        /// <remarks>
        /// Returns a list of all existing board IDs in the Redis repository.
        /// Useful for quickly listing available boards.
        /// </remarks>
        /// <returns>List of board IDs wrapped in ApiResponse</returns>
        /// <response code="200">Successfully retrieved list of board IDs</response>        
        [HttpGet("boards/ids")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), 200)]
        public ActionResult<ApiResponse<List<string>>> GetAllBoardIds()
        {
            var boardIds = _service.GetAllBoardIds();

            if (boardIds == null || boardIds.Count < 1)
                _logger.LogInformation("No boards exist");

            return Ok(ApiResponse<List<string>>.Ok(boardIds, "Retrieved all board IDs"));
        }

        /// <summary>
        /// Get the current state of a board
        /// </summary>
        /// <param name="boardId">Board ID</param>
        [HttpGet("board/{boardId}")]
        [ProducesResponseType(typeof(ApiResponse<BoardStateResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<BoardStateResponseDto>), 404)]
        public async Task<ActionResult<ApiResponse<BoardStateResponseDto>>> GetBoard(string boardId)
        {
            var board = await _service.LoadBoard(boardId);

            return Ok(ApiResponse<BoardStateResponseDto>.Ok(new BoardStateResponseDto
            {
                BoardId = board.Id,
                Grid = board.Grid,
                Generation = board.Generation,
                Timestamp = DateTime.UtcNow
            }));
        }

        /// <summary>
        /// Get the next generation of a board
        /// </summary>
        /// <remarks>
        /// Note:  If autoSave is true, the board state will be saved after computing the next generation.  If it is false, the board state will not be saved and will not progress to the next state.
        /// </remarks>
        /// <param name="boardId">Board ID</param>
        /// <param name="autoSave">Whether to save automatically</param>
        [HttpGet("board/{boardId}/next")]
        [ProducesResponseType(typeof(ApiResponse<BoardStateResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<BoardStateResponseDto>), 400)]
        [ProducesResponseType(typeof(ApiResponse<BoardStateResponseDto>), 404)]
        public async Task<ActionResult<ApiResponse<BoardStateResponseDto>>> GetNextState(string boardId, bool autoSave = false)
        {
            var board = await _service.LoadBoard(boardId);
            var nextBoard = _service.GetNextGeneration(board);

            if (autoSave)
                await _service.SaveBoard(nextBoard);

            return Ok(ApiResponse<BoardStateResponseDto>.Ok(new BoardStateResponseDto
            {
                BoardId = nextBoard.Id,
                Grid = nextBoard.Grid,
                Generation = nextBoard.Generation,
                Timestamp = DateTime.UtcNow
            }));
        }

        /// <summary>
        /// Advance a board N generations
        /// </summary>
        /// <param name="boardId">Board ID</param>
        /// <param name="iterations">Number of generations to advance</param>
        /// <param name="autoSave">Whether to save automatically</param>
        /// <remarks>
        /// Note:  If autoSave is true, the board state will be saved after computing the next generation.  If it is false, the board state will not be saved and will not progress to the next state.
        /// </remarks>
        [HttpGet("board/{boardId}/advance/{iterations}")]
        [ProducesResponseType(typeof(ApiResponse<BoardStateResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<BoardStateResponseDto>), 400)]
        [ProducesResponseType(typeof(ApiResponse<BoardStateResponseDto>), 404)]
        public async Task<ActionResult<ApiResponse<BoardStateResponseDto>>> GetNStatesAhead(string boardId, int iterations, bool autoSave = false)
        {
            var (isValid, errorMessage) = _validator.ValidateIterations(iterations);
            if (!isValid)
                return BadRequest(ApiResponse<BoardStateResponseDto>.Fail(errorMessage));

            var board = await _service.LoadBoard(boardId);
            var futureBoard = _service.GetNthGeneration(board, iterations);

            if (autoSave)
                await _service.SaveBoard(futureBoard);

            return Ok(ApiResponse<BoardStateResponseDto>.Ok(new BoardStateResponseDto
            {
                BoardId = futureBoard.Id,
                Grid = futureBoard.Grid,
                Generation = futureBoard.Generation,
                Timestamp = DateTime.UtcNow,
                Message = $"Advanced {iterations} generations"
            }));
        }

        /// <summary>
        /// Get the final stable or cyclic state of a board
        /// </summary>
        /// <param name="boardId">Board ID</param>
        /// <param name="maxIterations">Maximum generations to simulate (1-100,000)</param>
        /// <param name="autoSave">Whether to save automatically</param>
        /// <remarks>
        /// Note:  If autoSave is true, the board state will be saved after computing the next generation.  If it is false, the board state will not be saved and will not progress to the next state.
        /// </remarks>
        [HttpGet("board/{boardId}/final")]
        [ProducesResponseType(typeof(ApiResponse<BoardStateResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<BoardStateResponseDto>), 400)]
        [ProducesResponseType(typeof(ApiResponse<BoardStateResponseDto>), 404)]
        public async Task<ActionResult<ApiResponse<BoardStateResponseDto>>> GetFinalState(string boardId, int maxIterations = 10000, bool autoSave = false)
        {
            if (maxIterations < 1 || maxIterations > _settings.Value.IterationMax)
                return BadRequest(ApiResponse<BoardStateResponseDto>.Fail($"Max iterations must be between 1 and {_settings.Value.IterationMax}"));

            var board = await _service.LoadBoard(boardId);
            var finalState = _service.GetFinalState(board, maxIterations);

            if (finalState.Board != null && autoSave)
                await _service.SaveBoard(finalState.Board);

            return Ok(ApiResponse<BoardStateResponseDto>.Ok(new BoardStateResponseDto
            {
                BoardId = finalState.Board?.Id ?? boardId,
                Grid = finalState.Board?.Grid,
                Generation = finalState.Board?.Generation ?? 0,
                IsStable = finalState.IsStable,
                IsCyclic = finalState.IsCyclic,
                CycleLength = finalState.CycleLength,
                Message = finalState.Message ?? string.Empty,
                Timestamp = DateTime.UtcNow
            }));
        }

        /// <summary>
        /// Delete a board by ID
        /// </summary>
        /// <param name="boardId">ID of the board to delete</param>
        /// <returns>Boolean indicating success</returns>
        /// <response code="200">Board successfully deleted</response>
        /// <response code="404">Board not found</response>
        [HttpDelete("board/{boardId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteBoard(string boardId)
        {
            if (string.IsNullOrWhiteSpace(boardId))
                throw new ArgumentException("Board ID cannot be null or empty", nameof(boardId));

            var deleted = await _service.DeleteBoard(boardId);

            if (!deleted)
                throw new KeyNotFoundException($"Board with ID {boardId} not found");

            return Ok(ApiResponse<bool>.Ok(true, $"Board {boardId} deleted successfully"));
        }
    }
}
