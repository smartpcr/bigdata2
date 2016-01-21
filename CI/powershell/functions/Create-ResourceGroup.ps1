function Create-ResourceGroup (
    [Parameter(Mandatory=$true)][string]$name,
    [Parameter(Mandatory=$true)][string][ValidateSet("East US", "West US")]$location
)
{
    New-AzureRmResourceGroup -Name $name -Location $location
}