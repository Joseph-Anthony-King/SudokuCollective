namespace SudokuCollective.Core.Structs
{
    public struct SudokuCellEventArgs
    {
        public int Index { get; set; }
        public int Value { get; set; }
        public int Column { get; set; }
        public int Region { get; set; }
        public int Row { get; set; }

        public SudokuCellEventArgs(
            int index,
            int value,
            int column,
            int region,
            int row)
        {
            Index = index;
            Value = value;
            Column = column;
            Region = region;
            Row = row;
        }
    }
}
