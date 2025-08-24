namespace GameOfLife.Models
{
    public class Board
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public bool[][] Grid { get; set; }
        public int Width => Grid.Length > 0 ? Grid[0].Length : 0;
        public int Height => Grid.Length;
        public int Generation { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Board()
        {
            // Needed for deserialization
        }
        public Board(int rows, int cols)
        {
            Grid = new bool[rows][];
            for (int r = 0; r < rows; r++)
            {
                Grid[r] = new bool[cols];
            }
        }

        public Board(bool[][] grid)
        {
            Grid = grid;
        }
        public class BoardState
        {
            public Board? Board { get; set; }
            public bool IsStable { get; set; }
            public bool IsCyclic { get; set; }
            public int CycleLength { get; set; }
            public string? Message { get; set; }
        }
        public Board Clone()
        {
            int rows = Height;
            int cols = Width;
            bool[][] newGrid = new bool[rows][];
            for (int r = 0; r < rows; r++)
            {
                newGrid[r] = new bool[cols];
                Array.Copy(Grid[r], newGrid[r], cols);
            }

            return new Board(newGrid)
            {
                Id = Id,
                Generation = Generation,
                CreatedAt = CreatedAt
            };
        }

        public string GetHash()
        {
            // Flatten grid to string for cycle detection
            var flat = Grid.SelectMany(row => row.Select(b => b ? '1' : '0')).ToArray();
            return new string(flat);
        }
    }
}
