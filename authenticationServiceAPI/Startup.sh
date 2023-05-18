#/usr/bin/bash

export Secret="mySecretIsASecret"
export Issuer="myIssuerIsAnIssue"
#export server="localhost"
#export port="27017"
#export connectionString="mongodb://localhost:27017"
#export database="Auth"
#export collection="authCol"
#echo $database connectionString
#dotnet run server="$server" port="$port" export connectionString"mongodb://localhost:27017" export database="Auth"
export mySecret="$mySecret" 
export myIssuer="$myIssuer"
dotnet run Issuer="$myIssuer" Secret="$mySecret"



#/    ". ./startup.sh"