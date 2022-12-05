using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Labyrinth.Enums;
using Newtonsoft.Json;

namespace LabyrinthMaker;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private int _size = 10;
    private Brush _selectedBrush = Brushes.Black;
    private MazeGenerator _mazeGenerator;
    public int Size
    {
        get => _size;
        set {
            if (value % 2 == 0)
                _size = value + (_size > value ? -1 : 1);
            else
                _size = value;
            Grid grid = new Grid();
            for (int i = 0; i < Size; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
            }
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    Border border = new Border {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0.5)
                    };

                    Grid cell = new Grid {
                        Background = Brushes.White
                    };

                    cell.MouseEnter += Cell_MouseEnter;
                    border.Child = cell;
                    Grid.SetColumn(border, i);
                    Grid.SetRow(border, j);
                    grid.Children.Add(border);
                }
            }
            grid.Height = 500;
            grid.Width = 500;
            Main_Grid.Children.Clear();
            Main_Grid.Children.Add(grid);
            _mazeGenerator?.GenerateMaze(Size);
        }
    }
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        Size_IUP.Value = 11;
        Size = 11;
        _mazeGenerator = new MazeGenerator((cells) => {
            for (int rowIndex = 0; rowIndex < cells.GetLength(0); rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < cells.GetLength(1); columnIndex++)
                {
                    IEnumerable<Border> borders = ((Grid)Main_Grid.Children[0]).Children.Cast<Border>();

                    if (borders.First(b => Grid.GetRow(b) == rowIndex
                        && Grid.GetColumn(b) == columnIndex).Child is not Grid curr)
                    {
                        continue;
                    }
                    if (rowIndex == 1 && columnIndex == 1)
                        curr.Background = Brushes.Green;
                    else if (rowIndex == Size - 2 && columnIndex == Size - 2)
                        curr.Background = Brushes.Red;
                    else
                        curr.Background = cells[rowIndex, columnIndex] ? Brushes.White : Brushes.Black;
                }
            }
        });
        //for (int i = 1; i <= 20; i++)
        //{
        //    Size = new Random().Next(13, 22);
        //    Thread.Sleep(10);
        //    _mazeGenerator.GenerateMaze(Size);
        //    SaveToFile($"States\\state{i}.json");
        //}
    }

    private void Cell_MouseEnter(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            if (_selectedBrush == Brushes.Red || _selectedBrush == Brushes.Green)
            {
                List<Grid> cells = ((Grid)Main_Grid.Children[0]).Children.Cast<Border>().Select(b => b.Child).Cast<Grid>().ToList();
                Grid? lastPrinted = cells.FirstOrDefault(g => g.Background == _selectedBrush);
                if (lastPrinted != null)
                    lastPrinted.Background = Brushes.White;
            }
            ((Grid)sender).Background = _selectedBrush;
        }
    }

    private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _selectedBrush = ((Grid)sender).Background;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        SaveToFile("result.json");
        MessageBox.Show("Saved successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void SaveToFile(string path)
    {
        CompressedCell[,] cells = new CompressedCell[Size, Size];
        for (int rowIndex = 0; rowIndex < Size; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < Size; columnIndex++)
            {
                CellType state = CellType.Empty;
                if (((Grid)Main_Grid.Children[0]).Children.Cast<Border>().First(b => Grid.GetRow(b) == rowIndex
                    && Grid.GetColumn(b) == columnIndex).Child is not Grid curr)
                {
                    continue;
                }

                if (curr.Background == Brushes.Black)
                    state = CellType.Wall;
                else if (curr.Background == Brushes.Red)
                    state = CellType.Destination;
                else if (curr.Background == Brushes.Green)
                    state = CellType.Source;

                cells[rowIndex, columnIndex] = new CompressedCell((columnIndex, rowIndex), state);
            }
        }
        string result = JsonConvert.SerializeObject(cells, Formatting.Indented);
        File.WriteAllText(path, result);
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        string json = File.ReadAllText("result.json");
        CompressedCell[,]? cells = JsonConvert.DeserializeObject<CompressedCell[,]>(json);
        if (cells == null)
        {
            MessageBox.Show("Error");
            return;
        }
        Size = cells.GetLength(0);
        for (int rowIndex = 0; rowIndex < cells.GetLength(0); rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < cells.GetLength(1); columnIndex++)
            {
                IEnumerable<Border> borders = ((Grid)Main_Grid.Children[0]).Children.Cast<Border>();
                if (borders.First(b => Grid.GetRow(b) == rowIndex
                    && Grid.GetColumn(b) == columnIndex).Child is not Grid curr)
                {
                    continue;
                }

                curr.Background = cells[rowIndex, columnIndex].Type switch {
                    CellType.Empty or CellType.Visited or CellType.Selected => Brushes.White,
                    CellType.Source => Brushes.Green,
                    CellType.Destination => Brushes.Red,
                    CellType.Wall => Brushes.Black,
                    _ => throw new System.NotImplementedException()
                };
            }
        }
    }
    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        _mazeGenerator.GenerateMaze(Size);
    }
}
