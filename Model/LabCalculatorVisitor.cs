using System.Globalization;
using MyExcelMAUIApp.Grammar;

namespace MyExcelMAUIApp.Model
{
    public class LabCalculatorVisitor : LabCalculatorBaseVisitor<double>
    {
        private readonly Func<string, double> _resolveCell;

        public LabCalculatorVisitor(Func<string, double> resolveCell)
        {
            _resolveCell = resolveCell;
        }

        public override double VisitCompileUnit(LabCalculatorParser.CompileUnitContext context)
        {
            return Visit(context.expression());
        }

        public override double VisitNumberExpr(LabCalculatorParser.NumberExprContext context)
        {
            return double.Parse(context.GetText(), CultureInfo.InvariantCulture);
        }

        public override double VisitIdentifierExpr(LabCalculatorParser.IdentifierExprContext context)
        {
            return _resolveCell(context.GetText().ToUpperInvariant());
        }

        public override double VisitParenthesizedExpr(LabCalculatorParser.ParenthesizedExprContext context)
        {
            return Visit(context.expression());
        }

        public override double VisitExponentialExpr(LabCalculatorParser.ExponentialExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);
            return Math.Pow(left, right);
        }

        public override double VisitUnaryExpr(LabCalculatorParser.UnaryExprContext context)
        {
            var value = Visit(context.expression());
            return context.operatorToken.Type == LabCalculatorLexer.SUBTRACT ? -value : value;
        }

        public override double VisitMultiplicativeExpr(LabCalculatorParser.MultiplicativeExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            if (context.operatorToken.Type == LabCalculatorLexer.MULTIPLY)
                return left * right;

            CheckNotZero(right);
            return left / right;
        }

        public override double VisitAdditiveExpr(LabCalculatorParser.AdditiveExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            return context.operatorToken.Type == LabCalculatorLexer.ADD
                ? left + right
                : left - right;
        }

        public override double VisitModDivExpr(LabCalculatorParser.ModDivExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);
            CheckNotZero(right);

            return context.operatorToken.Type == LabCalculatorLexer.MOD
                ? left % right
                : Math.Truncate(left / right);
        }

        public override double VisitMminExpr(LabCalculatorParser.MminExprContext context)
        {
            double minValue = double.PositiveInfinity;

            foreach (var child in context.paramlist.expression())
            {
                double childValue = Visit(child);
                if (childValue < minValue)
                {
                    minValue = childValue;
                }
            }
            return minValue;
        }

        public override double VisitMmaxExpr(LabCalculatorParser.MmaxExprContext context)
        {
            double maxValue = double.NegativeInfinity;

            foreach (var child in context.paramlist.expression())
            {
                double childValue = Visit(child);
                if (childValue > maxValue)
                {
                    maxValue = childValue;
                }
            }
            return maxValue;
        }

        private double WalkLeft(LabCalculatorParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<LabCalculatorParser.ExpressionContext>(0));
        }

        private double WalkRight(LabCalculatorParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<LabCalculatorParser.ExpressionContext>(1));
        }

        private static void CheckNotZero(double value)
        {
            if (value == 0)
                throw new EvaluationException("Ділення на нуль");
        }
    }
}
