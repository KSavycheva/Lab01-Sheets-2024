using Newtonsoft.Json;
using System;
using CommunityToolkit.Maui.Storage;


namespace MauiCells;

public partial class MainPage : ContentPage
{

    static int CountColumn = 5;
    static int CountRow = 8;

    readonly static Dictionary<string, IView> Cells = new(capacity: (int)(CountColumn * CountRow * 1.4));

    ////////////////   INIT PAGE   /////////////////

    public MainPage()
    {
        InitializeComponent();
        this.Title = "Excel Cubik edition";
        CreateGrid();
    }

    private void CreateGrid()
    {
        AddColumnsAndColumnLabels();
        AddRowsAndCellEntries();
    }

    private void AddColumnsAndColumnLabels()
    {
        grid.RowDefinitions.Add(new RowDefinition()); // Add Row for labels

        for (int col = 0; col <= CountColumn; col++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            var label = NewLabel(CalcColName(col));
            AddLabel(label, 0, col);
        }
    }

    private void AddRowsAndCellEntries()
    {
        for (int row = 1; row <= CountRow; row++)
        {
            grid.RowDefinitions.Add(new RowDefinition());
            var label = NewLabel(row.ToString());
            AddLabel(label, row, 0); // add lable of row in the beginning of it
            for (int col = 1; col <= CountColumn; col++)
            {
                var entry = NewEmptyEntry();
                var cellIdentifier = CalcColName(col) + label.Text;
                AddEntry(entry, cellIdentifier, row, col);
            }
        }
    }

    ////////////////   CLICK HANDLERS  /////////////////

    private void OnAddRowBtnClicked(object sender, EventArgs e) //handling adding a row by pressing button
    {
        int newRowIndex = grid.RowDefinitions.Count;
        grid.RowDefinitions.Add(new RowDefinition());
        var label = NewLabel(newRowIndex.ToString());
        AddLabel(label, newRowIndex, 0);
        for (int col = 1; col <= CountColumn; col++)
        {
            var entry = NewEmptyEntry();
            var cellCode = CalcColName(col) + label.Text;
            AddEntry(entry, cellCode, newRowIndex, col);
        }
        CountRow++;
    }

    private async void OnDelRowBtnClicked(object sender, EventArgs e)
    {
        if (grid.RowDefinitions.Count <= 2) return; //having at least one cell is mandatory 

        for (int col = 1; col <= CountColumn; col++)
        {
            var cellCode = CalcColName(col) + CountRow;

            if (Sheet.cells[cellCode].AppearsInCells.Count > 0) //checking if any cell from row is written in any expression within the sheet
            {
                string referencedCell = Sheet.cells[cellCode].AppearsInCells.First();
                await DisplayAlert("Помилка", $"Клітина {cellCode} фігурує в виразі клітинки {referencedCell}. Видалення рядка неможливе.", "Ок");
                return;
            }
        }

        int lastRowIndex = grid.RowDefinitions.Count - 1;
        grid.RowDefinitions.RemoveAt(lastRowIndex);

        grid.Children.Remove(Cells[CountRow.ToString()]);
        Cells.Remove(CountRow.ToString());

        for (int col = 1; col <= CountColumn; col++)
        {
            var cellCode = CalcColName(col) + CountRow;
            grid.Children.Remove(Cells[cellCode]);
            Cells.Remove(cellCode);
            Sheet.RemoveEntry(cellCode);
        }
        CountRow--;
    }

    private void OnAddColBtnClicked(object sender, EventArgs e)
    {
        int newColumnIndex = grid.ColumnDefinitions.Count;
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        var label = NewLabel(CalcColName(newColumnIndex));
        AddLabel(label, 0, newColumnIndex);
        for (int row = 1; row <= CountRow; row++)
        {
            var entry = NewEmptyEntry();
            var cellCode = label.Text + row;
            AddEntry(entry, cellCode, row, newColumnIndex);
        }
        CountColumn++;
    }

    private async void OnDelColBtnClicked(object sender, EventArgs e)
    {
        if (grid.ColumnDefinitions.Count <= 2) return;
        string columnName = CalcColName(CountColumn);

        for (int row = 1; row <= CountRow; row++)
        {
            var cellCode = columnName + row;
            if (Sheet.cells[cellCode].AppearsInCells.Count != 0)
            {
                string firstcellCode = Sheet.cells[cellCode].AppearsInCells.First();
                await DisplayAlert("Помилка", "Клітина " + cellCode + " фігурує в виразі клітинки " + firstcellCode + ". Видалення стовпця неможливе.", "Ок");
                return;
            }
        }

        int lastColumnIndex = grid.ColumnDefinitions.Count - 1;
        grid.ColumnDefinitions.RemoveAt(lastColumnIndex);

        grid.Children.Remove(Cells[columnName]);
        Cells.Remove(columnName);

        for (int row = 1; row <= CountRow; row++)
        {
            var cellCode = columnName + row;
            grid.Children.Remove(Cells[cellCode]);
            Cells.Remove(cellCode);
            Sheet.RemoveEntry(cellCode);
        }
        CountColumn--;
    }

