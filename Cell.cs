namespace MauiCells
{
	[Serializable]
	public class Cell
	{
		public string Expression { get; set; }
		public double Value { get; set; }

		public HashSet<string> CellsInsideExpression { get; set; } //for cells identifiers which are mentioned in current cell´s expression
		public HashSet<string> AppearsInCells { get; set; } // cells that have identifier of current cell in their own expressions

		public Cell()
		{
			Expression = string.Empty;
			Value = 0;
			CellsInsideExpression = new HashSet<string>();
			AppearsInCells = new HashSet<string>();
		}

		public Cell(double val)
		{
			Expression = string.Empty;
			Value = val;
			CellsInsideExpression = new HashSet<string>();
			AppearsInCells = new HashSet<string>();
		}
	}
}

