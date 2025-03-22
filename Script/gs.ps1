# Prompt user for project directories
$servicePath = Read-Host "Enter the full path of the Services project"
$repoPath = Read-Host "Enter the full path of the Repository project"

# Extract project names
$serviceProjectName = Split-Path -Path $servicePath -Leaf
$repoProjectName = Split-Path -Path $repoPath -Leaf

# Define paths for Models and Repositories
$modelsPath = "$repoPath\Models"
$repositoriesPath = "$repoPath\Repositories"
$iRepositoriesPath = "$repoPath\IRepositories"

# Define output directory for services
$outputServicePath = "$servicePath\Services"

# Ensure the services directory exists
if (!(Test-Path $outputServicePath)) {
    New-Item -ItemType Directory -Path $outputServicePath -Force
}

# Scan the Repositories folder for repository classes (excluding Context files)
$repoClasses = Get-ChildItem -Path $repositoriesPath -Filter "*Repository.cs" | Where-Object { $_.BaseName -ne "DbContext" -and $_.BaseName -ne "RepositoryBase" }

# Check if any repositories were found
if ($repoClasses.Count -eq 0) {
    Write-Output "No repositories found in $repositoriesPath. Ensure your repository files exist and follow the naming convention '*Repository.cs'."
    exit
}

foreach ($repoFile in $repoClasses) {
    $entity = $repoFile.BaseName -replace "Repository$", ""
    $namespace = "$serviceProjectName.Services"

    # Generate service class content with validation
    $serviceContent = @"
using System;
using System.Collections.Generic;
using System.Linq;
using $repoProjectName.Models;
using $repoProjectName.IRepositories;

namespace $namespace
{
    public class ${entity}Service
    {
        private readonly I${entity}Repository _repo;

        public ${entity}Service(I${entity}Repository repo)
        {
            _repo = repo;
        }

        public IEnumerable<${entity}> GetAll()
        {
            return _repo.GetAll();
        }

        public ${entity}? GetById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid ID.");

            return _repo.GetById(id);
        }

        public void Add(${entity} entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "${entity} cannot be null.");
            
            _repo.Add(entity);
        }

        public void Update(${entity} entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "${entity} cannot be null.");

            _repo.Update(entity);
        }

        public void Delete(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid ID.");

            var entity = _repo.GetById(id);
            if (entity != null)
            {
                _repo.Delete(entity);
            }
        }

        public IEnumerable<${entity}> Search(string searchTerm)
        {
            return _repo.Search(searchTerm);
        }

        public IEnumerable<${entity}> Find(System.Linq.Expressions.Expression<Func<${entity}, bool>> expression)
        {
            return _repo.Find(expression);
        }

        public int Count(System.Linq.Expressions.Expression<Func<${entity}, bool>> expression)
        {
            return _repo.Count(expression);
        }
    }
}
"@

    # Define service file path
    $serviceFile = "$outputServicePath\${entity}Service.cs"

    # Write content to file and check if successful
    try {
        Set-Content -Path $serviceFile -Value $serviceContent -Force
        Write-Output "Generated: $serviceFile"
    } catch {
        Write-Output "Failed to create: $serviceFile"
    }
}

Write-Output "Service layer generation completed successfully!"
