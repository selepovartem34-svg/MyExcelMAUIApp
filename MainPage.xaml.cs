using MyExcelMAUIApp.Model;
using Cell = MyExcelMAUIApp.Model.Cell;

namespace MyExcelMAUIApp
{
    public partial class MainPage : ContentPage
    {
        const int CountColumn = 10;
        const int CountRow = 20;

        private readonly Table table = new(CountRow, CountColumn);
        private Entry? currentEntry;

        public MainPage()
        {
            InitializeComponent();
            CreateGrid();
        }

        private void CreateGrid()
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
            currentEntry = null;

            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            grid.ColumnDefinitions.Add(new ColumnDefinition(40));

            for (int col = 1; col <= table.ColumnCount; col++)
                AddColumnHeader(col);

            for (int row = 1; row <= table.RowCount; row++)
                AddRowWithCells(row);

            RefreshValues();
        }

        private void AddColumnHeader(int col)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(100));
            var label = new Label
            {
                Text = Table.GetColumnName(col),
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center
            };
            grid.Add(label, col, 0);
        }

        private void AddRowWithCells(int row)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var label = new Label
            {
                Text = row.ToString(),
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            grid.Add(label, 0, row);

            for (int col = 1; col <= table.ColumnCount; col++)
                AddCellEntry(row, col);
        }

        private void AddCellEntry(int row, int col)
        {
            var entry = new Entry();
            entry.Focused += Entry_Focused;
            entry.Unfocused += Entry_Unfocused;
            entry.Completed += (s, e) => entry.Unfocus();
            grid.Add(entry, col, row);
        }

        private static (int row, int col) Position(Entry entry) => (Grid.GetRow(entry), Grid.GetColumn(entry));

        private void RefreshValues()
        {
            foreach (var child in grid.Children)
            {
                if (child is Entry entry && entry != currentEntry)
                {
                    var (row, col) = Position(entry);
                    entry.Text = table.GetCell(row, col).DisplayText;
                }
            }
        }

        private void Entry_Focused(object? sender, FocusEventArgs e)
        {
            var entry = (Entry)sender!;
            currentEntry = entry;
            var (row, col) = Position(entry);
            Cell cell = table.GetCell(row, col);

            entry.Text = cell.Expression;
            currentCellLabel.Text = cell.Name;
            textInput.Text = cell.Expression;
            statusLabel.Text = cell.Error ?? "";
        }

        // викликається, коли користувач вийде зі зміненої клітинки (втратить фокус)
        private void Entry_Unfocused(object? sender, FocusEventArgs e)
        {
            var entry = (Entry)sender!;
            var (row, col) = Position(entry);
            if (currentEntry == entry)
                currentEntry = null;

            table.SetExpression(row, col, entry.Text ?? "");
            Cell cell = table.GetCell(row, col);
            textInput.Text = cell.Expression;
            statusLabel.Text = cell.Error ?? "";
            RefreshValues();
        }

        private void TextInput_Completed(object? sender, EventArgs e)
        {
            if (!Table.TryParseCellName(currentCellLabel.Text ?? "", out int row, out int col)
                || row > table.RowCount || col > table.ColumnCount)
            {
                statusLabel.Text = "Спочатку виберіть клітинку";
                return;
            }

            table.SetExpression(row, col, textInput.Text ?? "");
            statusLabel.Text = table.GetCell(row, col).Error ?? "";
            RefreshValues();
        }

        private async void CalculateButton_Clicked(object sender, EventArgs e)
        {
            table.Recalculate();
            RefreshValues();

            var errors = new List<string>();
            for (int row = 1; row <= table.RowCount; row++)
                for (int col = 1; col <= table.ColumnCount; col++)
                {
                    Cell cell = table.GetCell(row, col);
                    if (cell.Error != null)
                        errors.Add($"{cell.Name}: {cell.Error}");
                }

            if (errors.Count > 0)
                await DisplayAlert("Помилки в таблиці", string.Join("\n", errors.Take(10)), "OK");
            else
                statusLabel.Text = "Таблицю перераховано, помилок немає";
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            string? fileName = await DisplayPromptAsync("Зберегти", "Назва файлу:", initialValue: "table");
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string path = Path.Combine(folder, fileName.Trim() + ".json");
            try
            {
                await table.SaveAsync(path);
                await DisplayAlert("Збережено", $"Таблицю збережено у файл:\n{path}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", $"Не вдалося зберегти файл: {ex.Message}", "OK");
            }
        }

        private async void ReadButton_Clicked(object sender, EventArgs e)
        {
            var fileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".json" } }
            });

            try
            {
                FileResult? file = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Виберіть файл таблиці",
                    FileTypes = fileType
                });
                if (file == null)
                    return;

                await using var stream = await file.OpenReadAsync();
                await table.LoadAsync(stream);
                CreateGrid();
                currentCellLabel.Text = "";
                textInput.Text = "";
                statusLabel.Text = $"Відкрито файл {file.FileName}";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", $"Не вдалося прочитати файл: {ex.Message}", "OK");
            }
        }

        private async void ExitButton_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Підтвердження", "Ви дійсно хочете вийти?", "Так", "Ні");
            if (answer)
            {
                System.Environment.Exit(0);
            }
        }

        private async void HelpButton_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Довідка",
                "Лабораторна робота 1. Студента групи К25 Шелепова Артема.\n\n" +
                "У клітинку можна ввести число або вираз (знак = на початку необов'язковий).\n" +
                "Операції: +, -, *, /, ^ (степінь), унарні + і -, дужки.\n" +
                "Функції: mod(a, b), div(a, b), mmax(a, b, ...), mmin(a, b, ...).\n" +
                "Посилання на клітинки: A1, B2, ... Значення перераховуються автоматично.\n" +
                "Помилки (ділення на нуль, циклічні посилання, неіснуючі клітинки) " +
                "показуються як #ПОМИЛКА, опис - під рядком введення.\n" +
                "Зберегти - у JSON-файл у папці Документи, Прочитати - вибір файлу.",
                "OK");
        }

        private void DeleteRowButton_Clicked(object sender, EventArgs e)
        {
            if (table.RowCount <= 1)
                return;

            int lastRow = table.RowCount;
            foreach (var child in grid.Children.Where(c => Grid.GetRow((BindableObject)c) == lastRow).ToList())
                grid.Children.Remove(child);
            grid.RowDefinitions.RemoveAt(lastRow);

            table.DeleteLastRow();
            RefreshValues();
        }

        private void DeleteColumnButton_Clicked(object sender, EventArgs e)
        {
            if (table.ColumnCount <= 1)
                return;

            int lastColumn = table.ColumnCount;
            foreach (var child in grid.Children.Where(c => Grid.GetColumn((BindableObject)c) == lastColumn).ToList())
                grid.Children.Remove(child);
            grid.ColumnDefinitions.RemoveAt(lastColumn);

            table.DeleteLastColumn();
            RefreshValues();
        }

        private void AddRowButton_Clicked(object sender, EventArgs e)
        {
            table.AddRow();
            AddRowWithCells(table.RowCount);
            RefreshValues();
        }

        private void AddColumnButton_Clicked(object sender, EventArgs e)
        {
            table.AddColumn();
            int col = table.ColumnCount;
            AddColumnHeader(col);
            for (int row = 1; row <= table.RowCount; row++)
                AddCellEntry(row, col);
            RefreshValues();
        }
    }
}
