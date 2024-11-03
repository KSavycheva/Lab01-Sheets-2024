using System.Diagnostics;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace MauiCells
{
    public class GrammarVisitor : GrammarBaseVisitor<double>
    {
        public override double VisitCompileUnit(GrammarParser.CompileUnitContext context)
        {
            return Visit(context.expression());
        }

        public override double VisitNumberExpr(GrammarParser.NumberExprContext context)
        {
            var result = double.Parse(context.GetText());
            Debug.WriteLine(result);

            return result;
        }

        public override double VisitParenthesizedExpr(GrammarParser.ParenthesizedExprContext context)
        {
            return Visit(context.expression());
        }

        public override double VisitIdentifierExpr(GrammarParser.IdentifierExprContext context)
        {
            var identifier = context.GetText();
            Sheet.cells[Calculator.CurrentCellCode].CellsInsideExpression.Add(identifier);
            Sheet.cells[identifier].AppearsInCells.Add(Calculator.CurrentCellCode);
            return Sheet.GetValue(identifier);
        }


        public override double VisitExponentialExpr(GrammarParser.ExponentialExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            if (left == 0 && right < 0)
            {
                throw new ArgumentException();
            }

            Debug.WriteLine("{0} ^ {1}", left, right);
            return System.Math.Pow(left, right);
        }


        public override double VisitUnaryExpr([NotNull] GrammarParser.UnaryExprContext context)
        {
            var left = WalkLeft(context);

            if (context.operatorToken.Type == GrammarLexer.PLUS)
                return left;
            else return -left;
        }

        public override double VisitIncDecExpr(GrammarParser.IncDecExprContext context)
        {
            var left = WalkLeft(context);
            if (context.operatorToken.Type == GrammarLexer.INC)
                return left + 1;
            else return left - 1;
        }

        public override double VisitMMinExpr(GrammarParser.MMinExprContext context)
        {
            double minimum = Double.PositiveInfinity;
            foreach (var child in context.paramlist.children.OfType<GrammarParser.ExpressionContext>())
            {
                double childValue = this.Visit(child);
                if (childValue < minimum)
                {
                    minimum = childValue;
                }
            }
            return minimum;
        }

        public override double VisitMMaxExpr(GrammarParser.MMaxExprContext context)
        {
            double maximum = Double.NegativeInfinity;
            foreach (var child in context.paramlist.children.OfType<GrammarParser.ExpressionContext>())
            {
                double childValue = this.Visit(child);
                if (childValue > maximum)
                {
                    maximum = childValue;
                }
            }
            return maximum;
        }

        private double WalkLeft(GrammarParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<GrammarParser.ExpressionContext>(0));
        }

        private double WalkRight(GrammarParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<GrammarParser.ExpressionContext>(1));
        }
    }
}

