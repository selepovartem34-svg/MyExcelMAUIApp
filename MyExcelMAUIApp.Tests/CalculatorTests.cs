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

        [Fact]
        public void PowerIsRightAssociative()
        {
            // 2^3^2 = 2^(3^2) = 512, а не (2^3)^2 = 64
            Value value = Calculator.Evaluate("2^3^2=512", _ => default);

            Assert.True(value.AsLogical());
        }

        [Fact]
        public void IntegerDivision()
        {
            Value value = Calculator.Evaluate("7/2=3", _ => default);

            Assert.True(value.AsLogical());
        }

        [Fact]
        public void BigNumbers()
        {
            Value value = Calculator.Evaluate("99999999999999999999+1=100000000000000000000", _ => default);

            Assert.True(value.AsLogical());
        }

        [Fact]
        public void RecalculateAfterChange()
        {
            var table = new Table(10, 10);
            table.SetExpression(1, 1, "5");
            table.SetExpression(1, 2, "A1>10");
            Assert.Equal("ХИБНІСТЬ", table.GetCell(1, 2).DisplayText);

            table.SetExpression(1, 1, "20");

            Assert.Equal("ІСТИНА", table.GetCell(1, 2).DisplayText);
        }

        [Fact]
        public void CyclicReference()
        {
            var table = new Table(10, 10);
            table.SetExpression(1, 1, "B1=1");
            table.SetExpression(1, 2, "A1=1");

            Cell a1 = table.GetCell(1, 1);
            Cell b1 = table.GetCell(1, 2);

            Assert.Equal("#ПОМИЛКА", a1.DisplayText);
            Assert.Equal("#ПОМИЛКА", b1.DisplayText);
            Assert.Contains(new[] { a1.Error, b1.Error }, e => e != null && e.Contains("Циклічне посилання"));
        }

        [Fact]
        public void ReferenceToMissingCell()
        {
            var table = new Table(2, 2);
            table.SetExpression(1, 1, "C5>0");

            Assert.Equal("Клітинки C5 не існує", table.GetCell(1, 1).Error);
        }

        [Fact]
        public void SyntaxError()
        {
            var ex = Assert.Throws<EvaluationException>(() => Calculator.Evaluate("1+>2", _ => default));

            Assert.StartsWith("Синтаксична помилка", ex.Message);
        }
    }
}
