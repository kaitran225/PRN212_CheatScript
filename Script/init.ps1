# Collect all inputs upfront
Write-Host "Please provide the following information:" -ForegroundColor Cyan
Write-Host "----------------------------------------"

# Get project paths and trim them
$repoPath = (Read-Host "Enter the full path of the Repository project").Trim()
$servicePath = (Read-Host "Enter the full path of the Services project").Trim()
$wpfPath = (Read-Host "Enter the full path of the main WPF project").Trim()
$databaseName = (Read-Host "Enter the database name (e.g., Sp25PerfumeStoreDB)").Trim()

# Define SQL Server credentials and connection settings
$serverName = "KAINOTE\SQLEXPRESS"    # SQL Server instance name
$userId = "sa"                        # SQL Server username
$password = "123456"                  # SQL Server password

# Build the connection string dynamically
$connectionString = "Server=$serverName;Database=$databaseName;User Id=$userId;Password=$password;TrustServerCertificate=True;"

# Generate context name
$contextName = $databaseName + "Context"

# Validate paths exist and remove any trailing spaces
$repoPath = $repoPath.TrimEnd()
$servicePath = $servicePath.TrimEnd()
$wpfPath = $wpfPath.TrimEnd()

if (-not (Test-Path $repoPath)) {
    Write-Host "Error: Repository project path does not exist: $repoPath" -ForegroundColor Red
    exit
}
if (-not (Test-Path $servicePath)) {
    Write-Host "Error: Services project path does not exist: $servicePath" -ForegroundColor Red
    exit
}
if (-not (Test-Path $wpfPath)) {
    Write-Host "Error: WPF project path does not exist: $wpfPath" -ForegroundColor Red
    exit
}

# Validate project files exist (ensure no extra spaces in path construction)
$repoProjectFile = Join-Path $repoPath "$(Split-Path -Path $repoPath -Leaf).csproj"
$serviceProjectFile = Join-Path $servicePath "$(Split-Path -Path $servicePath -Leaf).csproj"
$wpfProjectFile = Join-Path $wpfPath "$(Split-Path -Path $wpfPath -Leaf).csproj"

if (-not (Test-Path $repoProjectFile)) {
    Write-Host "Error: Repository project file not found: $repoProjectFile" -ForegroundColor Red
    exit
}
if (-not (Test-Path $serviceProjectFile)) {
    Write-Host "Error: Services project file not found: $serviceProjectFile" -ForegroundColor Red
    exit
}
if (-not (Test-Path $wpfProjectFile)) {
    Write-Host "Error: WPF project file not found: $wpfProjectFile" -ForegroundColor Red
    exit
}

# Validate project files are valid XML
Write-Host "`nValidating project files..." -ForegroundColor Cyan
try {
    $repoXml = [xml](Get-Content $repoProjectFile -Raw)
    Write-Host "Repository project file is valid XML" -ForegroundColor Green
} catch {
    Write-Host "Error: Repository project file is not valid XML: $_" -ForegroundColor Red
    exit
}

try {
    $serviceXml = [xml](Get-Content $serviceProjectFile -Raw)
    Write-Host "Service project file is valid XML" -ForegroundColor Green
} catch {
    Write-Host "Error: Service project file is not valid XML: $_" -ForegroundColor Red
    exit
}

try {
    $wpfXml = [xml](Get-Content $wpfProjectFile -Raw)
    Write-Host "WPF project file is valid XML" -ForegroundColor Green
} catch {
    Write-Host "Error: WPF project file is not valid XML: $_" -ForegroundColor Red
    exit
}
# Extract project names
$repoProjectName = Split-Path -Path $repoPath -Leaf
$serviceProjectName = Split-Path -Path $servicePath -Leaf
$wpfProjectName = Split-Path -Path $wpfPath -Leaf

# Get solution directory
$solutionDir = Split-Path -Path $repoPath -Parent

