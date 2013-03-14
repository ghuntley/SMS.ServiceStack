@echo Off
@rmdir /Q /S build-packages
@.nuget\nuget.exe install MSBuildTasks -Version 1.4.0.45 -ExcludeVersion -OutputDirectory .\build-packages
@.nuget\nuget.exe install StyleCop.MSBuild -Version 4.7.35.0 -ExcludeVersion -OutputDirectory .\build-packages
@.nuget\nuget.exe install Nunit.Runners -Version 2.6.0.12051 -ExcludeVersion -OutputDirectory .\build-packages
@.nuget\nuget.exe install OpenCover -Version 4.0.519 -ExcludeVersion -OutputDirectory .\build-packages
@.nuget\nuget.exe install ReportGenerator -Version 1.5.0.0 -ExcludeVersion -OutputDirectory .\build-packages