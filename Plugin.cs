using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;

namespace readline_terminal_keybindings;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
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
