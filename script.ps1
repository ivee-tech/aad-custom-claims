$tenantId = '***'
$otherAppName = '***'

# connect to the tenant
Connect-AzureAD -TenantId $tenantId

# create App Registration and SP
$appName = 'DeseExternalClaimsApp'
$app = New-AzureADApplication -DisplayName $appName
New-AzureADServicePrincipal -AppId $app.AppId 

# create custom property
$propName = 'CustomCode'
$extensionProperty = New-AzureADApplicationExtensionProperty -ObjectId $app.ObjectId -Name $propName -DataType "String" -TargetObjects "User"
$extensionProperty


# create claims mapping policy to map the custom property to other app registration
$extensionAppName = 'DeseExternalClaimsApp'
$filter = "DisplayName eq '" + $extensionAppName + "'"
$schemaExtensionApp = (Get-AzureADApplication -Filter $filter)
 
$extensionName = 'CustomCode'
$extensionId = (Get-AzureADApplicationExtensionProperty -ObjectId $schemaExtensionApp.ObjectId).Where( { $_.Name.endsWith($extensionName)})[0].Name
$extensionId

 
$claimsMappingPolicy = @{
  ClaimsMappingPolicy = @{
    Version = 1
    IncludeBasicClaimSet = $true
    ClaimsSchema = @(
    @{
        Source = "User"
        ExtensionID = $extensionId
        JwtClaimType = $extensionName
     }
    )
  }
}

$filter = "DisplayName eq '" + $otherAppName + "'"
$sp = (Get-AzureADServicePrincipal -Filter $filter) 

$policyName = 'CustomCode_Policy'
$policyDef = $claimsMappingPolicy | ConvertTo-Json -Depth 10 -Compress
$policy = New-AzureADPolicy -Type "ClaimsMappingPolicy" -DisplayName $policyName -Definition $policyDef
Add-AzureADServicePrincipalPolicy -Id $sp.ObjectId -RefObjectId $policy.Id
