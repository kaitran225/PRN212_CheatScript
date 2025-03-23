# Prompt user for base information
Write-Host "Please provide the following information:" -ForegroundColor Cyan
Write-Host "----------------------------------------"

# Get solution directory and database name
$solutionPath = (Read-Host "Enter the directory path of the existing solution").Trim()
$databaseName = (Read-Host "Enter the database name (e.g., Sp25PerfumeStoreDB)").Trim()

# Extract solution name from path
$solutionName = (Get-Item $solutionPath).Name

# Ask for project structure
Write-Host "`nChoose project structure:" -ForegroundColor Yellow
Write-Host "1. Two projects (Repository and Service in same project)"
Write-Host "2. Three projects (Separate Repository, Service, and WPF projects)"
$structureChoice = Read-Host "Enter your choice (1 or 2)"

# Ask if user wants to input custom project names
Write-Host "`nDo you want to input custom project names?" -ForegroundColor Yellow
Write-Host "1. Use default names (based on solution name)"
Write-Host "2. Input custom names"
$nameChoice = Read-Host "Enter your choice (1 or 2)"

if ($nameChoice -eq "2") {
    if ($structureChoice -eq "1") {
        $wpfAppName = (Read-Host "Enter WPF project name").Trim()
        $libraryName = (Read-Host "Enter Library project name").Trim()
    } else {
        $wpfAppName = (Read-Host "Enter WPF project name").Trim()
        $serviceName = (Read-Host "Enter Service project name").Trim()
        $repoName = (Read-Host "Enter Repository project name").Trim()
    }
} else {
    if ($structureChoice -eq "1") {
        $wpfAppName = "$solutionName.App"
        $libraryName = "$solutionName.Library"
    } else {
        $wpfAppName = "$solutionName.App"
        $serviceName = "$solutionName.Service"
        $repoName = "$solutionName.Repository"
    }
}

# Validate solution exists
$solutionFile = Join-Path $solutionPath "$solutionName.sln"
if (-not (Test-Path $solutionFile)) {
    Write-Host "Error: Solution file not found: $solutionFile" -ForegroundColor Red
    exit
}

# Change to the solution directory
Set-Location -Path $solutionPath

if ($structureChoice -eq "1") {
    # Two-project structure
    Write-Host "`nCreating projects..." -ForegroundColor Green
    Write-Host "Creating WPF Application: $wpfAppName"
    dotnet new wpf -n $wpfAppName
    
    Write-Host "Creating Library Project: $libraryName"
    dotnet new classlib -n $libraryName
    
    # Add projects to solution
    Write-Host "`nAdding projects to solution..." -ForegroundColor Yellow
    dotnet sln add "$wpfAppName\$wpfAppName.csproj"
    dotnet sln add "$libraryName\$libraryName.csproj"
    
    # Add reference from WPF to Library
    Write-Host "Adding project reference..." -ForegroundColor Yellow
    dotnet add "$wpfAppName\$wpfAppName.csproj" reference "$libraryName\$libraryName.csproj"
    
    Write-Host "`nTwo-project solution created successfully!" -ForegroundColor Green
} else {
    # Three-project structure
    Write-Host "`nCreating projects..." -ForegroundColor Green
    Write-Host "Creating WPF Application: $wpfAppName"
    dotnet new wpf -n $wpfAppName
    
    Write-Host "Creating Service Project: $serviceName"
    dotnet new classlib -n $serviceName
    
    Write-Host "Creating Repository Project: $repoName"
    dotnet new classlib -n $repoName
    
    # Add projects to solution
    Write-Host "`nAdding projects to solution..." -ForegroundColor Yellow
    dotnet sln add "$wpfAppName\$wpfAppName.csproj"
    dotnet sln add "$serviceName\$serviceName.csproj"
    dotnet sln add "$repoName\$repoName.csproj"
    
    # Add project references
    Write-Host "Adding project references..." -ForegroundColor Yellow
    dotnet add "$wpfAppName\$wpfAppName.csproj" reference "$serviceName\$serviceName.csproj"
    dotnet add "$serviceName\$serviceName.csproj" reference "$repoName\$repoName.csproj"
    
    Write-Host "`nThree-project solution created successfully!" -ForegroundColor Green
}

# Create appsettings.json in WPF project
Write-Host "`nCreating appsettings.json..." -ForegroundColor Green
$appSettingsContent = @"
{
  "ConnectionStrings": {
    "PerfumeDb": "Server=KAINOTE\\SQLEXPRESS;Database=$databaseName;User Id=sa;Password=123456;TrustServerCertificate=True;"
  }
}
"@

# Create appsettings.json in WPF project
$appSettingsPath = Join-Path $wpfAppName "appsettings.json"
Set-Content -Path $appSettingsPath -Value $appSettingsContent

