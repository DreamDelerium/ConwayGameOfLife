namespace GameOfLife.Models
{ 
    public class Cell
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsAlive { get; set; }

        public Cell(int x, int y, bool isAlive = false)
        {
            X = x;
            Y = y;
            IsAlive = isAlive;
        }
    }
}
