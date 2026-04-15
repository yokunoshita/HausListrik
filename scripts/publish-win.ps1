param(
    [string]$Runtime = "win-x64"
)

$project = "src/BatteryBuddy.App/BatteryBuddy.App.csproj"

dotnet publish $project `
    -c Release `
    -r $Runtime `
    --self-contained true `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true
