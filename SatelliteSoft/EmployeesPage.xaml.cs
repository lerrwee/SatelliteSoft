using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace SatelliteSoft
{
    public partial class EmployeesPage : Page
    {
        private SatelliteSoftDBEntities context;
        private Employees selectedEmployee;
        private string userLogin;
        private string userRole;

        public EmployeesPage(string login, string role)
        {
            InitializeComponent();
            context = new SatelliteSoftDBEntities();
            userLogin = login;
            userRole = role;
            LoadEmployees();

            CommonMethods.DisplayUserInfo(LoginTextBlock, RoleTextBlock, userLogin, userRole);

            if (userRole != "Админ")
            {
                DeleteButton.Visibility = Visibility.Collapsed;
            }
        }
               
        private void LoadEmployees()
        {
            EmployeesListBox.ItemsSource = context.Employees.ToList();
            CommonMethods.UpdateRecordsCount(RecordsCountTextBlock, EmployeesListBox);
        }              

        private void EmployeesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedEmployee = EmployeesListBox.SelectedItem as Employees;
            if (selectedEmployee != null)
            {
                FullNameTextBox.Text = selectedEmployee.name;
                PositionTextBox.Text = selectedEmployee.position;
                ContactInfoTextBox.Text = selectedEmployee.contact_info;
                if (selectedEmployee.photo != null)
                {
                    EmployeePhoto.Source = LoadImage(selectedEmployee.photo);
                }
                else
                {
                    EmployeePhoto.Source = null;
                }
                DeleteButton.Visibility = Visibility.Visible;
            }
            else
            {
                DeleteButton.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsValidInput())
            {
                try
                {
                    if (selectedEmployee == null)
                    {
                        selectedEmployee = new Employees();
                        context.Employees.Add(selectedEmployee);
                    }

                    selectedEmployee.name = CommonMethods.TrimAndReplaceDoubleSpaces(FullNameTextBox.Text);
                    selectedEmployee.position = CommonMethods.TrimAndReplaceDoubleSpaces(PositionTextBox.Text);
                    selectedEmployee.contact_info = CommonMethods.TrimAndReplaceDoubleSpaces(ContactInfoTextBox.Text);

                    if (EmployeePhoto.Source != null)
                    {
                        selectedEmployee.photo = ConvertImageToByteArray(EmployeePhoto.Source as BitmapSource);
                    }

                    context.SaveChanges();
                    LoadEmployees();
                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Произошла ошибка при сохранении данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, заполните все поля корректно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedEmployee != null)
            {
                try
                {
                    context.Employees.Remove(selectedEmployee);
                    context.SaveChanges();
                    LoadEmployees();
                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Произошла ошибка при удалении данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            LoadEmployees();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            FullNameTextBox.Text = string.Empty;
            PositionTextBox.Text = string.Empty;
            ContactInfoTextBox.Text = string.Empty;
            EmployeePhoto.Source = null;
            selectedEmployee = null;
            DeleteButton.Visibility = Visibility.Collapsed;
            EmployeesListBox.SelectedIndex = -1;
        }

        private bool IsValidInput()
        {
            return !string.IsNullOrEmpty(FullNameTextBox.Text) &&
                   !string.IsNullOrEmpty(PositionTextBox.Text) &&
                   !string.IsNullOrEmpty(ContactInfoTextBox.Text) &&
                   IsValidName(FullNameTextBox.Text) &&
                   IsValidPosition(PositionTextBox.Text) &&
                   IsValidPhoneNumber(ContactInfoTextBox.Text);
        }

        private bool IsValidName(string name)
        {
            // Проверка на ввод только букв
            return Regex.IsMatch(name, @"^[a-zA-Zа-яА-Я\s]+$");
        }

        private bool IsValidPosition(string position)
        {
            // Проверка на ввод только букв
            return Regex.IsMatch(position, @"^[a-zA-Zа-яА-Я\s]+$");
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            // Проверка на ввод только цифр и знака "+"
            return Regex.IsMatch(phoneNumber, @"^[+0-9]+$");
        }

        private void ProjectsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ProjectsPage(userLogin, userRole));
        }

        private void EmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EmployeesPage(userLogin, userRole));
        }

        private void CustomersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CustomersPage(userLogin, userRole));
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SearchPage(userLogin, userRole));
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();
            EmployeesListBox.ItemsSource = context.Employees
                .Where(c => c.name.ToLower().Contains(searchText) ||
                            c.position.ToLower().Contains(searchText) ||
                            c.contact_info.ToLower().Contains(searchText))
                .ToList();

            CommonMethods.UpdateRecordsCount(RecordsCountTextBlock, EmployeesListBox);
        }

        private void SelectPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                EmployeePhoto.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
        }

        private byte[] ConvertImageToByteArray(BitmapSource image)
        {
            using (var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(stream);
                return stream.ToArray();
            }
        }

        private BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        private void PreviewMouseWheel1(object sender, MouseWheelEventArgs e)
        {
            CommonMethods.PreviewMouseWheel1(sender, e);
        }
    }
}