# Modify WPF project file to set appsettings.json properties
$wpfProjectFile = Join-Path $wpfAppName "$wpfAppName.csproj"
$projectContent = Get-Content $wpfProjectFile -Raw
$projectContent = $projectContent.Replace("</Project>", @"
  <ItemGroup>
    <None Update="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
</Project>
"@)
Set-Content -Path $wpfProjectFile -Value $projectContent

# Define SQL Server credentials and connection settings
$serverName = "KAINOTE\SQLEXPRESS"    # SQL Server instance name
$userId = "sa"                        # SQL Server username
$password = "123456"                  # SQL Server password

# Build the connection string dynamically
$connectionString = "Server=$serverName;Database=$databaseName;User Id=$userId;Password=$password;TrustServerCertificate=True;"

# Generate context name
$contextName = $databaseName + "Context"

# Show summary of what will be done
Write-Host "`nSummary of operations to be performed:" -ForegroundColor Yellow
Write-Host "----------------------------------------"
Write-Host "1. Install required packages"
Write-Host "2. Generate database context"
Write-Host "3. Generate repository and service layers"
Write-Host "----------------------------------------"

# Ask for confirmation
$confirmation = Read-Host "`nDo you want to proceed with these operations? (Y/N)"
if ($confirmation -ne 'Y' -and $confirmation -ne 'y') {
    Write-Host "Operation cancelled by user." -ForegroundColor Yellow
    exit
}

# Start automated process
Write-Host "`nStarting automated process..." -ForegroundColor Green

# Install packages
Write-Host "`nInstalling required packages..." -ForegroundColor Cyan

# Define the packages and versions
$packages = @(
    "Microsoft.EntityFrameworkCore.SqlServer",
    "Microsoft.EntityFrameworkCore.Tools",
    "Microsoft.EntityFrameworkCore.Design",
    "Microsoft.Extensions.Configuration",
    "Microsoft.Extensions.Configuration.Json"
)

# Install packages for each project
Write-Host "Installing packages for Repository project..." -ForegroundColor Green
Push-Location $repoName
foreach ($package in $packages) {
    Write-Host "Installing package: $package" -ForegroundColor Yellow
    dotnet add package $package --version 8.0.2
}
Pop-Location

Write-Host "`nInstalling packages for Service project..." -ForegroundColor Green
Push-Location $serviceName
foreach ($package in $packages) {
    Write-Host "Installing package: $package" -ForegroundColor Yellow
    dotnet add package $package --version 8.0.2
}
Pop-Location

Write-Host "`nInstalling packages for WPF project..." -ForegroundColor Green
Push-Location $wpfAppName
foreach ($package in $packages) {
    Write-Host "Installing package: $package" -ForegroundColor Yellow
    dotnet add package $package --version 8.0.2
}
Pop-Location

Write-Host "`nPackage installation completed!" -ForegroundColor Green

# Generate database context
Write-Host "`nGenerating database context..." -ForegroundColor Cyan
$modelsPath = Join-Path $repoName "Models"
if (!(Test-Path $modelsPath)) {
    New-Item -ItemType Directory -Path $modelsPath -Force
}

# Execute the Scaffold-DbContext command using dotnet ef
Write-Host "Running Scaffold-DbContext for database: $databaseName" -ForegroundColor Cyan

# Build the scaffold command with proper parameters
$command = "dotnet ef dbcontext scaffold `"$connectionString`" " + `
    "Microsoft.EntityFrameworkCore.SqlServer " + `
    "--output-dir Models " + `
    "--context $contextName " + `
    "--project `"$repoName\$repoName.csproj`" " + `
    "--startup-project `"$wpfAppName\$wpfAppName.csproj`" " + `
    "--force"

Write-Host "Executing command: $command" -ForegroundColor Yellow

# Change to the solution directory before running the command
Push-Location $solutionPath
try {
    Invoke-Expression $command
    Write-Host "Scaffolding completed successfully!" -ForegroundColor Green
} catch {
    Write-Host "Error during scaffolding: $_" -ForegroundColor Red
    Pop-Location
    exit
}
Pop-Location

# Continue with repository and service generation
Write-Host "`nStarting repository and service generation..." -ForegroundColor Cyan

# Define paths for Models, Repositories, and IRepositories
$repositoriesPath = "$repoName\Repositories"
$iRepositoriesPath = "$repoName\IRepositories"
$outputServicePath = "$serviceName\Services"
$iServicePath = "$serviceName\IServices"

# Ensure output directories exist
if (!(Test-Path $repositoriesPath)) {
    New-Item -ItemType Directory -Path $repositoriesPath
}
if (!(Test-Path $iRepositoriesPath)) {
    New-Item -ItemType Directory -Path $iRepositoriesPath
}
if (!(Test-Path $outputServicePath)) {
    New-Item -ItemType Directory -Path $outputServicePath -Force
}
if (!(Test-Path $iServicePath)) {
    New-Item -ItemType Directory -Path $iServicePath -Force
}

# Find the DbContext file
$contextFile = Get-ChildItem -Path $modelsPath -Filter "*.cs" | Where-Object { $_.Name -match "Context\.cs$" }

if (-not $contextFile) {
    Write-Output "No DbContext file found! Exiting..."
    exit
}

# Read DbContext file content
$contextContent = Get-Content -Path $contextFile.FullName

# Extract the actual DbContext class name
$contextClassMatch = $contextContent | Select-String -Pattern "public partial class (\w+) : DbContext"
$contextClassName = $contextClassMatch.Matches.Groups[1].Value

if (-not $contextClassName) {
    Write-Output "No valid DbContext class found! Exiting..."
    exit
}

# Extract entity names from DbSet<> properties in the DbContext file
$entityMatches = $contextContent | Select-String -Pattern "public virtual DbSet<(\w+)>" -AllMatches
$entities = $entityMatches.Matches | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique

if (-not $entities) {
    Write-Output "No DbSet<> entities found in the DbContext! Exiting..."
    exit
}

Write-Output "Generating base repository and unit of work classes..."

# Generate IRepositoryBase interface
$iRepositoryBaseContent = @"
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace $repoName.IRepositories
{
    public interface IRepositoryBase<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetById(object id);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        IEnumerable<T> Find(Expression<Func<T, bool>> expression);
        int Count(Expression<Func<T, bool>> expression);
        IEnumerable<T> Search(string searchTerm);
    }
}
"@

