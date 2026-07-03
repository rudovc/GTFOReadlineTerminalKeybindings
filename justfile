init:
    dotnet restore
build:
    dotnet build --configuration Debug
release:
    dotnet build --configuration Release
bump version:
    sed -i 's/<Version>.*</<Version>{{ version }}</' ReadlineTerminalKeybindings.csproj
    sed -i '/"Readline Terminal Keybindings"/{n;s/"[^"]*"/"{{ version }}"/}' Plugin.cs
    sed -i 's/"version_number": ".*"/"version_number": "{{ version }}"/' package/manifest.json

    git add ReadlineTerminalKeybindings.csproj Plugin.cs package/manifest.json
    git commit -m "v{{ version }}"
    git tag "v{{ version }}"
logs:
    tail -f $HOME/.config/r2modmanPlus-local/GTFO/profiles/Development/BepInEx/LogOutput.log
