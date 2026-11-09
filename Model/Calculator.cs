using Antlr4.Runtime;
using MyExcelMAUIApp.Grammar;

namespace MyExcelMAUIApp.Model
{
    public class EvaluationException : Exception
    {
        public EvaluationException(string message) : base(message) { }
    }

    public static class Calculator
    {
        public static Value Evaluate(string expression, Func<string, Value> resolveCell)
        {
            var errorListener = new ThrowExceptionErrorListener();

            var lexer = new LabCalculatorLexer(new AntlrInputStream(expression));
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(errorListener);

            var parser = new LabCalculatorParser(new CommonTokenStream(lexer));
            parser.RemoveErrorListeners();
            parser.AddErrorListener(errorListener);

            var tree = parser.compileUnit();
            var visitor = new LabCalculatorVisitor(resolveCell);
            Value value = visitor.Visit(tree);

            // у варіанті 45 вираз логічний, окреме ціле число - це дані для інших клітинок
            if (!value.IsLogical && tree.expression() is not LabCalculatorParser.NumberExprContext)
                throw new EvaluationException("Вираз має бути логічним");

            return value;
        }
    }
}
