[CmdletBinding()]
param(
	[Parameter(Mandatory=$true)]
	[string]$packageId,

	[Parameter(Mandatory=$true)]
	[string]$newVersion)


$assemblyReferencePattern = "($packageId, Version)=\d+\.\d+\.\d+\.\d+"
$hitPathPattren = "($packageId)\.\d+\.\d+\.\d+\.\d+"

Get-ChildItem -Filter *.*proj -Recurse |
	Select-String -List -Pattern $assemblyReferencePattern | % `
{
	(Get-Content $_.Path) | % `
	{ $_-replace $assemblyReferencePattern, "`$1=$newVersion" } | % `
	{ $_-replace $hitPathPattren, "`$1.$newVersion" } |
	Set-Content $_.Path -Force
	Write-Host $_.Path, " - Updated" -Foregroundcolor "darkgreen"
	
}

$packageConfigFilePattern = "(id=""$packageId"" version)=""\d+\.\d+\.\d+\.\d+"""
Get-ChildItem -Filter packages.config -Recurse |
	Select-String -List -Pattern $packageConfigFilePattern | % `
{
	(Get-Content $_.Path) | % `
	{ $_-replace $packageConfigFilePattern, "`$1=""$newVersion""" } |
	Set-Content $_.Path -Force
	Write-Host $_.Path, " - Updated" -foregroundcolor "green"
}
