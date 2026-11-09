namespace MyExcelMAUIApp.Model
{
    public class Cell
    {
        public string Name { get; }
        public string Expression { get; set; } = "";
        public Value Value { get; set; }
        public string? Error { get; set; }

        public Cell(string name)
        {
            Name = name;
        }

        public bool IsEmpty => string.IsNullOrWhiteSpace(Expression);

        public string DisplayText
        {
            get
            {
                if (IsEmpty) return "";
                if (Error != null) return "#ПОМИЛКА";
                return Value.ToString();
            }
        }
    }
}
