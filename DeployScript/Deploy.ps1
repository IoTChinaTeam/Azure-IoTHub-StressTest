<#
 .SYNOPSIS
    Deploys a template to Azure

 .DESCRIPTION
    Deploys an Azure Resource Manager template

 .PARAMETER subscriptionId
    The subscription id where the template will be deployed.

 .PARAMETER resourceGroupName
    The resource group where the template will be deployed. Can be the name of an existing or a new resource group.

 .PARAMETER resourceGroupLocation
    Optional, a resource group location. If specified, will try to create a new resource group in this location. If not specified, assumes resource group is existing.

 .PARAMETER deploymentName
    The deployment name.

 .PARAMETER templateFilePath
    Optional, path to the template file. Defaults to template.json.

 .PARAMETER parametersFilePath
    Optional, path to the parameters file. Defaults to parameters.json. If file is not found, will prompt for parameter values based on template.
#>

param(
 [string]
 $AzureEnvironmentName='AzureCloud',

 [string]
 $subscriptionId,


 [string]
 $resourceGroupName,

 [string]
 $templateFilePath = "template.json",

 [string]
 $parametersFilePath ="parameters.json" 
)

<#
.SYNOPSIS
    Registers RPs
#>
Function RegisterRP {
    Param(
        [string]$ResourceProviderNamespace
    )

    Write-Host "Registering resource provider '$ResourceProviderNamespace'";
    Register-AzureRmResourceProvider -ProviderNamespace $ResourceProviderNamespace;
}

#******************************************************************************
# Script body
# Execution begins here
#******************************************************************************
$ErrorActionPreference = "Stop"


. "$(Split-Path $MyInvocation.MyCommand.Path)\Deploylib.ps1"

#login
ValidateLoginCredentials

# select subscription
$subscriptionId = SelectSubscriptionId
echo $subscriptionId
set-azurermcontext -SubscriptionId $SubscriptionId


 # Register RPs
$resourceProviders = @("microsoft.batch","microsoft.devices","microsoft.storage");
if($resourceProviders.length) {
    Write-Host "Registering resource providers"
    foreach($resourceProvider in $resourceProviders) {
        RegisterRP($resourceProvider);
    }
}

# Get resource group ready
$resourceGroupName = PrepareResourceGroup

# Get template and parameter file for current azure environment
$currentenv = $global:azureEnvironment.Name
switch ($currentenv)
{
     "AzureCloud"
     {
        $templateFilePath="Global\$templateFilePath"
        $parametersFilePath = "Global\$parametersFilePath"
     }
     "AzureChinaCloud"
     {
        $templateFilePath="China\$templateFilePath"
        $parametersFilePath = "China\$parametersFilePath"
     }
     “AzureGermanCloud”
     {
         $templateFilePath="German\$templateFilePath"
         $parametersFilePath = "German\$parametersFilePath"
     }
}

(Get-Content $parametersFilePath).replace('placeholder', $global:resourceGroupLocation) | Set-Content $parametersFilePath
# Start the deployment
Write-Host "Starting deployment...";
if(Test-Path $parametersFilePath) {
    New-AzureRmResourceGroupDeployment -ResourceGroupName $resourceGroupName -TemplateFile $templateFilePath -TemplateParameterFile $parametersFilePath;
} else {
    New-AzureRmResourceGroupDeployment -ResourceGroupName $resourceGroupName -TemplateFile $templateFilePath;
    }

(Get-Content $parametersFilePath).replace($global:resourceGroupLocation, 'placeholder') | Set-Content $parametersFilePath