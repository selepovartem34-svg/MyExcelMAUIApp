using System.Text.Json;

namespace MyExcelMAUIApp.Model
{
    public class Table
    {
        private readonly Dictionary<string, Cell> _cells = new();

        public int RowCount { get; private set; }
        public int ColumnCount { get; private set; }

        public Table(int rows, int columns)
        {
            RowCount = rows;
            ColumnCount = columns;
        }

        public static string GetColumnName(int colIndex)
        {
            int dividend = colIndex;
            string columnName = string.Empty;
            while (dividend > 0)
            {
                int modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }
            return columnName;
        }

        public static string GetCellName(int row, int col) => GetColumnName(col) + row;

        public Cell GetCell(int row, int col)
        {
            string name = GetCellName(row, col);
            if (!_cells.TryGetValue(name, out var cell))
            {
                cell = new Cell(name);
                _cells[name] = cell;
            }
            return cell;
        }

        public void SetExpression(int row, int col, string expression)
        {
            GetCell(row, col).Expression = expression.Trim();
            Recalculate();
        }

        public void AddRow() => RowCount++;

        public void AddColumn() => ColumnCount++;

        public void DeleteLastRow()
        {
            for (int col = 1; col <= ColumnCount; col++)
                _cells.Remove(GetCellName(RowCount, col));
            RowCount--;
            Recalculate();
        }

        public void DeleteLastColumn()
        {
            for (int row = 1; row <= RowCount; row++)
                _cells.Remove(GetCellName(row, ColumnCount));
            ColumnCount--;
            Recalculate();
        }

        public void Recalculate()
        {
            var calculated = new HashSet<string>();
            foreach (var cell in _cells.Values.ToList())
                Evaluate(cell, calculated, new HashSet<string>());
        }

        private void Evaluate(Cell cell, HashSet<string> calculated, HashSet<string> inProgress)
        {
            if (calculated.Contains(cell.Name)) return;

            if (!inProgress.Add(cell.Name))
                throw new EvaluationException($"Циклічне посилання на {cell.Name}");

            try
            {
                string text = cell.Expression.StartsWith('=') ? cell.Expression[1..] : cell.Expression;
                cell.Value = cell.IsEmpty ? 0 : Calculator.Evaluate(text, name => ResolveCell(name, calculated, inProgress));
                cell.Error = double.IsFinite(cell.Value) ? null : "Результат не є числом";
            }
            catch (EvaluationException ex)
            {
                cell.Value = 0;
                cell.Error = ex.Message;
            }
            finally
            {
                inProgress.Remove(cell.Name);
                calculated.Add(cell.Name);
            }
        }

        private double ResolveCell(string name, HashSet<string> calculated, HashSet<string> inProgress)
        {
            if (!TryParseCellName(name, out int row, out int col) || row < 1 || row > RowCount || col < 1 || col > ColumnCount)
                throw new EvaluationException($"Клітинки {name} не існує");

            Cell cell = GetCell(row, col);
            if (inProgress.Contains(cell.Name))
                throw new EvaluationException($"Циклічне посилання на {cell.Name}");

            Evaluate(cell, calculated, inProgress);
            if (cell.Error != null)
                throw new EvaluationException($"Помилка в клітинці {name}");
            return cell.Value;
        }

        public static bool TryParseCellName(string name, out int row, out int col)
        {
            row = 0;
            col = 0;
            int i = 0;
            while (i < name.Length && char.IsLetter(name[i]))
            {
                col = col * 26 + (char.ToUpperInvariant(name[i]) - 'A' + 1);
                i++;
            }
            return i > 0 && i < name.Length && int.TryParse(name[i..], out row);
        }

        public async Task SaveAsync(string path)
        {
            var data = new TableData
            {
                Rows = RowCount,
                Columns = ColumnCount,
                Cells = _cells.Values.Where(c => !c.IsEmpty).ToDictionary(c => c.Name, c => c.Expression)
            };
            await using var stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, data, new JsonSerializerOptions { WriteIndented = true });
        }

        public async Task LoadAsync(Stream stream)
        {
            var data = await JsonSerializer.DeserializeAsync<TableData>(stream)
                       ?? throw new InvalidDataException("Порожній файл");

            _cells.Clear();
            RowCount = data.Rows;
            ColumnCount = data.Columns;
            foreach (var (name, expression) in data.Cells)
            {
                if (TryParseCellName(name, out int row, out int col))
                    GetCell(row, col).Expression = expression;
            }
            Recalculate();
        }

        private class TableData
        {
            public int Rows { get; set; }
            public int Columns { get; set; }
            public Dictionary<string, string> Cells { get; set; } = new();
        }
    }
}
