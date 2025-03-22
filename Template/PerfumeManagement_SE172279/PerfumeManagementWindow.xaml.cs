using PerfumeRepository.DTOs;
using PerfumeRepository.Models;
using PerfumeRepository.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PerfumeManagement_SE172279
{
    /// <summary>
    /// Interaction logic for PerfumeManagementWindow.xaml
    /// </summary>
    public partial class PerfumeManagementWindow : Window
    {
        private readonly ServiceProvider _serviceProvider;
        private UserDTO _currentUser;
        
        // For tracking if we're in edit mode
        private bool _isEditMode = false;
        
        public PerfumeManagementWindow()
        {
            InitializeComponent();
            _serviceProvider = new ServiceProvider();
            _currentUser = LoginWindow.CurrentUser;
            
            // If no authenticated user, close the window
            if (_currentUser == null || !_currentUser.IsAuthenticated)
            {
                MessageBox.Show("You must be logged in to access this window.", "Authentication Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Display user info
            txtUserInfo.Text = _currentUser.Email;
            txtUserRole.Text = _currentUser.Role == 2 ? "Manager" : "Staff";
            
            // Load data
            LoadPerfumes();
            LoadCompanies();
            
            // Set UI based on user role
            SetUIBasedOnRole();
            
            // Clear form
            ClearForm();
        }
        
        private void SetUIBasedOnRole()
        {
            bool isManager = _currentUser.IsManager;
            
            // Management buttons - only enabled for managers
            btnAdd.IsEnabled = isManager;
            btnUpdate.IsEnabled = isManager;
            btnDelete.IsEnabled = isManager;
            
            // Input fields - read-only for staff
            txtPerfumeId.IsReadOnly = !isManager;
            txtPerfumeName.IsReadOnly = !isManager;
            txtIngredients.IsReadOnly = !isManager;
            txtConcentration.IsReadOnly = !isManager;
            txtLongevity.IsReadOnly = !isManager;
            dpReleaseDate.IsEnabled = isManager;
            cboCompany.IsEnabled = isManager;
        }
        
        private void LoadPerfumes()
        {
            try
            {
                var perfumes = _serviceProvider.PerfumeService.GetAllPerfumes();
                dgPerfumes.ItemsSource = perfumes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading perfumes: {ex.Message}", "Data Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void LoadCompanies()
        {
            try
            {
                var companies = _serviceProvider.ProductionCompanyService.GetAllCompanies();
                cboCompany.ItemsSource = companies;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading companies: {ex.Message}", "Data Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void ClearForm()
        {
            // Clear all form fields
            txtPerfumeId.Text = string.Empty;
            txtPerfumeName.Text = string.Empty;
            txtIngredients.Text = string.Empty;
            txtConcentration.Text = string.Empty;
            txtLongevity.Text = string.Empty;
            dpReleaseDate.SelectedDate = DateTime.Today;
            cboCompany.SelectedIndex = -1;
            txtErrorMsg.Text = string.Empty;
            
            // Reset edit mode
            _isEditMode = false;
            
            // Enable ID field (for new entries)
            txtPerfumeId.IsReadOnly = !_currentUser.IsManager;
        }
        
        private PerfumeDTO GetFormData()
        {
            return new PerfumeDTO
            {
                PerfumeId = txtPerfumeId.Text.Trim(),
                PerfumeName = txtPerfumeName.Text.Trim(),
                Ingredients = txtIngredients.Text.Trim(),
                Concentration = txtConcentration.Text.Trim(),
                Longevity = txtLongevity.Text.Trim(),
                ReleaseDate = dpReleaseDate.SelectedDate,
                ProductionCompanyId = cboCompany.SelectedValue?.ToString(),
                ProductionCompanyName = (cboCompany.SelectedItem as ProductionCompany)?.ProductionCompanyName
            };
        }
        
        private void LoadPerfumeToForm(PerfumeDTO perfume)
        {
            if (perfume == null) return;
            
            txtPerfumeId.Text = perfume.PerfumeId;
            txtPerfumeName.Text = perfume.PerfumeName;
            txtIngredients.Text = perfume.Ingredients;
            txtConcentration.Text = perfume.Concentration;
            txtLongevity.Text = perfume.Longevity;
            dpReleaseDate.SelectedDate = perfume.ReleaseDate;
            
            // Select the correct company in the combobox
            if (!string.IsNullOrEmpty(perfume.ProductionCompanyId))
            {
                foreach (ProductionCompany company in cboCompany.Items)
                {
                    if (company.ProductionCompanyId == perfume.ProductionCompanyId)
                    {
                        cboCompany.SelectedItem = company;
                        break;
                    }
                }
            }
            
            // In edit mode, ID should be read-only
            txtPerfumeId.IsReadOnly = true;
            _isEditMode = true;
        }
        
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string searchTerm = txtSearch.Text.Trim();
                
                if (string.IsNullOrEmpty(searchTerm))
                {
                    LoadPerfumes();
                    return;
                }
                
                // Allow both Manager and Staff to search (previously only Manager could search)
                if (!_currentUser.IsAuthenticated || !(_currentUser.IsManager || _currentUser.IsStaff))
                {
                    MessageBox.Show("You must be logged in as Manager or Staff to perform search operations.", 
                        "Authorization Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Get grouped search results
                var groupedResults = _serviceProvider.PerfumeService.SearchPerfumes(searchTerm);
                
                // Flatten the results for display
                var flatList = new List<PerfumeDTO>();
                foreach (var group in groupedResults)
                {
                    flatList.AddRange(group);
                }
                
                // Create a CollectionViewSource for grouping
                var cvs = new CollectionViewSource();
                cvs.Source = flatList;
                cvs.GroupDescriptions.Add(new PropertyGroupDescription("Ingredients"));
                
                // Set the grouped view as the ItemsSource
                dgPerfumes.ItemsSource = cvs.View;
                
                // Show a message about the results
                txtErrorMsg.Text = $"Found {flatList.Count} perfumes matching '{searchTerm}', grouped by Ingredients.";
                txtErrorMsg.Foreground = System.Windows.Media.Brushes.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching perfumes: {ex.Message}", "Search Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;
            LoadPerfumes();
        }
        
        private void dgPerfumes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgPerfumes.SelectedItem is PerfumeDTO selectedPerfume)
            {
                LoadPerfumeToForm(selectedPerfume);
            }
        }
        
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check authorization
                if (!_serviceProvider.AuthService.IsAuthorizedForCrud(_currentUser))
                {
                    MessageBox.Show("You do not have permission to add perfumes.", "Authorization Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Get form data
                PerfumeDTO perfume = GetFormData();
                
                // Add the perfume
                var result = _serviceProvider.PerfumeService.AddPerfume(perfume);
                
                if (result.success)
                {
                    MessageBox.Show(result.message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadPerfumes();
                    ClearForm();
                }
                else
                {
                    txtErrorMsg.Text = result.message;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding perfume: {ex.Message}", "Add Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check authorization
                if (!_serviceProvider.AuthService.IsAuthorizedForCrud(_currentUser))
                {
                    MessageBox.Show("You do not have permission to update perfumes.", "Authorization Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Check if in edit mode
                if (!_isEditMode)
                {
                    MessageBox.Show("Please select a perfume to update first.", "Update Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Get form data
                PerfumeDTO perfume = GetFormData();
                
                // Update the perfume
                var result = _serviceProvider.PerfumeService.UpdatePerfume(perfume);
                
                if (result.success)
                {
                    MessageBox.Show(result.message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadPerfumes();
                    ClearForm();
                }
                else
                {
                    txtErrorMsg.Text = result.message;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating perfume: {ex.Message}", "Update Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check authorization
                if (!_serviceProvider.AuthService.IsAuthorizedForCrud(_currentUser))
                {
                    MessageBox.Show("You do not have permission to delete perfumes.", "Authorization Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Check if a perfume is selected
                if (string.IsNullOrEmpty(txtPerfumeId.Text))
                {
                    MessageBox.Show("Please select a perfume to delete first.", "Delete Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Confirm deletion
                var result = MessageBox.Show("Are you sure you want to delete this perfume?", "Confirm Delete",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                if (result == MessageBoxResult.Yes)
                {
                    bool success = _serviceProvider.PerfumeService.DeletePerfume(txtPerfumeId.Text);
                    
                    if (success)
                    {
                        MessageBox.Show("Perfume deleted successfully.", "Success", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadPerfumes();
                        ClearForm();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete perfume.", "Delete Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting perfume: {ex.Message}", "Delete Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }
        
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Confirm logout
                var result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                if (result == MessageBoxResult.Yes)
                {
                    // Clear the current user using the static method
                    LoginWindow.Logout();
                    
                    // Open a new login window
                    LoginWindow loginWindow = new LoginWindow();
                    loginWindow.Show();
                    
                    // Close this window
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during logout: {ex.Message}", "Logout Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
