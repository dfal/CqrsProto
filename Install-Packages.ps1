
# list all the solution folders where the "packages" folder will be placed.
$solutionRelativePaths = @('Sources')


$scriptPath = Split-Path (Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path 

$solutionFolders = New-Object object[] $solutionRelativePaths.Length
$allPackagesFiles = New-Object object[] $solutionRelativePaths.Length
for($i=0; $i -lt $solutionRelativePaths.Length; $i++)
{
    $solutionFolder = Join-Path $scriptPath $solutionRelativePaths[$i]
    $solutionFolders[$i] = $solutionFolder
    $allPackagesFiles[$i] = Get-ChildItem $solutionFolder -Include "packages.config" -Recurse
}


# get all the packages to install
$packages = @()
foreach ($packageFilesForSolution in $allPackagesFiles)
{
    $packageFilesForSolution | ForEach-Object { 
        $xml = New-Object "System.Xml.XmlDocument"
        $xml.Load($_.FullName)
        $xml | Select-Xml -XPath '//packages/package' | 
            Foreach { $packages += " - "+ $_.Node.id + " v" + $_.Node.version }
    }
}

$packages = $packages | Select -uniq | Sort-Object
$packages = [system.string]::Join("`r`n", $packages)

Write-Host "DOWNLOADING NUGET PACKAGE DEPENDENCIES"


# copy NuGet.exe bootstrapper to a temp folder if it's not there (this is to avoid distributing the full version of NuGet, and avoiding source control issues with updates).
$nuget = Join-Path $scriptPath 'Build\Temp\NuGet.exe'
$nugetExists = Test-Path $nuget

if ($nugetExists -eq 0)
{
	$tempFolder = Join-Path $scriptPath 'Build\Temp\'
	mkdir $tempFolder -Force > $null
	$nugetOriginal = Join-Path $scriptPath 'Build\NuGet.exe'
	Copy-Item $nugetOriginal -Destination $nuget -Force
}

$env:EnableNuGetPackageRestore=$true

for($i=0; $i -lt $solutionFolders.Length; $i++)
{
    pushd $solutionFolders[$i]

    # install the packages
    $allPackagesFiles[$i] | ForEach-Object { & $nuget install $_.FullName -o packages }

    popd
}