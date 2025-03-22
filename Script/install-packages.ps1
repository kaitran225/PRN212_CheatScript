# Prompt the user to enter the solution directory path
$solutionDir = Read-Host "Enter the full path of the solution directory"

# Check if the directory exists
if (!(Test-Path $solutionDir)) {
    Write-Host "Error: The specified directory does not exist." -ForegroundColor Red
    exit
}

# Define the packages and versions
$packages = @(
    "Microsoft.EntityFrameworkCore.SqlServer -Version 8.0.2",
    "Microsoft.EntityFrameworkCore.Tools -Version 8.0.2",
    "Microsoft.EntityFrameworkCore.Design -Version 8.0.2",
    "Microsoft.Extensions.Configuration -Version 8.0.2",
    "Microsoft.Extensions.Configuration.Json -Version 8.0.2"
)

# Get all .csproj files in the specified solution directory
$projects = Get-ChildItem -Path $solutionDir -Recurse -Filter *.csproj

# Check if any .csproj files were found
if ($projects.Count -eq 0) {
    Write-Host "No .csproj files found in the specified directory." -ForegroundColor Yellow
    exit
}

# Loop through each project and install the packages
foreach ($project in $projects) {
    Write-Host "Installing packages for project: $($project.FullName)" -ForegroundColor Cyan

    foreach ($package in $packages) {
        Write-Host "Installing package: $package" -ForegroundColor Green
        dotnet add $project.FullName package $package
    }
}

Write-Host "All packages installed for all projects!" -ForegroundColor Magenta 