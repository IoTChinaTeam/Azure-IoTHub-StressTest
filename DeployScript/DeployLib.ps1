function ValidateLoginCredentials()
{
    # Validate Azure RM
    $profilePath = Join-Path $PSScriptRoot "user"
    if (test-path $profilePath) 
    {
        $useold = Read-Host "`nDo you want to use the stored credential for logging in?(Y/N)`n"
        if($useold.ToLowerInvariant() -ne "y")
        {
            $AzureEnvironmentName= Read-Host "`nPlease specify azure environment(AzureCloud/AzureChinaCloud/AzureGermanCloud)"
            SpecifyAzureEnvironment $AzureEnvironmentName
            Write-Host "`nManual Logging in"
            Login-AzureRmAccount -EnvironmentName $global:azureEnvironment.Name | Out-Null
            Save-AzureRmProfile -Path $profilePath
            return;
        }
        else
        {
            Write-Host "`nTrying to use saved profile $($profilePath)"
            $rmProfile = Select-AzureRmProfile -Path $profilePath
            $global:azureEnvironment = $rmProfile.Context.Environment
            $rmProfileLoaded = ($rmProfile -ne $null) -and ($rmProfile.Context -ne $null) -and ((Get-AzureRmSubscription) -ne $null)
        }
    }
    else
    {
            $AzureEnvironmentName= Read-Host "`nPlease specify azure environment(AzureCloud/AzureChinaCloud/AzureGermanCloud)"
            SpecifyAzureEnvironment $AzureEnvironmentName
            Write-Host "`nManual Logging in"
            Login-AzureRmAccount -EnvironmentName $global:azureEnvironment.Name | Out-Null
            Save-AzureRmProfile -Path $profilePath
            return;
    }

    if ($rmProfileLoaded -ne $true) {
        $AzureEnvironmentName= Read-Host "`nPlease specify azure environment(AzureCloud/AzureChinaCloud/AzureGermanCloud)"
            SpecifyAzureEnvironment $AzureEnvironmentName
            Write-Host "`nManual Logging in"
            Login-AzureRmAccount -EnvironmentName $global:azureEnvironment.Name | Out-Null
            Save-AzureRmProfile -Path $profilePath
            return;
    }
}

function CreateNewRG()
{
    Param
    (
    [Parameter(Mandatory=$True)][string] $rgname
    )
    Write-Host "`nResource group '$rgname ' does not exist. To create a new resource group, please enter a location.";
    $global:resourceGroupLocation = Read-Host "`nresourceGroupLocation";
    Write-Host "`nCreating resource group '$rgname ' in location '$global:resourceGroupLocation'";
    $newrg = New-AzureRmResourceGroup -Name $rgname  -Location $global:resourceGroupLocation
    return $newrg[0]
}

function PrepareResourceGroup()
{
   $NameOfRG  = Read-Host "`nPlease input the resource group name where you want to run load test`n"
    #Create or check for existing resource group
    $resourceGroup = Get-AzureRmResourceGroup -Name $NameOfRG  -ErrorAction SilentlyContinue
    if(!$resourceGroup)
    {
        $newrg =CreateNewRg $NameOfRG 
        return $newrg.ResourceGroupName
    }
    else
    { 
        $resp = Read-Host "`nResource group '$NameOfRG ' already exists. Do you want to use it anyway?(Y/N)";
        if($resp.ToLowerInvariant() -eq "n")
        {
            PrepareResourceGroup
        } 
        else
        {
            $global:resourceGroupLocation=$resourceGroup.Location
            return $NameOfRG 
        }
    }
}

function SelectSubscriptionId()
{

            $subsId = "not set"
            $subscriptions = Get-AzureRMSubscription
            Write-Host "Available subscriptions:"
            $global:index = 0
            $selectedIndex = -1
            Write-Host ($subscriptions | Format-Table -Property @{name="Option";expression={$global:index;$global:index+=1}},SubscriptionName, SubscriptionId -au | Out-String)

            while (!$subscriptions.SubscriptionId.Contains($subsId))
            {
                try
                {
                    [int]$selectedIndex = Read-Host "`nSelect an option from the above list"
                }
                catch
                {
                    Write-Host "Must be a number"
                    continue
                }

                if ($selectedIndex -lt 1 -or $selectedIndex -gt $subscriptions.length)
                {
                    continue
                }
                $subsId = $subscriptions[$selectedIndex - 1].SubscriptionId
                return $subsId
            }

}

