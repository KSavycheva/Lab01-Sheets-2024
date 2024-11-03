using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Antlr4.Runtime;

namespace MauiCells
{/* Calculator class - responsible for identifying cell references */
    public static class Calculator
    {
        public static string CurrentCellCode;

        public static double Evaluate(string expression)
        {
            if (!string.IsNullOrEmpty(CurrentCellCode)) // if expr is null - clearing ties between interacting cells
            {
                foreach (var OutdatedcellCode in Sheet.cells[Calculator.CurrentCellCode].CellsInsideExpression)
                {
                    Sheet.cells[OutdatedcellCode].AppearsInCells.Remove(Calculator.CurrentCellCode);
                }
                Sheet.cells[Calculator.CurrentCellCode].CellsInsideExpression.Clear();
            }

            if (expression.Equals(string.Empty)) return 0;

            if (Regex.Match(expression, @"^([+-]?\d+(\.\d+)?)$").Success) // checking if expression is num
            {
                return Double.Parse(expression);
            }

            var lexer = new GrammarLexer(new AntlrInputStream(expression)); //breaks into tokens
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(new ThrowExceptionErrorListener());

            var tokens = new CommonTokenStream(lexer);

            var parser = new GrammarParser(tokens);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new ThrowExceptionErrorListener());
            var tree = parser.compileUnit(); //building of syntax tree

            var visitor = new GrammarVisitor();
            return visitor.Visit(tree);
        }

        // Checks if a specific identifier (cell reference) exists in the expression
        public static bool HasIdentifier(string expression, string identifier)
        {
            string identifierRegex = @"[a-zA-Z]+[1-9][0-9]*";
            foreach (Match m in Regex.Matches(expression, identifierRegex).Cast<Match>())
            {
                if (m.Value.Equals(identifier)) return true;
            }
            return false;
        }

        public static List<string> ListOfIdentifiers(string expression)
        {
            string identifierRegex = @"[a-zA-Z]+[1-9][0-9]*";
            return Regex.Matches(expression, identifierRegex).Cast<Match>().Select(match => match.Value).ToList();
        }
    }

}

