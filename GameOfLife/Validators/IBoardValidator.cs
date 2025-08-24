namespace GameOfLife.Validators
{
    public interface IBoardValidator
    {
        public (bool isValid, string errorMessage) ValidateBoard(bool[][] grid);
        public (bool isValid, string errorMessage) ValidateIterations(int iterations);
    }
}