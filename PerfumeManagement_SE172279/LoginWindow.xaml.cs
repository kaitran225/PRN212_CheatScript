using System;
using PerfumeManagement_SE172279_DAL.Services;
using PerfumeManagement_SE172279_DAL.DTO;
using System.Windows;

namespace PerfumeManagement_SE172279
{

    public partial class LoginWindow : Window
    {
        private readonly PsaccountService _psaccountService;
        public static UserDTO? CurrentUser { get; private set; }
        
        public static void Logout()
        {
            CurrentUser = null;
        }
        
        public LoginWindow()
        {
            InitializeComponent();
            _psaccountService = new PsaccountService();
            
            // Set focus to email field
            Loaded += (s, e) => txtEmail.Focus();
            
            // For testing - pre-fill credentials
            #if DEBUG
            txtEmail.Text = "manager@PerfumeStore.net";
            txtPassword.Password = "@5";
            #endif
        }
        
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clear any previous error messages
                txtErrorMessage.Text = string.Empty;
                
                // Get input values
                string email = txtEmail.Text.Trim();
                string password = txtPassword.Password.Trim();
                
                // Validate input
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    txtErrorMessage.Text = "Please enter both email and password.";
                    return;
                }
                
                // Show login attempt details (for debugging)
                txtErrorMessage.Text = $"Attempting login with: {email} / {password}";
                
                // Authenticate user
                UserDTO user = _psaccountService.Login(email, password);
                
                if (user.IsAuthenticated)
                {
                    // Store authenticated user
                    CurrentUser = user;
                    
                    // Open the main management window
                    PerfumeManagementWindow managementWindow = new();
                    managementWindow.Show();
                    
                    // Close the login window
                    this.Close();
                }
                else
                {
                    txtErrorMessage.Text = "You have no permission to access this function!";
                }
            }
            catch (Exception ex)
            {
                txtErrorMessage.Text = $"Login error: {ex.Message}";
            }
        }
    }
} 