# Generate RepositoryBase class
$repositoryBaseContent = @"
using Microsoft.EntityFrameworkCore;
using $repoName.IRepositories;
using $repoName.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace $repoName.Repositories
{
    public class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected readonly ${contextClassName} _context;
        protected readonly DbSet<T> _dbSet;

        public RepositoryBase(${contextClassName} context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public int Count(Expression<Func<T, bool>> expression)
        {
            return _dbSet.Count(expression);
        }

        public void Delete(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> expression)
        {
            return _dbSet.Where(expression).ToList();
        }

        public IEnumerable<T> GetAll()
        {
            return _dbSet.ToList();
        }

        public T GetById(object id)
        {
            return _dbSet.Find(id);
        }

        public void Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public virtual IEnumerable<T> Search(string searchTerm)
        {
            var entities = GetAll();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                return entities.Where(x => x.ToString()!.ToLower().Contains(searchTerm.ToLower())).ToList();
            }
            return entities;
        }
    }
}
"@

# Generate IUnitOfWork interface
$iUnitOfWorkContent = @"
using System;

namespace $repoName.IRepositories
{
    public interface IUnitOfWork : IDisposable
    {
        int Complete();
"@

# Add repository properties to IUnitOfWork
foreach ($entityName in $entities) {
    $iUnitOfWorkContent += @"
        I${entityName}Repository ${entityName} { get; }
        
"@
}

$iUnitOfWorkContent += @"
    }
}
"@

# Generate UnitOfWork class
$unitOfWorkContent = @"
using $repoName.Models;
using $repoName.IRepositories;
using System;

namespace $repoName.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ${contextClassName} _context;

"@

# Add repository fields to UnitOfWork with unique names
foreach ($entityName in $entities) {
    $repoName = $entityName.ToLower()
    $fieldName = "_${repoName}Repository"
    $unitOfWorkContent += @"
        private I${entityName}Repository $fieldName;

"@
}

$unitOfWorkContent += @"

        public UnitOfWork(${contextClassName} context)
        {
            _context = context;
        }

"@

# Add repository properties to UnitOfWork with unique names
foreach ($entityName in $entities) {
    $repoName = $entityName.ToLower()
    $fieldName = "_${repoName}Repository"
    $unitOfWorkContent += @"

        public I${entityName}Repository ${entityName}
        {
            get
            {
                if ($fieldName == null)
                {
                    $fieldName = new ${entityName}Repository(_context);
                }
                return $fieldName;
            }
        }
"@
}

$unitOfWorkContent += @"

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
"@

# Create base files
try {
    Set-Content -Path "$iRepositoriesPath\IRepositoryBase.cs" -Value $iRepositoryBaseContent
    Write-Output "Generated IRepositoryBase interface"
    
    Set-Content -Path "$repositoriesPath\RepositoryBase.cs" -Value $repositoryBaseContent
    Write-Output "Generated RepositoryBase class"
    
    Set-Content -Path "$iRepositoriesPath\IUnitOfWork.cs" -Value $iUnitOfWorkContent
    Write-Output "Generated IUnitOfWork interface"
    
    Set-Content -Path "$repositoriesPath\UnitOfWork.cs" -Value $unitOfWorkContent
    Write-Output "Generated UnitOfWork class"
} catch {
    Write-Output "Failed to generate base classes: $_"
    exit
}

