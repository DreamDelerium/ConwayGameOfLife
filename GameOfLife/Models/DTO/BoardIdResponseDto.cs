namespace GameOfLife.Models.DTO
{
    public class BoardIdResponseDto
    {
        public string BoardId { get; set; } = string.Empty;
        public bool[][]? Board { get; set; }
    }
}
