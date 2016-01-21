function Create-WindowsVM (
    [Parameter(Mandatory=$true)][string]$vmName,
    [Parameter(Mandatory=$true)][string]$resourceGroupName,
    [Parameter(Mandatory=$true)][string]$vmUsername,
    [Parameter(Mandatory=$true)][string]$vmPasswordString,
    [Parameter(Mandatory=$true)][string]$vnetName,
    [Parameter(Mandatory=$true)][string]$storageAccountName,
    [ValidateSet("Standard_D1", "Standard_D2", "Standard_D3", "Standard_D4")][string]$vmSize = "Standard_D1",
    [Parameter(Mandatory=$true)][string][ValidateSet("East US", "West US")]$location
)
{
    $vmPassword = $vmPasswordString | ConvertTo-SecureString -AsPlainText -Force
    $vmCredential = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $vmUsername, $vmPassword
    $vmConfig = New-AzureRmVMConfig -VMName $vmName -VMSize $vmSize
    $vmConfig = Set-AzureRmVMOperatingSystem -VM $vmConfig -Windows -ComputerName $vmName -Credential $vmCredential -ProvisionVMAgent -EnableAutoUpdate
    $vmConfig = Set-AzureRmVMSourceImage -VM $vmConfig -PublisherName MicrosoftWindowsServer -Offer WindowsServer -Skus 2012-R2-DataCenter -Version "latest"
    
    $pubIpAddr = Get-AzureRmPublicIpAddress -ResourceGroupName $resourceGroupName | Where { $_.Name -eq ($vmName+"Interface")}
    if($pubIpAddr -eq $null) {
        $pubIpAddr = New-AzureRmPublicIpAddress -Name ($vmName+"Interface") -ResourceGroupName $resourceGroupName -Location $location -AllocationMethod Static
    }
    
    $vnet = Get-AzureRmVirtualNetwork -Name $vnetName -ResourceGroupName $resourceGroupName 
    $vmInterface = Get-AzureRmNetworkInterface -ResourceGroupName $resourceGroupName |  Where { $_.Name -eq ($vmName+"Interface") -and $_.IpConfigurations[0].Subnet.Id -eq $vnet.Subnets[0].Id }
    if($vmInterface -eq $null) {
        $vmInterface = New-AzureRmNetworkInterface -Name ($vmName+"Interface") -ResourceGroupName $resourceGroupName -Location $location -SubnetId $vnet.Subnets[0].Id -PublicIpAddressId $pubIpAddr.Id
    }
    $vmConfig = Add-AzureRmVMNetworkInterface -VM $vmConfig -Id $vmInterface.Id
    
    $storageAccount = Get-AzureRmStorageAccount -ResourceGroupName $resourceGroupName -Name $storageAccountName
    $storageKey = (Get-AzureRmStorageAccountKey -ResourceGroupName $resourceGroupName -Name $storageAccountName).Key1
    $storageContext = New-AzureStorageContext -StorageAccountName $storageAccountName -StorageAccountKey $storageKey
    $osDiskUri = $storageAccount.PrimaryEndpoints.Blob.ToString() + "vhds/" + ($vmNameDns+"OSDisk"+".vhd")
    Get-AzureStorageBlob -Context $storageContext -Container "vhds" | where { $_.Name -eq ($vmNameDns+"OSDisk"+".vhd") } | Remove-AzureStorageBlob
    $vmConfig = Set-AzureRmVMOSDisk -VM $vmConfig -Name ($vmNameDns+"OSDisk") -VhdUri $osDiskUri -CreateOption fromImage

    New-AzureRmVM -ResourceGroupName $resourceGroupName -Location $location -VM $vmConfig
}