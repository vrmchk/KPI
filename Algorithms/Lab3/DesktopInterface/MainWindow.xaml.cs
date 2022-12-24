using System;
using System.Windows;
using LooseIndexFile;
using LooseIndexFile.Models;

namespace DesktopInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly LooseIndexFile<int> _looseIndexFile =
            new(new LooseIndexFileConfiguration(dataFolder: @"D:\Lab3Data"));

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int value = Convert.ToInt32(AddInput.Text);

                var entry = _looseIndexFile.Add(value);
                MessageBox.Show($"Value added key is: {entry.Key}");
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter a valid integer for the key.");
            }
            catch (ArgumentException exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                AddInput.Clear();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int key = int.Parse(SearchInput.Text);
                var entry = _looseIndexFile.Get(key);
                MessageBox.Show(entry is null ? "No entry found with the given key." : entry.Value.ToString());
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter a valid integer for the key.");
            }
            finally
            {
                SearchInput.Clear();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int key = int.Parse(DeleteInput.Text);
                _looseIndexFile.Delete(key);
                MessageBox.Show("Deleted successfully");
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter a valid integer for the key.");
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Entry with this key doesn't exist");
            }
            finally
            {
                DeleteInput.Clear();
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int key = int.Parse(UpdateKeyInput.Text);
                int value = int.Parse(UpdateValueInput.Text);
                _looseIndexFile.Update(key, value);
                MessageBox.Show("Updated successfully");
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter a valid integer for the key.");
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Entry with this key doesn't exist");
            }
            finally
            {
                UpdateKeyInput.Clear();
                UpdateValueInput.Clear();
            }
        }
    }
}