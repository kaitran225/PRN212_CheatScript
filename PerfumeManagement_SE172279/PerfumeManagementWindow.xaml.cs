using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using PerfumeManagement_SE172279_DAL.DTO;
using PerfumeManagement_SE172279_DAL.Services;
using PerfumeManagement_SE172279_BLL.Models;

namespace PerfumeManagement_SE172279
{
    public partial class PerfumeManagementWindow : Window
    {
        private readonly PerfumeInformationService _perfumeInformationService;
        private readonly ProductionCompanyService  _productService;
        private readonly PsaccountService _psaccountService;

        private readonly UserDTO? _currentUser;
        
        private bool _isEditMode = false;
        
        public PerfumeManagementWindow()
        {
            _perfumeInformationService = new PerfumeInformationService();
            _productService = new ProductionCompanyService();
            _psaccountService = new PsaccountService();
            _currentUser = LoginWindow.CurrentUser!;

            InitializeComponent();
            
            if (_currentUser == null || !_currentUser.IsAuthenticated)
            {
                MessageBox.Show("You must be logged in to access this window.", "Authentication Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtUserInfo.Text = _currentUser!.Email;
            txtUserRole.Text = _currentUser.Role == 2 ? "Manager" : "Staff";
            
            LoadPerfumes();
            LoadCompanies();
            SetUIBasedOnRole();
            ClearForm();
        }
        
        private void SetUIBasedOnRole()
        {
            bool isManager = _currentUser!.IsManager;
            
            btnAdd.IsEnabled = isManager;
            btnUpdate.IsEnabled = isManager;
            btnDelete.IsEnabled = isManager;
            
            // Read-only for staff
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
                var perfumes = _perfumeInformationService.GetAllPerfumes();
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
                var companies = _productService.GetAllCompanies()!;
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
            txtPerfumeId.Text = string.Empty;
            txtPerfumeName.Text = string.Empty;
            txtIngredients.Text = string.Empty;
            txtConcentration.Text = string.Empty;
            txtLongevity.Text = string.Empty;
            dpReleaseDate.SelectedDate = DateTime.Today;
            cboCompany.SelectedIndex = -1;
            txtErrorMsg.Text = string.Empty;
            
            _isEditMode = false;
            
            txtPerfumeId.IsReadOnly = !_currentUser!.IsManager;
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
                
                if (!_currentUser!.IsAuthenticated || !(_currentUser.IsManager || _currentUser.IsStaff))
                {
                    MessageBox.Show("You must be logged in as Manager or Staff to perform search operations.", 
                        "Authorization Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                var groupedResults = _perfumeInformationService.SearchPerfumes(searchTerm);
                
                var flatList = new List<PerfumeDTO>();
                foreach (var group in groupedResults!)
                {
                    flatList.AddRange(group);
                }
                
                var cvs = new CollectionViewSource();
                cvs.Source = flatList;
                cvs.GroupDescriptions.Add(new PropertyGroupDescription("Ingredients"));
                
                dgPerfumes.ItemsSource = cvs.View;
                
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
                if (!_psaccountService.IsAuthorizedForCrud(_currentUser!))
                {
                    MessageBox.Show("You do not have permission to add perfumes.", "Authorization Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                PerfumeDTO perfume = GetFormData();
                
                var result = _perfumeInformationService.AddPerfume(perfume);
                
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
                if (!_psaccountService.IsAuthorizedForCrud(_currentUser!))
                {
                    MessageBox.Show("You do not have permission to update perfumes.", "Authorization Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                if (!_isEditMode)
                {
                    MessageBox.Show("Please select a perfume to update first.", "Update Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                PerfumeDTO perfume = GetFormData();
                
                var result = _perfumeInformationService.UpdatePerfume(perfume);
                
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
                if (!_psaccountService.IsAuthorizedForCrud(_currentUser!))
                {
                    MessageBox.Show("You do not have permission to delete perfumes.", "Authorization Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                if (string.IsNullOrEmpty(txtPerfumeId.Text))
                {
                    MessageBox.Show("Please select a perfume to delete first.", "Delete Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                var result = MessageBox.Show("Are you sure you want to delete this perfume?", "Confirm Delete",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                if (result == MessageBoxResult.Yes)
                {
                    bool success = _perfumeInformationService.DeletePerfume(txtPerfumeId.Text);
                    
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
                var result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                if (result == MessageBoxResult.Yes)
                {
                    LoginWindow.Logout();
                    
                    LoginWindow loginWindow = new();
                    loginWindow.Show();
                    
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
