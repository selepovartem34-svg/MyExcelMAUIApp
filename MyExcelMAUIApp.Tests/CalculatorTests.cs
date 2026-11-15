using Xunit;
using MyExcelMAUIApp.Model;

namespace MyExcelMAUIApp.Tests
{
    public class CalculatorTests
    {
        [Fact]
        public void ComparisonWithCells()
        {
            var table = new Table(10, 10);
            table.SetExpression(1, 1, "7");
            table.SetExpression(1, 2, "3");
            table.SetExpression(1, 3, "A1+B1>9");

            Cell cell = table.GetCell(1, 3);

            Assert.Null(cell.Error);
            Assert.True(cell.Value.AsLogical());
            Assert.Equal("ІСТИНА", cell.DisplayText);
        }

        [Fact]
        public void MaxMinAndNot()
        {
            Value value = Calculator.Evaluate("not(max(2,5)*3=min(15,20))", _ => default);

            Assert.False(value.AsLogical());
        }

        [Fact]
        public void DivisionByZero()
        {
            var ex = Assert.Throws<EvaluationException>(() => Calculator.Evaluate("1/0>0", _ => default));

            Assert.Equal("Ділення на нуль", ex.Message);
        }
    }
}
