init:
    dotnet restore
build:
    dotnet build
logs:
    tail -f $HOME/.config/r2modmanPlus-local/GTFO/profiles/Development/BepInEx/LogOutput.log
