using GameOfLife.Models;
using GameOfLife.RedisRepositories;
using GameOfLife.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GameOfLife.Tests.Services
{
    public class GameOfLifeServiceTests
    {
        private readonly Mock<IRedisBoardRepository> _mockRepo;
        private readonly Mock<ILogger<GameOfLifeService>> _mockLogger;
        private readonly GameOfLifeService _service;

        public GameOfLifeServiceTests()
        {
            _mockRepo = new Mock<IRedisBoardRepository>();
            _mockLogger = new Mock<ILogger<GameOfLifeService>>();
            _service = new GameOfLifeService(_mockRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateInitialBoard_ReturnsBoardWithCorrectDimensions()
        {
            var board = await _service.CreateInitialBoard(3, 4);

            Xunit.Assert.Equal(3, board.Height);
            Xunit.Assert.Equal(4, board.Width);
        }

        [Fact]
        public async Task DeleteBoard_ValidId_ReturnsTrue()
        {
            _mockRepo.Setup(r => r.DeleteBoard("id")).ReturnsAsync(true);

            var result = await _service.DeleteBoard("id");

            Xunit.Assert.True(result);
        }

        [Fact]
        public void GetNextGeneration_ReturnsBoardWithIncrementedGeneration()
        {
            var board = new Board(new bool[2][] { new bool[2], new bool[2] });
            board.Generation = 0;

            var next = _service.GetNextGeneration(board);

            Xunit.Assert.Equal(board.Generation + 1, next.Generation);
        }

        [Fact]
        public void GetNthGeneration_ReturnsBoardAfterNGenerations()
        {
            var board = new Board(new bool[2][] { new bool[2], new bool[2] });
            var result = _service.GetNthGeneration(board, 5);

            Xunit.Assert.Equal(5, result.Generation);
        }

        [Fact]
        public async Task SaveBoard_CallsRepository()
        {
            var board = new Board(new bool[2][] { new bool[2], new bool[2] });
            await _service.SaveBoard(board);

            _mockRepo.Verify(r => r.SaveBoardAsync(board), Times.Once);
        }

        [Fact]
        public async Task LoadBoard_ReturnsBoardFromRepository()
        {
            var board = new Board(new bool[2][] { new bool[2], new bool[2] });
            _mockRepo.Setup(r => r.GetBoardAsync("id")).ReturnsAsync(board);

            var result = await _service.LoadBoard("id");

            Xunit.Assert.Equal(board, result);
        }

        [Fact]
        public void GetFinalState_EmptyBoard_ReturnsStable()
        {
            var board = new Board(new bool[2][] { new bool[2], new bool[2] });
            var state = _service.GetFinalState(board, 1);

            Xunit.Assert.True(state.IsStable);
        }
    }
}

