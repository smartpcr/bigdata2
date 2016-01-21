Get-AzurePublishSettingsFile
# download and rename as biosoft.publishsettings
# make sure pwd is changed

Import-AzurePublishSettingsFile biosoft.publishsettings
Select-AzureSubscription "Bizspark"


Login-AzureRmAccount # cannot be co-administrator


