### .\WebEnrollforPrimary.ps1 -Uri "http://opewdevcgy002.network.dev/OPEService/OPEService.svc?singleWsdl" -CSVFileName "WebEnrollforPrimary.csv" *> WebEnrollforPrimary.txt

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
$OpeService = New-WebServiceProxy -Uri $Uri 

$ObjectType = $OpeService.GetType().Namespace

# The Object 'PrimaryMemberEnrollmentInfo' already exposed by WebInterface. So we don't need to create Type again. 
# Refer the object exposed by using the WebService Proxy.
$MemberObj = New-Object($ObjectType + '.PrimaryMemberEnrollmentInfo')
'loyaltyCardNo' >> 'CreateRetailId.csv'
foreach ($line in $csvFileObject)
{
       $MemberObj.FirstName = $line.FirstName
       $MemberObj.LastName = $line.LastName
       $MemberObj.Email = $line.Email
       $MemberObj.City = $line.City
       $MemberObj.Channel = $line.Channel
       $MemberObj.Province = $line.Province
       $MemberObj.PostalCode = $line.PostalCode
         
       $result = $OpeService.WebEnrollForPrimary($MemberObj)

       $LogLine = (timestamp) + ",FirstName:"+ $MemberObj.FirstName + " LastName : "  + $MemberObj.LastName +" CardNo :" + $result.Result +" RespDescription :" + $result.ResDescription
   	$result.Result >> 'CreateRetailId.csv'
       Write-Host $LogLine
}
