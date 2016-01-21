$resourceGroupName = "IOT"
$location = "West US"
$storageAccountNameVM = "iotstorageaccountvm"
$storageAccountNameDeepStorage1 = "iotstorageaccountblob1"
$storageAccountNameDeepStorage2 = "iotstorageaccountblob2"
$storageAccountNameDeepStorage3 = "iotstorageaccountblob3"
$vnetName = "iot_network"
$cloudServiceNameAPI = "iot_api"

# load functions
. .\functions\Create-ResourceGroup.ps1
. .\functions\Create-WindowsVM.ps1

# 1. create resource group 
if ((Get-AzureRmResourceGroup | Where-Object ResourceGroupName -EQ $resourceGroupName) -eq $null) {
    Write-Output "Creating resource group $resourceGroupName on location $location..."
    Create-ResourceGroup -name $resourceGroupName -location $location 
}
else {
    Write-Output "Resource group $resourceGroupName on location $location already exist"
}

# 2. create storage accounts 
# use geo-redundant by default 
$storageAccountType = "Standard_GRS"
if((Get-AzureRmStorageAccount -ResourceGroupName $resourceGroupName | Where-Object Name -EQ $storageAccountNameVM) -eq $null) {
    Write-Output "Creating storage account $storageAccountNameVM..."
    New-AzureRmStorageAccount -ResourceGroupName $resourceGroupName -Location $location -Name $storageAccountNameVM -Type $storageAccountType
} 
else {
    Write-Output "Storage account $storageAccountNameVM already exist within resource group $resourceGroupName"
}
$storageAccount = Get-AzureRmStorageAccount -ResourceGroupName $resourceGroupName -Name $storageAccountNameVM

# 3. create virtual network
if((Get-AzureRmVirtualNetwork -ResourceGroupName $resourceGroupName | Where-Object Name -EQ $vnetName) -eq $null) {
    Write-Output "Creating virtual network $vnetName..."
    $subnet = New-AzureRmVirtualNetworkSubnetConfig -Name "Subnet-1" -AddressPrefix "10.10.0.0/24"
    New-AzureRmVirtualNetwork -Location $location -ResourceGroupName $resourceGroupName -Name $vnetName -AddressPrefix "10.10.0.0/16" -Subnet $subnet
}
else {
    Write-Output "Virtual network $vnetName aready exist"
}
$vnet = Get-AzureRmVirtualNetwork -Name $vnetName -ResourceGroupName $resourceGroupName 


# 4. create VMs
$vmUserName = "li.xiaodong"
$vmPasswordString = "Summer1!" 
$vmNameDns = "IOT-DNS"
if((Get-AzureRmVM -ResourceGroupName $resourceGroupName | Where-Object Name -EQ $vmNameDns) -eq $null) {
    Write-Output "Creating VM $vmNameDns..."
    Create-WindowsVM -vmName $vmNameDns -resourceGroupName $resourceGroupName -vmUsername $vmUserName -vmPasswordString $vmPasswordString -vnetName $vnetName -storageAccountName $storageAccountNameVM -vmSize Standard_D1 -location $location
}
else {
    Write-Output "VM $vmNameDns already exist"
}


