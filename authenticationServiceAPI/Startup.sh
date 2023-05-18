#/usr/bin/bash

export Secret="mySecretIsASecret"
export Issuer="myIssuerIsAnIssue"
#export server="localhost"
#export port="27017"
#export connectionString=""
#export database="Auction"
#export collection="userCol"
#echo $database connectionString
#dotnet run server="$server" port="$port" export connectionString"" export database="Auction"
export mySecret="$mySecret" 
export myIssuer="$myIssuer"
dotnet run Issuer="$myIssuer" Secret="$mySecret"



#/    ". ./startup.sh"