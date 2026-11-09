using System.Numerics;

namespace MyExcelMAUIApp.Model
{
    // значення виразу: ціле число довільної довжини або логічне
    public readonly struct Value
    {
        private readonly BigInteger _number;
        private readonly bool _logical;

        public bool IsLogical { get; }

        public Value(BigInteger number)
        {
            _number = number;
            _logical = false;
            IsLogical = false;
        }

        public Value(bool logical)
        {
            _number = BigInteger.Zero;
            _logical = logical;
            IsLogical = true;
        }

        public BigInteger AsNumber()
        {
            if (IsLogical)
                throw new EvaluationException("Очікувалось число, а не логічне значення");
            return _number;
        }

        public bool AsLogical()
        {
            if (!IsLogical)
                throw new EvaluationException("Очікувалось логічне значення, а не число");
            return _logical;
        }

        public bool SameAs(Value other)
        {
            if (IsLogical != other.IsLogical)
                throw new EvaluationException("Не можна порівнювати число з логічним значенням");
            return IsLogical ? _logical == other._logical : _number == other._number;
        }

        public override string ToString()
        {
            if (IsLogical)
                return _logical ? "ІСТИНА" : "ХИБНІСТЬ";
            return _number.ToString();
        }
    }
}
