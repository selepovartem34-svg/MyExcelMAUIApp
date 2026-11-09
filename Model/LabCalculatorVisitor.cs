using System.Numerics;
using MyExcelMAUIApp.Grammar;

namespace MyExcelMAUIApp.Model
{
    public class LabCalculatorVisitor : LabCalculatorBaseVisitor<Value>
    {
        private const int MaxExponent = 100000;

        private readonly Func<string, Value> _resolveCell;

        public LabCalculatorVisitor(Func<string, Value> resolveCell)
        {
            _resolveCell = resolveCell;
        }

        public override Value VisitCompileUnit(LabCalculatorParser.CompileUnitContext context)
        {
            return Visit(context.expression());
        }

        public override Value VisitNumberExpr(LabCalculatorParser.NumberExprContext context)
        {
            return new Value(BigInteger.Parse(context.GetText()));
        }

        public override Value VisitIdentifierExpr(LabCalculatorParser.IdentifierExprContext context)
        {
            return _resolveCell(context.GetText().ToUpperInvariant());
        }

        public override Value VisitParenthesizedExpr(LabCalculatorParser.ParenthesizedExprContext context)
        {
            return Visit(context.expression());
        }

        public override Value VisitExponentialExpr(LabCalculatorParser.ExponentialExprContext context)
        {
            var left = WalkLeft(context).AsNumber();
            var right = WalkRight(context).AsNumber();

            if (right.Sign < 0)
                throw new EvaluationException("Степінь не може бути від'ємним");
            if (right > MaxExponent)
                throw new EvaluationException($"Степінь не може бути більшим за {MaxExponent}");

            return new Value(BigInteger.Pow(left, (int)right));
        }

        public override Value VisitMultiplicativeExpr(LabCalculatorParser.MultiplicativeExprContext context)
        {
            var left = WalkLeft(context).AsNumber();
            var right = WalkRight(context).AsNumber();

            if (context.operatorToken.Type == LabCalculatorLexer.MULTIPLY)
                return new Value(left * right);

            if (right.IsZero)
                throw new EvaluationException("Ділення на нуль");
            return new Value(left / right);
        }

        public override Value VisitAdditiveExpr(LabCalculatorParser.AdditiveExprContext context)
        {
            var left = WalkLeft(context).AsNumber();
            var right = WalkRight(context).AsNumber();

            return context.operatorToken.Type == LabCalculatorLexer.ADD
                ? new Value(left + right)
                : new Value(left - right);
        }

        public override Value VisitComparisonExpr(LabCalculatorParser.ComparisonExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            return context.operatorToken.Type switch
            {
                LabCalculatorLexer.EQUAL => new Value(left.SameAs(right)),
                LabCalculatorLexer.LESS => new Value(left.AsNumber() < right.AsNumber()),
                _ => new Value(left.AsNumber() > right.AsNumber())
            };
        }

        public override Value VisitNotExpr(LabCalculatorParser.NotExprContext context)
        {
            return new Value(!Visit(context.expression()).AsLogical());
        }

        public override Value VisitMaxMinExpr(LabCalculatorParser.MaxMinExprContext context)
        {
            var left = WalkLeft(context).AsNumber();
            var right = WalkRight(context).AsNumber();

            return context.operatorToken.Type == LabCalculatorLexer.MAX
                ? new Value(BigInteger.Max(left, right))
                : new Value(BigInteger.Min(left, right));
        }

        private Value WalkLeft(LabCalculatorParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<LabCalculatorParser.ExpressionContext>(0));
        }

        private Value WalkRight(LabCalculatorParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<LabCalculatorParser.ExpressionContext>(1));
        }
    }
}
