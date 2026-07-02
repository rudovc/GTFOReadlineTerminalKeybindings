init:
    dotnet restore
build:
    dotnet build
release:
    dotnet build --configuration Release
logs:
    tail -f $HOME/.config/r2modmanPlus-local/GTFO/profiles/Development/BepInEx/LogOutput.log
