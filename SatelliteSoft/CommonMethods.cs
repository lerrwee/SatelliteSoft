using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SatelliteSoft
{
    public static class CommonMethods
    {
        internal static void DisplayUserInfo(TextBlock loginTextBlock, TextBlock roleTextBlock, string userLogin, string userRole)
        {
            loginTextBlock.Text = userLogin;
            roleTextBlock.Text = userRole;
        }

        public static string TrimAndReplaceDoubleSpaces(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return Regex.Replace(input.Trim(), @"\s+", " ");
        }

        public static void UpdateRecordsCount(TextBlock recordsCountTextBlock, ItemsControl itemsControl)
        {
            int count = itemsControl.Items.Count;
            recordsCountTextBlock.Text = count > 0 ? $"Количество записей: {count}" : "Записи не найдены";
        }

        public static void PreviewMouseWheel1(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = FindVisualParent<ScrollViewer>(sender as DependencyObject);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }

        public static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindVisualParent<T>(parentObject);
        }                
    }
}