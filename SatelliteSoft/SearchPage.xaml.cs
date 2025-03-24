using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SatelliteSoft
{
    public partial class SearchPage : Page
    {
        private SatelliteSoftDBEntities context;
        private string userLogin;
        private string userRole;

        public SearchPage(string login, string role)
        {
            InitializeComponent();
            context = new SatelliteSoftDBEntities();
            userLogin = login;
            userRole = role;
            LoadSearchResults();

            CommonMethods.DisplayUserInfo(LoginTextBlock, RoleTextBlock, userLogin, userRole);
        }

        private void LoadSearchResults()
        {
            var projects = context.Projects.ToList();
            var results = projects.Select(p => new
            {
                Name = p.name,
                Description = p.description,
                Budget = p.budget,
                Technologies = p.technologies,
                Dates = $"{p.start_date:d} - {p.end_date:d}",
                Employee = string.Join(", ", p.Employees.Select(e => e.name)),
                Customer = p.Customers.name
            }).ToList();

            SearchResultsDataGrid.ItemsSource = results;
            CommonMethods.UpdateRecordsCount(RecordsCountTextBlock, SearchResultsDataGrid);
        }                

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            LoadSearchResults();
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
            var projects = context.Projects.ToList();

            int budget;
            bool isBudgetSearch = int.TryParse(searchText, out budget);

            DateTime date;
            bool isDateSearch = DateTime.TryParseExact(searchText, "d.M.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

            var results = projects
                .Where(p => p.name.ToLower().Contains(searchText) ||
                            p.description.ToLower().Contains(searchText) ||
                            p.technologies.ToLower().Contains(searchText) ||
                            p.Employees.Any(emp => emp.name.ToLower().Contains(searchText)) ||
                            p.Customers.name.ToLower().Contains(searchText) ||
                            (isBudgetSearch && p.budget == budget) ||
                            (isDateSearch && (p.start_date.Date == date.Date || p.end_date.Date == date.Date)))
                .Select(p => new
                {
                    Name = p.name,
                    Description = p.description,
                    Budget = p.budget,
                    Technologies = p.technologies,
                    Dates = $"{p.start_date:d} - {p.end_date:d}",
                    Employee = string.Join(", ", p.Employees.Select(ex => ex.name)),
                    Customer = p.Customers.name
                })
                .ToList();

            SearchResultsDataGrid.ItemsSource = results;
            CommonMethods.UpdateRecordsCount(RecordsCountTextBlock, SearchResultsDataGrid);
        }

        private void PreviewMouseWheel1(object sender, MouseWheelEventArgs e)
        {
            CommonMethods.PreviewMouseWheel1(sender, e);
        }
    }
}