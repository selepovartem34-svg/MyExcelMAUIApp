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
        public static double Evaluate(string expression, Func<string, double> resolveCell)
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
            return visitor.Visit(tree);
        }
    }
}
