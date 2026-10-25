using Antlr4.Runtime;

namespace MyExcelMAUIApp.Model
{
    public class ThrowExceptionErrorListener : BaseErrorListener, IAntlrErrorListener<int>
    {
        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol,
            int line, int charPositionInLine, string msg, RecognitionException e)
        {
            throw new EvaluationException($"Синтаксична помилка в позиції {charPositionInLine + 1}");
        }

        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol,
            int line, int charPositionInLine, string msg, RecognitionException e)
        {
            throw new EvaluationException($"Невідомий символ у позиції {charPositionInLine + 1}");
        }
    }
}
