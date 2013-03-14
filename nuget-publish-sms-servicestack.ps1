Remove-Item SMS.ServiceStack.*.nupkg

.\.nuget\NuGet.exe pack .\SMS.ServiceStack\SMS.ServiceStack.csproj -Prop Configuration=Release -Build
$package = (Get-ChildItem * -include *.nupkg).Name;
.\.nuget\NuGet.exe push $package $env:NuGetApiKey