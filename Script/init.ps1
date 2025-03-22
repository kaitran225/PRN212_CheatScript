# Prompt user for project directories
$repoPath = Read-Host "Enter the full path of the Repository project"
$servicePath = Read-Host "Enter the full path of the Services project"
$wpfPath = Read-Host "Enter the full path of the main WPF project"

# Extract project names
$repoProjectName = Split-Path -Path $repoPath -Leaf
$serviceProjectName = Split-Path -Path $servicePath -Leaf
$wpfProjectName = Split-Path -Path $wpfPath -Leaf

# Get solution directory (parent of repository project)
$solutionDir = Split-Path -Path $repoPath -Parent

# Define the packages and versions
$packages = @(
    "Microsoft.EntityFrameworkCore.SqlServer -Version 8.0.2",
    "Microsoft.EntityFrameworkCore.Tools -Version 8.0.2",
    "Microsoft.EntityFrameworkCore.Design -Version 8.0.2",
    "Microsoft.Extensions.Configuration -Version 8.0.2",
    "Microsoft.Extensions.Configuration.Json -Version 8.0.2"
)

Write-Host "Installing required packages..." -ForegroundColor Cyan

# Get all .csproj files in the solution directory
$projects = Get-ChildItem -Path $solutionDir -Recurse -Filter *.csproj

# Check if any .csproj files were found
if ($projects.Count -eq 0) {
    Write-Host "No .csproj files found in the solution directory." -ForegroundColor Yellow
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

Write-Host "Adding project references..." -ForegroundColor Cyan

# Add reference from WPF to Service
Write-Host "Adding reference from $wpfProjectName to $serviceProjectName" -ForegroundColor Green
dotnet add "$wpfPath\$wpfProjectName.csproj" reference "$servicePath\$serviceProjectName.csproj"

# Add reference from Service to Repository
Write-Host "Adding reference from $serviceProjectName to $repoProjectName" -ForegroundColor Green
dotnet add "$servicePath\$serviceProjectName.csproj" reference "$repoPath\$repoProjectName.csproj"

Write-Host "Project references added successfully!" -ForegroundColor Green
Write-Host "Starting repository and service generation..." -ForegroundColor Cyan

# Define paths for Models, Repositories, and IRepositories
$modelsPath = "$repoPath\Models"
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