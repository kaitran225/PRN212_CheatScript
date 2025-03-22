# Prompt user for project directory
$projectDir = Read-Host "Enter the full path of your project directory"

# Extract project name from directory path
$projectName = Split-Path -Path $projectDir -Leaf

# Define paths for Models, Repositories, and Context
$modelsPath = "$projectDir\Models"
$outputPath = "$projectDir\Repositories"

# Ensure output directory exists
if (!(Test-Path $outputPath)) {
    New-Item -ItemType Directory -Path $outputPath
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

foreach ($entityName in $entities) {
    $namespace = "$projectName.Repositories"

    # Interface content
    $interfaceContent = @"
using System.Collections.Generic;
using $projectName.Models;

namespace $namespace
{
    public interface I${entityName}Repository
    {
        IEnumerable<${entityName}> GetAll();
        ${entityName} GetById(int id);
        void Add(${entityName} entity);
        void Update(${entityName} entity);
        void Delete(int id);
    }
}
"@

    # Repository class content
    $repositoryContent = @"
using $projectName.Models;
using System.Collections.Generic;
using System.Linq;

namespace $namespace
{
    public class ${entityName}Repository : I${entityName}Repository
    {
        private readonly ${contextClassName}? _dbContext;

        private ${entityName}Repository() { }

        public ${entityName}Repository(${contextClassName} context)
        {
            _dbContext = context;
        }

        public IEnumerable<${entityName}> GetAll()
        {
            return _dbContext!.Set<${entityName}>().ToList();
        }

        public ${entityName} GetById(int id)
        {
            return _dbContext!.Set<${entityName}>().Find(id)!;
        }

        public void Add(${entityName} entity)
        {
            _dbContext?.Set<${entityName}>().Add(entity);
            _dbContext?.SaveChanges();
        }

        public void Update(${entityName} entity)
        {
            _dbContext?.Set<${entityName}>().Update(entity);
            _dbContext?.SaveChanges();
        }

        public void Delete(int id)
        {
            ${entityName}? entity = _dbContext!.Set<${entityName}>().Find(id);
            if (entity != null)
            {
                _dbContext?.Set<${entityName}>().Remove(entity);
                _dbContext?.SaveChanges();
            }
        }
    }
}
"@

    # Create repository files
    $interfaceFile = "$outputPath\I${entityName}Repository.cs"
    $repositoryFile = "$outputPath\${entityName}Repository.cs"

    Set-Content -Path $interfaceFile -Value $interfaceContent
    Set-Content -Path $repositoryFile -Value $repositoryContent
}

Write-Output "Repositories generated successfully for all entities in $contextClassName!"
