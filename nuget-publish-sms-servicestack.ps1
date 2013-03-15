Remove-Item SMS.ServiceStack.*.nupkg

.\.nuget\NuGet.exe pack -Symbols .\SMS.ServiceStack\SMS.ServiceStack.csproj -Prop Configuration=Release -Build
$package = (Get-ChildItem * -include *.nupkg | Sort-Object Name)[0].Name;
.\.nuget\NuGet.exe push $package $env:NuGetApiKey