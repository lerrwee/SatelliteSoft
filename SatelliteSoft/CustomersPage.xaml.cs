using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SatelliteSoft
{
    public partial class CustomersPage : Page
    {
        private SatelliteSoftDBEntities context;
        private Customers selectedCustomer;
        private string userLogin;
        private string userRole;

        public CustomersPage(string login, string role)
        {
            InitializeComponent();
            context = new SatelliteSoftDBEntities();
            userLogin = login;
            userRole = role;
            LoadCustomers();

            CommonMethods.DisplayUserInfo(LoginTextBlock, RoleTextBlock, userLogin, userRole);

            if (userRole != "Админ")
            {
                DeleteButton.Visibility = Visibility.Collapsed;
            }
        }
             
        private void LoadCustomers()
        {
            CustomersListBox.ItemsSource = context.Customers.ToList();
            CommonMethods.UpdateRecordsCount(RecordsCountTextBlock, CustomersListBox);
        }
               

        private void CustomersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedCustomer = CustomersListBox.SelectedItem as Customers;
            if (selectedCustomer != null)
            {
                CompanyNameTextBox.Text = selectedCustomer.name;
                RepresentativeNameTextBox.Text = selectedCustomer.contact_person;
                PhoneNumberTextBox.Text = selectedCustomer.contact_info;
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
                    if (selectedCustomer == null)
                    {
                        selectedCustomer = new Customers();
                        context.Customers.Add(selectedCustomer);
                    }

                    selectedCustomer.name = CommonMethods.TrimAndReplaceDoubleSpaces(CompanyNameTextBox.Text);
                    selectedCustomer.contact_person = CommonMethods.TrimAndReplaceDoubleSpaces(RepresentativeNameTextBox.Text);
                    selectedCustomer.contact_info = CommonMethods.TrimAndReplaceDoubleSpaces(PhoneNumberTextBox.Text);

                    context.SaveChanges();
                    LoadCustomers();
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
            if (selectedCustomer != null)
            {
                try
                {
                    context.Customers.Remove(selectedCustomer);
                    context.SaveChanges();
                    LoadCustomers();
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
            LoadCustomers();
        }
                

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            CompanyNameTextBox.Text = string.Empty;
            RepresentativeNameTextBox.Text = string.Empty;
            PhoneNumberTextBox.Text = string.Empty;
            selectedCustomer = null;
            DeleteButton.Visibility = Visibility.Collapsed;
            CustomersListBox.SelectedIndex = -1;
        }

        private bool IsValidInput()
        {
            return !string.IsNullOrEmpty(CompanyNameTextBox.Text) &&
                   !string.IsNullOrEmpty(RepresentativeNameTextBox.Text) &&
                   !string.IsNullOrEmpty(PhoneNumberTextBox.Text) &&
                   IsValidName(CompanyNameTextBox.Text) &&
                   IsValidName(RepresentativeNameTextBox.Text) &&
                   IsValidPhoneNumber(PhoneNumberTextBox.Text);
        }

        private bool IsValidName(string name)
        {
            // Проверка на ввод только букв
            return Regex.IsMatch(name, @"^[a-zA-Zа-яА-Я\s]+$");
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
            CustomersListBox.ItemsSource = context.Customers
                .Where(c => c.name.ToLower().Contains(searchText) ||
                            c.contact_person.ToLower().Contains(searchText) ||
                            c.contact_info.ToLower().Contains(searchText))
                .ToList();

            CommonMethods.UpdateRecordsCount(RecordsCountTextBlock, CustomersListBox);
        }

        private void PreviewMouseWheel1(object sender, MouseWheelEventArgs e)
        {
            CommonMethods.PreviewMouseWheel1(sender, e);
        }
    }
}