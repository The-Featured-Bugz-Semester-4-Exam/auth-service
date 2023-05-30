#/usr/bin/bash

export Secret="mySecretIsASecret"
export Issuer="myIssuerIsAnIssue"
export apiGetUser="localhost:5081/api/GetUser"
#export server="localhost"
#export port="27017"
#export connectionString="mongodb://localhost:27017"
#export database="Auth"
#export collection="authCol"
#echo $database connectionString
#dotnet run server="$server" port="$port" export connectionString"mongodb://localhost:27017" export database="Auth"
dotnet run Issuer="$Issuer" Secret="$Secret" apiGetUser="$apiGetUser" ValidAudience="http://localhost"



#/    ". ./startup.sh"