    private async void OnHelpBtnClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Довідка", "Лабораторна робота 1. Студентки Савичевої Катерини", "OK");
    }

    private async void OnSaveBtnClicked(object sender, EventArgs e)
    {
        string currentDateTime = DateTime.Now.ToString("dd-MM-HH-mm-ss");

        var SheetData = new { rows = CountRow, cols = CountColumn, cells = Sheet.cells };
        string sheetInJson = JsonConvert.SerializeObject(SheetData);
        try
        {
            string fileName = $"{currentDateTime}.json";
            using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(sheetInJson)))
            {
                await FileSaver.Default.SaveAsync(fileName, stream);
            }
            await Shell.Current.DisplayAlert("Є!", $"Таблиця була збережена!", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Помилка", $"Не вийшло зберегти таблицю: {ex.Message}", "OK");
        }
    }

    private async void OnReadBtnClicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Попередження", "Ця дія видалить поточну таблицю та завантажить іншу. Хочете продовжити?", "Так", "Ні");
        if (answer)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Виберіть файл",
                });
                if (result != null)
                {
                    Cells.Clear();                     //cleaning 
                    Sheet.cells.Clear();               //after an 
                    grid.ColumnDefinitions.Clear();    //old sheet ....
                    grid.RowDefinitions.Clear();
                    grid.Children.Clear();

                    ApplyDataFromJson(result.FullPath);  //we will get data from json which user selected and then apply it to app
                    await Shell.Current.DisplayAlert("Є!", $"Таблиця завантажена!", "OK");
                }

            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Не вийшло завантажити таблицю: {ex.Message}", "OK");
            }
        }
    }

    private class SheetData //created only for working with json
    {
        public int cols { get; set; }
        public int rows { get; set; }
        public Dictionary<string, Cell> Cells { get; set; }
    }

    private void ApplyDataFromJson(string filepath)
    {
        string json = File.ReadAllText(filepath);

        SheetData? savedSheet = JsonConvert.DeserializeObject<SheetData>(json);
        if (savedSheet == null)
        {
            throw new InvalidOperationException("Не вийшло прочитати дані з JSON.");
        }
        // here we gonna load an actual sheet to look at 
        CountColumn = savedSheet.cols;
        CountRow = savedSheet.rows;
        CreateGrid();
        foreach (var pair in savedSheet.Cells)
        {
            string key = pair.Key;
            var entry = (Entry)Cells[key];
            Cell cell = pair.Value;
            if (cell.Expression.Equals(string.Empty)) entry.Text = string.Empty;
            else entry.Text = cell.Value.ToString();

            Sheet.cells[key] = cell;
        }
    }

    private async void OnExitBtnClicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Підтвердження", "Ви дійсно хочете вийти ? ", "Так", "Ні");
        if (answer)
        {
            System.Environment.Exit(0);
        }

    }

    ////////////////   UTIL FUNCTIONS  /////////////////

    const int engLettersNum = 26; // amount of letters in English alphabet
    const int asciiForA = 65; // ASCII value for the character 'A'
    private static string CalcColName(int colIndex) //creating column's name
    {
        int div = colIndex;
        string columnName = string.Empty;
        while (div > 0)
        {
            int modulo = (div - 1) % engLettersNum;
            columnName = Convert.ToChar(asciiForA + modulo) + columnName; //creating letters 
            div = (div - modulo) / engLettersNum;
        }
        return columnName;
    }

    private void Entry_Focused(object sender, FocusEventArgs e) // when clicking on specific cell
    {
       if (sender is Entry entry)
    {
        int row = Grid.GetRow(entry);
        int col = Grid.GetColumn(entry);
        var cellCoords = CalcColName(col) + row;
        entry.Text = Sheet.GetExpression(cellCoords); //showing hidden in cell expression for user
    }
    }

    async private void Entry_Unfocused(object sender, FocusEventArgs e) // clicking on something beyond cell
    {
        var entry = (Entry)sender;
        int row = Grid.GetRow(entry);
        int col = Grid.GetColumn(entry);
        var cellCoords = CalcColName(col) + row;
        try
        {
            Sheet.Refresh(cellCoords, entry.Text).ToString();
            Refresh(cellCoords);
        }
        catch (ArgumentException)
        {
            Refresh(cellCoords);
            await DisplayAlert("Помилка", "Було введено недопустимий вираз", "Ок");
        }
        catch 
        { 

        }
    }

    public static void Refresh(string cellCode) // for automatic modifying of data in cells 
    {
        var entry = (Entry)Cells[cellCode];
        entry.Text = Sheet.GetValue(cellCode).ToString();
    }

    private static Label NewLabel(string text)
    {
        return new Label
        {
            Text = text,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };
    }

    private void AddLabel(Label label, int row, int col)
    {
        Grid.SetRow(label, row);
        Grid.SetColumn(label, col);
        grid.Children.Add(label);
        Cells.Add(label.Text, label);
    }
    
    private Entry NewEmptyEntry()
    {
        var entry = new Entry
        {
            Text = string.Empty
        };
        entry.Focused += Entry_Focused;
        entry.Unfocused += Entry_Unfocused;
        return entry;
    }
    
    private void AddEntry(Entry entry, string cellCode, int row, int col)
    {
        Grid.SetRow(entry, row);
        Grid.SetColumn(entry, col);
        grid.Children.Add(entry);

        Sheet.AddNewEntry(CalcColName(col) + row);
        Cells.Add(cellCode, entry);
    }

}