function SpecifyAzureEnvironment()
{
    Param
    (
        [Parameter(Mandatory=$True)][string]$azureenvironmentname
    )
    switch($azureenvironmentname)
{
    "AzureCloud" {
        if ((Get-AzureEnvironment AzureCloud) -eq $null)
        {
            Add-AzureEnvironment –Name AzureCloud -EnableAdfsAuthentication $False -ActiveDirectoryServiceEndpointResourceId https://management.core.windows.net/ -GalleryUrl https://gallery.azure.com/ -ServiceManagementUrl https://management.core.windows.net/ -SqlDatabaseDnsSuffix .database.windows.net -StorageEndpointSuffix core.windows.net -ActiveDirectoryAuthority https://login.microsoftonline.com/ -GraphUrl https://graph.windows.net/ -trafficManagerDnsSuffix trafficmanager.net -AzureKeyVaultDnsSuffix vault.azure.net -AzureKeyVaultServiceEndpointResourceId https://vault.azure.net -ResourceManagerUrl https://management.azure.com/ -ManagementPortalUrl http://go.microsoft.com/fwlink/?LinkId=254433
        }

        if ((Get-AzureRMEnvironment AzureCloud) -eq $null)
        {
            Add-AzureRMEnvironment –Name AzureCloud -EnableAdfsAuthentication $False -ActiveDirectoryServiceEndpointResourceId https://management.core.windows.net/ -GalleryUrl https://gallery.azure.com/ -ServiceManagementUrl https://management.core.windows.net/ -SqlDatabaseDnsSuffix .database.windows.net -StorageEndpointSuffix core.windows.net -ActiveDirectoryAuthority https://login.microsoftonline.com/ -GraphUrl https://graph.windows.net/ -trafficManagerDnsSuffix trafficmanager.net -AzureKeyVaultDnsSuffix vault.azure.net -AzureKeyVaultServiceEndpointResourceId https://vault.azure.net -ResourceManagerUrl https://management.azure.com/ -ManagementPortalUrl http://go.microsoft.com/fwlink/?LinkId=254433
        }

        $global:locations = @("East US", "North Europe", "East Asia", "West US", "West Europe", "Southeast Asia", "Japan East", "Japan West", "Australia East", "Australia Southeast")
    }
    "AzureGermanCloud" {
        if ((Get-AzureEnvironment AzureGermanCloud) -eq $null)
        {
            Add-AzureEnvironment –Name AzureGermanCloud -EnableAdfsAuthentication $False -ActiveDirectoryServiceEndpointResourceId https://management.core.cloudapi.de/ -GalleryUrl https://gallery.cloudapi.de -ServiceManagementUrl https://management.core.cloudapi.de/ -SqlDatabaseDnsSuffix .database.cloudapi.de -StorageEndpointSuffix core.cloudapi.de -ActiveDirectoryAuthority https://login.microsoftonline.de/ -GraphUrl https://graph.cloudapi.de/ -trafficManagerDnsSuffix azuretrafficmanager.de -AzureKeyVaultDnsSuffix vault.microsoftazure.de -AzureKeyVaultServiceEndpointResourceId https://vault.microsoftazure.de -ResourceManagerUrl https://management.microsoftazure.de/ -ManagementPortalUrl https://portal.microsoftazure.de
        }

        if ((Get-AzureRMEnvironment AzureGermanCloud) -eq $null)
        {
            Add-AzureRMEnvironment –Name AzureGermanCloud -EnableAdfsAuthentication $False -ActiveDirectoryServiceEndpointResourceId https://management.core.cloudapi.de/ -GalleryUrl https://gallery.cloudapi.de -ServiceManagementUrl https://management.core.cloudapi.de/ -SqlDatabaseDnsSuffix .database.cloudapi.de -StorageEndpointSuffix core.cloudapi.de -ActiveDirectoryAuthority https://login.microsoftonline.de/ -GraphUrl https://graph.cloudapi.de/ -trafficManagerDnsSuffix azuretrafficmanager.de -AzureKeyVaultDnsSuffix vault.microsoftazure.de -AzureKeyVaultServiceEndpointResourceId https://vault.microsoftazure.de -ResourceManagerUrl https://management.microsoftazure.de/ -ManagementPortalUrl https://portal.microsoftazure.de
        }

        $global:locations = @("Germany Central", "Germany Northeast")
    }
	"AzureChinaCloud" {
       if ((Get-AzureEnvironment AzureChinaCloud) -eq $null)
       {
           Add-AzureEnvironment –Name AzureChinaCloud -EnableAdfsAuthentication $False -ActiveDirectoryServiceEndpointResourceId https://management.core.chinacloudapi.cn/ -GalleryUrl https://gallery.chinacloudapi.cn -ServiceManagementUrl https://management.core.chinacloudapi.cn/ -SqlDatabaseDnsSuffix .database.chinacloudapi.cn -StorageEndpointSuffix core.chinacloudapi.cn -ActiveDirectoryAuthority https://login.microsoftonline.cn/ -GraphUrl https://graph.chinacloudapi.cn/ -trafficManagerDnsSuffix azuretrafficmanager.cn -AzureKeyVaultDnsSuffix vault.azure.cn -AzureKeyVaultServiceEndpointResourceId https://vault.azure.cn -ResourceManagerUrl https://management.chinacloudapi.cn/ -ManagementPortalUrl http://go.microsoft.com/fwlink/?LinkId=301902
       }

       if ((Get-AzureRMEnvironment AzureChinaCloud) -eq $null)
       {
           Add-AzureRMEnvironment –Name AzureChinaCloud -EnableAdfsAuthentication $False -ActiveDirectoryServiceEndpointResourceId https://management.core.chinacloudapi.cn/ -GalleryUrl https://gallery.chinacloudapi.cn -ServiceManagementUrl https://management.core.chinacloudapi.cn/ -SqlDatabaseDnsSuffix .database.chinacloudapi.cn -StorageEndpointSuffix core.chinacloudapi.cn -ActiveDirectoryAuthority https://login.microsoftonline.cn/ -GraphUrl https://graph.chinacloudapi.cn/ -trafficManagerDnsSuffix azuretrafficmanager.cn -AzureKeyVaultDnsSuffix vault.azure.cn -AzureKeyVaultServiceEndpointResourceId https://vault.azure.cn -ResourceManagerUrl https://management.chinacloudapi.cn/ -ManagementPortalUrl http://go.microsoft.com/fwlink/?LinkId=301902
       }

       $global:locations = @("China North", "China East")
	}
    default {throw ("'{0}' is not a supported Azure Cloud environment" -f $azureEnvironmentName)}
}
$global:azureEnvironment = Get-AzureEnvironment $azureEnvironmentName
}