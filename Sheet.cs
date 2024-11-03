namespace MauiCells
{
	public static class Sheet
	{
		public static Dictionary<string, Cell> cells = new Dictionary<string, Cell>();

		public static void AddNewEntry(string cellCode)
		{
			cells[cellCode] = new Cell();
		}

		public static void RemoveEntry(string cellCode)
		{
			if (cells.ContainsKey(cellCode) && CheckIfRemovable(cellCode))
			{
				cells.Remove(cellCode);
			}
		}

		private static bool CheckIfRemovable(string cellCode) //if cell appears somewhere else we cant delete it in the same time
		{
			return cells[cellCode].AppearsInCells.Count == 0;
		}


		public static string GetExpression(string cellCode)
		{
			return cells.ContainsKey(cellCode) ? cells[cellCode].Expression : string.Empty;
		}

		public static double GetValue(string cellCode)
		{
			if (!cells.ContainsKey(cellCode))
			{
				throw new ArgumentException("Cell doesnt exist");
			}
			return cells[cellCode].Value;
		}


		public static double Refresh(string cellCode, string expression)
		{
			Calculator.CurrentCellCode = cellCode;

			if (ContainsLoop(cellCode, expression))
				throw new ArgumentException("Loop detected");

			UpdateDependencies(cellCode, expression); //updating cells which have current cell

			return Calculate(cellCode, expression);
		}

		private static bool ContainsLoop(string cellCode, string expression)
		{
			return Calculator.HasIdentifier(expression, cellCode);
		}

		private static void UpdateDependencies(string cellCode, string expression)
		{
			var dependencies = Calculator.ListOfIdentifiers(expression);

			foreach (var dependent in dependencies)
			{
				if (HasDependencyCycled(dependent))
					throw new ArgumentException("Cyclic dependency");
			}
		}
		public static double Calculate(string cellCode, string expression)
		{
			Calculator.CurrentCellCode = cellCode;
			double value = Calculator.Evaluate(expression); //calculation itself
			cells[cellCode].Expression = expression;
			cells[cellCode].Value = value;
			var OutdatedCells = Sheet.cells[Calculator.CurrentCellCode].AppearsInCells.ToArray();
			foreach (var OutdatedcellCode in OutdatedCells)
			{   //cells that has current cell in their expression must be refreshed after calculations
				Calculate(OutdatedcellCode, Sheet.GetExpression(OutdatedcellCode));
				MainPage.Refresh(OutdatedcellCode);
			}

			return value;
		}

		//checking if there is any cycles between several cells 
		private static bool HasDependencyCycled(string cellCode)
		{
			if (!cells.ContainsKey(cellCode)) return false;

			return cells[cellCode].CellsInsideExpression.Contains(Calculator.CurrentCellCode) ||
				   cells[cellCode].CellsInsideExpression.Any(HasDependencyCycled);
		}

	}
}