Write-Output "Generating base service classes..."

# Generate IServiceBase interface
$iServiceBaseContent = @"
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace $serviceName.IServices
{
    public interface IServiceBase<T> where T : class
    {
        IEnumerable<T> GetAll();
        T? GetById(int id);
        void Add(T entity);
        void Update(T entity);
        void Delete(int id);
        IEnumerable<T> Search(string searchTerm);
        IEnumerable<T> Find(Expression<Func<T, bool>> expression);
        int Count(Expression<Func<T, bool>> expression);
    }
}
"@

# Generate ServiceBase class
$serviceBaseContent = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using $repoName.IRepositories;
using $serviceName.IServices;


namespace $serviceName.Services
{
    public abstract class ServiceBase<T> : IServiceBase<T> where T : class
    {
        protected readonly IRepositoryBase<T> _repo;

        protected ServiceBase(IRepositoryBase<T> repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo), "Repository cannot be null.");
        }

        public virtual IEnumerable<T> GetAll()
        {
            return _repo.GetAll();
        }

        public virtual T? GetById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid ID.");

            return _repo.GetById(id);
        }

        public virtual void Add(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null.");
            
            _repo.Add(entity);
        }

        public virtual void Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null.");

            _repo.Update(entity);
        }

        public virtual void Delete(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid ID.");

            var entity = _repo.GetById(id);
            if (entity != null)
            {
                _repo.Delete(entity);
            }
        }

        public virtual IEnumerable<T> Search(string searchTerm)
        {
            return _repo.Search(searchTerm);
        }

        public virtual IEnumerable<T> Find(Expression<Func<T, bool>> expression)
        {
            return _repo.Find(expression);
        }

        public virtual int Count(Expression<Func<T, bool>> expression)
        {
            return _repo.Count(expression);
        }
    }
}
"@

# Create base service files
try {
    Set-Content -Path "$iServicePath\IServiceBase.cs" -Value $iServiceBaseContent
    Write-Output "Generated IServiceBase interface"
    
    Set-Content -Path "$outputServicePath\ServiceBase.cs" -Value $serviceBaseContent
    Write-Output "Generated ServiceBase class"
} catch {
    Write-Output "Failed to generate base service classes: $_"
    exit
}

Write-Output "Generating entity repositories and services..."

# Generate entity repositories and services
foreach ($entityName in $entities) {
    $namespace = "$repoName.IRepositories"
    $repoNamespace = "$repoName.Repositories"
    $serviceNamespace = "$serviceName.Services"
    $iServiceNamespace = "$serviceName.IServices"

    # Interface content
    $interfaceContent = @"
using System.Collections.Generic;
using $repoName.Models;

namespace $namespace
{
    public interface I${entityName}Repository : IRepositoryBase<${entityName}>
    {
    }
}
"@

    # Repository class content
    $repositoryContent = @"
using $repoName.Models;
using $namespace;
using System.Collections.Generic;
using System.Linq;

namespace $repoNamespace
{
    public class ${entityName}Repository : RepositoryBase<${entityName}>, I${entityName}Repository
    {
        public ${entityName}Repository(${contextClassName} context) : base(context)
        {
        }
    }
}
"@

    # IService interface content
    $iServiceContent = @"
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using $repoName.Models;

namespace $iServiceNamespace
{
    public interface I${entityName}Service : IServiceBase<${entityName}>
    {
    }
}
"@

    # Service class content
    $serviceContent = @"
using System;
using System.Collections.Generic;
using System.Linq;
using $repoName.Models;
using $namespace;
using $iServiceNamespace;

namespace $serviceNamespace
{
    public class ${entityName}Service : ServiceBase<${entityName}>, I${entityName}Service
    {
        private readonly I${entityName}Repository _repo;

        public ${entityName}Service(I${entityName}Repository repo) : base(repo)
        {
            _repo = repo;
        }
    }
}
"@

    try {
        # Generate repository files
        Set-Content -Path "$iRepositoriesPath\I${entityName}Repository.cs" -Value $interfaceContent
        Write-Output "Generated interface for ${entityName}"
        
        Set-Content -Path "$repositoriesPath\${entityName}Repository.cs" -Value $repositoryContent
        Write-Output "Generated repository for ${entityName}"

        # Generate service files
        Set-Content -Path "$iServicePath\I${entityName}Service.cs" -Value $iServiceContent
        Write-Output "Generated service interface for ${entityName}"
        
        Set-Content -Path "$outputServicePath\${entityName}Service.cs" -Value $serviceContent
        Write-Output "Generated service for ${entityName}"
    } catch {
        Write-Output "Failed to generate files for ${entityName}: $_"
    }
}

Write-Output "All repository and service classes generated successfully!" 