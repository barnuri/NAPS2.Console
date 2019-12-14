nuget pack -Properties Platform=x86 -IncludeReferencedProjects
dotnet nuget push NAPS2.Console.1.0.7.nupkg -k oy2cwx77u3wwiy2zokkqfuoju3s7be6enbkptk2icwhfrq -s https://api.nuget.org/v3/index.json
