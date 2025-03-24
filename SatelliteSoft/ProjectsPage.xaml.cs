using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SatelliteSoft
{
    public partial class ProjectsPage : Page
    {
        private SatelliteSoftDBEntities context;
        private Projects selectedProject;
        private string userLogin;
        private string userRole;
        private ObservableCollection<Employees> availableEmployees;
        private ObservableCollection<Employees> selectedEmployees;
        private HashSet<Employees> uniqueAvailableEmployees;
        private HashSet<Employees> uniqueSelectedEmployees;

        public ProjectsPage(string login, string role)
        {
            InitializeComponent();
            context = new SatelliteSoftDBEntities();
            userLogin = login;
            userRole = role;
            availableEmployees = new ObservableCollection<Employees>();
            selectedEmployees = new ObservableCollection<Employees>();
            uniqueAvailableEmployees = new HashSet<Employees>();
            uniqueSelectedEmployees = new HashSet<Employees>();
            LoadProjects();
            LoadEmployees();
            LoadCustomers();

            CommonMethods.DisplayUserInfo(LoginTextBlock, RoleTextBlock, userLogin, userRole);

            if (userRole != "Админ")
            {
                DeleteButton.Visibility = Visibility.Collapsed;
            }
        }
                
        private void LoadProjects()
        {
            ProjectsListBox.ItemsSource = context.Projects.ToList();

            CommonMethods.UpdateRecordsCount(RecordsCountTextBlock, ProjectsListBox);
        }

        private void LoadEmployees()
        {
            var allEmployees = context.Employees.ToList();
            foreach (var employee in allEmployees)
            {
                if (uniqueAvailableEmployees.Add(employee))
                {
                    availableEmployees.Add(employee);
                }
            }
            AvailableEmployeesListBox.ItemsSource = availableEmployees;
            SelectedEmployeesListBox.ItemsSource = selectedEmployees;
        }

        private void LoadCustomers()
        {
            CustomerComboBox.ItemsSource = context.Customers.ToList();
            CustomerComboBox.DisplayMemberPath = "name";
            CustomerComboBox.SelectedValuePath = "id";
        }               

        private void ProjectsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedProject = ProjectsListBox.SelectedItem as Projects;
            if (selectedProject != null)
            {
                ProjectNameTextBox.Text = selectedProject.name;
                DescriptionTextBox.Text = selectedProject.description;
                BudgetTextBox.Text = selectedProject.budget.ToString();
                TechnologiesTextBox.Text = selectedProject.technologies;
                StartDatePicker.SelectedDate = selectedProject.start_date;
                EndDatePicker.SelectedDate = selectedProject.end_date;
                CustomerComboBox.SelectedValue = selectedProject.customer_id;

                selectedEmployees.Clear();
                uniqueSelectedEmployees.Clear();
                foreach (var employee in selectedProject.Employees)
                {
                    if (uniqueSelectedEmployees.Add(employee))
                    {
                        selectedEmployees.Add(employee);
                    }
                }

                availableEmployees.Clear();
                uniqueAvailableEmployees.Clear();
                var selectedEmployeeIds = selectedProject.Employees.Select(emp => emp.id).ToList();
                var availableEmployeesList = context.Employees.Where(emp => !selectedEmployeeIds.Contains(emp.id)).ToList();
                foreach (var employee in availableEmployeesList)
                {
                    if (uniqueAvailableEmployees.Add(employee))
                    {
                        availableEmployees.Add(employee);
                    }
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
                if (selectedProject == null)
                {
                    selectedProject = new Projects();
                    context.Projects.Add(selectedProject);
                }

                selectedProject.name = CommonMethods.TrimAndReplaceDoubleSpaces(ProjectNameTextBox.Text);
                selectedProject.description = CommonMethods.TrimAndReplaceDoubleSpaces(DescriptionTextBox.Text);
                selectedProject.budget = int.Parse(BudgetTextBox.Text);
                selectedProject.technologies = CommonMethods.TrimAndReplaceDoubleSpaces(TechnologiesTextBox.Text);
                selectedProject.start_date = StartDatePicker.SelectedDate.Value;
                selectedProject.end_date = EndDatePicker.SelectedDate.Value;
                selectedProject.customer_id = (int)CustomerComboBox.SelectedValue;

                selectedProject.Employees.Clear();
                foreach (var employee in selectedEmployees)
                {
                    selectedProject.Employees.Add(employee);
                }

                context.SaveChanges();
                LoadProjects();
                ClearFields();
            }
            else
            {
                MessageBox.Show("Пожалуйста, заполните все поля корректно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProject != null)
            {
                try
                {
                    context.Projects.Remove(selectedProject);
                    context.SaveChanges();
                    LoadProjects();
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
            LoadProjects();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            ProjectNameTextBox.Text = string.Empty;
            DescriptionTextBox.Text = string.Empty;
            BudgetTextBox.Text = string.Empty;
            TechnologiesTextBox.Text = string.Empty;
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
            CustomerComboBox.SelectedIndex = -1;
            selectedEmployees.Clear();
            uniqueSelectedEmployees.Clear();
            availableEmployees.Clear();
            uniqueAvailableEmployees.Clear();
            var allEmployees = context.Employees.ToList();
            foreach (var employee in allEmployees)
            {
                if (uniqueAvailableEmployees.Add(employee))
                {
                    availableEmployees.Add(employee);
                }
            }
            selectedProject = null;
            DeleteButton.Visibility = Visibility.Collapsed;
            ProjectsListBox.SelectedIndex = -1;
        }

        private bool IsValidInput()
        {
            return !string.IsNullOrEmpty(ProjectNameTextBox.Text) &&
                   !string.IsNullOrEmpty(DescriptionTextBox.Text) &&
                   !string.IsNullOrEmpty(BudgetTextBox.Text) &&
                   !string.IsNullOrEmpty(TechnologiesTextBox.Text) &&
                   StartDatePicker.SelectedDate != null &&
                   EndDatePicker.SelectedDate != null &&
                   CustomerComboBox.SelectedValue != null &&
                   IsValidBudget(BudgetTextBox.Text);
        }

        private bool IsValidBudget(string budget)
        {
            return Regex.IsMatch(budget, @"^\d+$");
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
            ProjectsListBox.ItemsSource = context.Projects
                .Where(c => c.name.ToLower().Contains(searchText) ||
                            c.description.ToLower().Contains(searchText) ||
                            c.technologies.ToLower().Contains(searchText))
                .ToList();

            CommonMethods.UpdateRecordsCount(RecordsCountTextBlock, ProjectsListBox);
        }

        private void AddEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = AvailableEmployeesListBox.SelectedItems.Cast<Employees>().ToList();
            foreach (var item in selectedItems)
            {
                if (uniqueAvailableEmployees.Remove(item))
                {
                    availableEmployees.Remove(item);
                }
                if (uniqueSelectedEmployees.Add(item))
                {
                    selectedEmployees.Add(item);
                }
            }
        }

        private void RemoveEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = SelectedEmployeesListBox.SelectedItems.Cast<Employees>().ToList();
            foreach (var item in selectedItems)
            {
                if (uniqueSelectedEmployees.Remove(item))
                {
                    selectedEmployees.Remove(item);
                }
                if (uniqueAvailableEmployees.Add(item))
                {
                    availableEmployees.Add(item);
                }
            }
        }

        private void PreviewMouseWheel1(object sender, MouseWheelEventArgs e)
        {
            CommonMethods.PreviewMouseWheel1(sender, e);
        }
    }
}