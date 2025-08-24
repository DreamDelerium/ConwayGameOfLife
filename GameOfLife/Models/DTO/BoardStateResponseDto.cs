namespace GameOfLife.Models.DTO
{
    public class BoardStateResponseDto
    {
        public string BoardId { get; set; } = string.Empty;
        public bool[][]? Grid { get; set; }
        public int Generation { get; set; }
        public bool IsStable { get; set; }
        public bool IsCyclic { get; set; }
        public int CycleLength { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
