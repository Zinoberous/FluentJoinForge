$TestConfig = "Debug"

$ReportFolder = './CoverageReport'
$HistoryFolder = './CoverageHistory'

$DefaultErrorActionPreference = $ErrorActionPreference
$ErrorActionPreference = 'silentlyContinue'

# This only needs to be installed once (globally), if installed it fails silently:
dotnet tool install -g dotnet-reportgenerator-globaltool

$ErrorActionPreference = $DefaultErrorActionPreference

# Delete previous test run results (there's a bunch of subfolders named with guids)
Get-ChildItem -include TestResults -recurse | foreach ($_) { remove-item $_.fullname -force -recurse }

# Run the Coverlet.Collector
dotnet test --collect:"XPlat Code Coverage" 2>&1 --configuration $TestConfig

# To keep a history of the Code Coverage we need to use the argument: -historydir:SOME_DIRECTORY 
if (!(Test-Path $HistoryFolder)) {  
    New-Item -ItemType directory -Path $HistoryFolder
}

# Delete previous test run reports - note if you're getting wrong results do a Solution Clean and Rebuild to remove stale DLLs in the bin folder
if (Test-Path $ReportFolder) {
    Remove-Item -Recurse -Force $ReportFolder
}

# Generate the Code Coverage HTML Report
reportgenerator -reports:"./**/coverage.cobertura.xml" -targetdir:$ReportFolder -reporttypes:Html -historydir:$HistoryFolder

# Delete test run results after use
Get-ChildItem -include TestResults -recurse | foreach ($_) { remove-item $_.fullname -force -recurse }

# Open the Code Coverage HTML Report (if running on a WorkStation)
$osInfo = Get-CimInstance -ClassName Win32_OperatingSystem
if ($osInfo.ProductType -eq 1) {
    (& "$ReportFolder/index.html")
}
