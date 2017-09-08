### .\CreateRetailId.ps1 -Uri "http://opewdevcgy002.network.dev/OPEService/OPEService.svc?singleWsdl" -CSVFileName "NewMembers.csv" *> CreateRetailId.txt
# SCRIPT TO TRIGGER PROCESS ISSUE REWARDS

# Initialization - arguments passed to script
param (
    [Parameter(Mandatory=$true)][string]$Uri,
    [Parameter(Mandatory=$true)][string]$CSVFileName
    )
CD
# Initialization
filter timestamp {"$(Get-Date -Format o)"}

#Loading file into memory
$csvFileObject = Import-Csv $CSVFileName

#Initiating web service proxy
$OpeService = New-WebServiceProxy -Uri $Uri -Namespace WebServiceProxy

foreach ($line in $csvFileObject)
{   
   $loyaltyCardNo = $line.loyaltyCardNo   
   $result = $OpeService.CreateRetailId($loyaltyCardNo)

   $LogLine = (timestamp) + ",Card:"+ $loyaltyCardNo +"Response Code:" + $result.ResCode + " RespDescription :" + $result.ResDescription
   Write-Host $LogLine
}
