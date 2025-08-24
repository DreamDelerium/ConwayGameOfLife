using GameOfLife.Controllers;
using GameOfLife.Models;
using GameOfLife.Models.DTO;
using GameOfLife.Services;
using GameOfLife.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace GameOfLife.Tests.Controllers
{
    public class GameOfLifeControllerTests
    {
        private readonly Mock<IGameOfLifeService> _mockService;
        private readonly Mock<ILogger<GameOfLifeController>> _mockLogger;
        private readonly Mock<IBoardValidator> _mockValidator;
        private readonly GameOfLifeController _controller;
        private readonly Mock<IOptions<GameSettings>> _mockSettings;
        public GameOfLifeControllerTests()
        {
            _mockService = new Mock<IGameOfLifeService>();
            _mockLogger = new Mock<ILogger<GameOfLifeController>>();
            _mockValidator = new Mock<IBoardValidator>();
            _mockSettings = new Mock<IOptions<GameSettings>>();

            // Inject the mocked validator into the controller
            _controller = new GameOfLifeController(
                _mockService.Object,
                _mockLogger.Object,
                _mockValidator.Object,
                _mockSettings.Object
            );
        }

        [Fact]
        public void GetAllBoardIds_ReturnsBoardIds()
        {
            var boardIds = new List<string> { "id1", "id2" };
            _mockService.Setup(s => s.GetAllBoardIds()).Returns(boardIds);

            var result = _controller.GetAllBoardIds();

            var okResult = Xunit.Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Xunit.Assert.IsType<ApiResponse<List<string>>>(okResult.Value);
            Xunit.Assert.Equal(boardIds.Count, apiResponse.Data.Count);
        }

        [Fact]
        public async Task CreateInitialBoard_ReturnsCreatedBoard()
        {
            var board = new Board(new bool[2][] { new bool[2], new bool[2] });
            _mockService.Setup(s => s.CreateInitialBoard(2, 2)).ReturnsAsync(board);

            var result = await _controller.CreateInitialBoard(2, 2);

            var okResult = Xunit.Assert.IsType<OkObjectResult>(result.Result);
            var response = Xunit.Assert.IsType<ApiResponse<BoardIdResponseDto>>(okResult.Value);
            Xunit.Assert.Equal(board.Id, response.Data.BoardId);
        }

        [Fact]
        public async Task DeleteBoard_ExistingBoard_ReturnsTrue()
        {
            string boardId = "test";
            _mockService.Setup(s => s.DeleteBoard(boardId)).ReturnsAsync(true);

            var result = await _controller.DeleteBoard(boardId);

            var okResult = Xunit.Assert.IsType<OkObjectResult>(result.Result);
            var response = Xunit.Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Xunit.Assert.True(response.Data);
        }

        [Fact]
        public async Task DeleteBoard_NonExistingBoard_ThrowsKeyNotFoundException()
        {
            string boardId = "notfound";
            _mockService.Setup(s => s.DeleteBoard(boardId)).ReturnsAsync(false);

            await Xunit.Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.DeleteBoard(boardId));
        }

        [Fact]
        public async Task UploadBoard_ValidBoard_ReturnsBoardId()
        {
            var request = new UploadBoardRequestDto
            {
                Grid = new bool[2][]
                {
                    new bool[2] { true, false },
                    new bool[2] { false, true }
                }
            };

            // Mock validator to always pass
            _mockValidator.Setup(v => v.ValidateBoard(It.IsAny<bool[][]>()))
                          .Returns((true, string.Empty));

            // Mock the service to return a board ID
            _mockService.Setup(s => s.SaveBoard(It.IsAny<Board>())).ReturnsAsync("board123");

            // Act
            var result = await _controller.UploadBoard(request);

            // Assert
            var okResult = Xunit.Assert.IsType<OkObjectResult>(result.Result);
            var response = Xunit.Assert.IsType<ApiResponse<BoardIdResponseDto>>(okResult.Value);
            Xunit.Assert.Equal("board123", response.Data.BoardId);
        }

        [Fact]
        public async Task GetNextState_ReturnsNextBoardState()
        {
            var board = new Board(new bool[2][] { new bool[2], new bool[2] });
            var nextBoard = new Board(new bool[2][] { new bool[2], new bool[2] });
            _mockService.Setup(s => s.LoadBoard("id")).ReturnsAsync(board);
            _mockService.Setup(s => s.GetNextGeneration(board)).Returns(nextBoard);

            var result = await _controller.GetNextState("id");

            var okResult = Xunit.Assert.IsType<OkObjectResult>(result.Result);
            var response = Xunit.Assert.IsType<ApiResponse<BoardStateResponseDto>>(okResult.Value);
            Xunit.Assert.Equal(nextBoard.Id, response.Data.BoardId);
        }

        [Fact]
        public async Task GetNStatesAhead_InvalidIterations_ReturnsBadRequest()
        {           
            _mockValidator.Setup(v => v.ValidateBoard(It.IsAny<bool[][]>()))
                          .Returns((true, string.Empty));
            _mockValidator.Setup(v => v.ValidateIterations(It.IsAny<int>()))
                          .Returns((false, "Number of iterations must be at least 1"));
            _mockService.Setup(s => s.SaveBoard(It.IsAny<Board>()))
                        .ReturnsAsync("board123");
            // Act
            var result = await _controller.GetNStatesAhead("Id", 0);
            // Assert
            var badRequest = Xunit.Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Xunit.Assert.IsType<ApiResponse<BoardStateResponseDto>>(badRequest.Value);
            Xunit.Assert.Equal("Number of iterations must be at least 1", response.Message);
        }

        [Fact]
        public async Task GetBoard_ReturnsBoardState()
        {
            var board = new Board(new bool[2][] { new bool[2], new bool[2] });
            _mockService.Setup(s => s.LoadBoard("id")).ReturnsAsync(board);

            var result = await _controller.GetBoard("id");

            var okResult = Xunit.Assert.IsType<OkObjectResult>(result.Result);
            var response = Xunit.Assert.IsType<ApiResponse<BoardStateResponseDto>>(okResult.Value);
            Xunit.Assert.Equal(board.Id, response.Data.BoardId);
        }
    }
}
