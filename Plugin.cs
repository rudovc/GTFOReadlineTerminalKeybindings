using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;

namespace readline_terminal_keybindings;

[BepInPlugin(
    "online.121ducks.rudovc.readline_terminal_keybindings",
    "Readline Terminal Keybindgins",
    "0.0.1"
)]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;

    public override void Load()
    {
        // Plugin startup logic
        Log = base.Log;
        Log.LogInfo($"Readline Terminal Keybdingins is loaded");
    }
}