# Show summary of what will be done
Write-Host "`nSummary of operations to be performed:" -ForegroundColor Yellow
Write-Host "----------------------------------------"
Write-Host "1. Install packages in all projects:"
Write-Host "   - Repository project: $repoProjectName"
Write-Host "   - Service project: $serviceProjectName"
Write-Host "   - WPF project: $wpfProjectName"
Write-Host "`n2. Add project references:"
Write-Host "   - $wpfProjectName -> $serviceProjectName"
Write-Host "   - $serviceProjectName -> $repoProjectName"
Write-Host "`n3. Generate database context:"
Write-Host "   - Database: $databaseName"
Write-Host "   - Server: $serverName"
Write-Host "   - Context: $contextName"
Write-Host "   - Output: $repoPath\Models"
Write-Host "`n4. Generate repository and service layers:"
Write-Host "   - Base classes and interfaces"
Write-Host "   - Entity-specific repositories and services"
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
Push-Location $repoPath
foreach ($package in $packages) {
    Write-Host "Installing package: $package" -ForegroundColor Yellow
    dotnet add package $package --version 8.0.2
}
Pop-Location

Write-Host "`nInstalling packages for Service project..." -ForegroundColor Green
Push-Location $servicePath
foreach ($package in $packages) {
    Write-Host "Installing package: $package" -ForegroundColor Yellow
    dotnet add package $package --version 8.0.2
}
Pop-Location

Write-Host "`nInstalling packages for WPF project..." -ForegroundColor Green
Push-Location $wpfPath
foreach ($package in $packages) {
    Write-Host "Installing package: $package" -ForegroundColor Yellow
    dotnet add package $package --version 8.0.2
}
Pop-Location

Write-Host "`nPackage installation completed!" -ForegroundColor Green

# Add project references
Write-Host "`nAdding project references..." -ForegroundColor Cyan
Write-Host "Adding reference from $wpfProjectName to $serviceProjectName" -ForegroundColor Green
dotnet add "$wpfProjectFile" reference "$serviceProjectFile"
Write-Host "Adding reference from $serviceProjectName to $repoProjectName" -ForegroundColor Green
dotnet add "$serviceProjectFile" reference "$repoProjectFile"

# Generate database context
Write-Host "`nGenerating database context..." -ForegroundColor Cyan
$modelsPath = Join-Path $repoPath "Models"
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
    "--project `"$repoProjectFile`" " + `
    "--startup-project `"$wpfProjectFile`" " + `
    "--force"

Write-Host "Executing command: $command" -ForegroundColor Yellow

# Change to the solution directory before running the command
Push-Location $solutionDir
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
$repositoriesPath = "$repoPath\Repositories"
$iRepositoriesPath = "$repoPath\IRepositories"
$outputServicePath = "$servicePath\Services"
$iServicePath = "$servicePath\IServices"

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

namespace $repoProjectName.IRepositories
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
using $repoProjectName.IRepositories;
using $repoProjectName.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace $repoProjectName.Repositories
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

namespace $repoProjectName.IRepositories
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
using $repoProjectName.Models;
using $repoProjectName.IRepositories;
using System;

namespace $repoProjectName.Repositories
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

namespace $serviceProjectName.IServices
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
using $repoProjectName.IRepositories;
using $serviceProjectName.IServices;


namespace $serviceProjectName.Services
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
    $namespace = "$repoProjectName.IRepositories"
    $repoNamespace = "$repoProjectName.Repositories"
    $serviceNamespace = "$serviceProjectName.Services"
    $iServiceNamespace = "$serviceProjectName.IServices"

    # Interface content
    $interfaceContent = @"
using System.Collections.Generic;
using $repoProjectName.Models;

namespace $namespace
{
    public interface I${entityName}Repository : IRepositoryBase<${entityName}>
    {
    }
}
"@

    # Repository class content
    $repositoryContent = @"
using $repoProjectName.Models;
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
using $repoProjectName.Models;

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
using $repoProjectName.Models;
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