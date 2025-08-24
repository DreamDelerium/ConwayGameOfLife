namespace GameOfLife.Models.DTO
{
    public class UploadBoardRequestDto
    {
        /// <example>
        /// [[true, false, true],
        ///  [false, true, false],
        ///  [true, false, true]]
        /// </example>
        public bool[][] Grid { get; set; } = Array.Empty<bool[]>();
    }
}
