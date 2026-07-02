using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace readline_terminal_keybindings;

[BepInPlugin(
    "online.121ducks.rudovc.readline_terminal_keybindings",
    "Readline Terminal Keybindings",
    "0.0.1"
)]
[BepInDependency("com.dak.MTFO")]
[BepInProcess("GTFO.exe")]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;

    public readonly char[] CTRL_READLINE_KEYS = ['A', 'E', 'F', 'B', 'L', 'U', 'P', 'N', 'K', 'W'];
    public readonly char[] ALT_READLINE_KEYS = ['F', 'B', 'P', 'N', 'D'];

    public override void Load()
    {
        // Plugin startup logic
        Log = base.Log;
        Log.LogInfo($"Readline Terminal Keybindings is loaded");

        var harmony = new Harmony("online.121ducks.rudovc.readline_terminal_keybindings");
        harmony.PatchAll();

        Log.LogInfo("Patched Readline bindings successfully");
    }
}

[HarmonyPatch(typeof(LevelGeneration.LG_ComputerTerminal), "Update")]
public static class ReadlineTerminalKeybindingsPatch
{
    [HarmonyPrefix]
    public static void CheckCtrlHeldDown(LevelGeneration.LG_ComputerTerminal __instance)
    {
        if (
            BepInEx.Unity.IL2CPP.UnityEngine.Input.GetKeyInt(
                BepInEx.Unity.IL2CPP.UnityEngine.KeyCode.LeftControl
            )
        )
        {
            Plugin.Log.LogDebug("^ is being held down");

            if (
                BepInEx.Unity.IL2CPP.UnityEngine.Input.GetKeyInt(
                    BepInEx.Unity.IL2CPP.UnityEngine.KeyCode.A
                )
            )
            {
                Plugin.Log.LogDebug("^A");

                Plugin.Log.LogDebug(
                    "setting offset from "
                        + __instance.m_caretBlinkOffsetFromEnd
                        + " to "
                        + -__instance.m_currentLine.Length
                );

                __instance.m_caretBlinkOffsetFromEnd = -__instance.m_currentLine.Length;
            }

            if (
                BepInEx.Unity.IL2CPP.UnityEngine.Input.GetKeyInt(
                    BepInEx.Unity.IL2CPP.UnityEngine.KeyCode.E
                )
            )
            {
                Plugin.Log.LogDebug("^E");

                Plugin.Log.LogDebug(
                    "setting offset from " + __instance.m_caretBlinkOffsetFromEnd + " to " + 0
                );

                __instance.m_caretBlinkOffsetFromEnd = 0;
            }

            if (
                BepInEx.Unity.IL2CPP.UnityEngine.Input.GetKeyInt(
                    BepInEx.Unity.IL2CPP.UnityEngine.KeyCode.U
                )
            )
            {
                Plugin.Log.LogDebug("^U");

                Plugin.Log.LogDebug(
                    "Setting current line " + __instance.m_currentLine + " to empty"
                );

                __instance.m_currentLine = "";
            }

            // Bugged right now
            // if (
            //     BepInEx.Unity.IL2CPP.UnityEngine.Input.GetKeyInt(
            //         BepInEx.Unity.IL2CPP.UnityEngine.KeyCode.L
            //     )
            // )
            // {
            //     Plugin.Log.LogDebug("^L");
            //
            //     var commandInterpreter = __instance.m_command;
            //
            //     Plugin.Log.LogDebug(
            //         "Clearing input buffer: " + commandInterpreter.m_inputBuffer + "."
            //     );
            //
            //     commandInterpreter.m_inputBuffer =
            //         new Il2CppSystem.Collections.Generic.List<string>();
            // }
        }

        if (
            BepInEx.Unity.IL2CPP.UnityEngine.Input.GetKeyInt(
                BepInEx.Unity.IL2CPP.UnityEngine.KeyCode.LeftAlt
            )
        )
        {
            string line = __instance.m_currentLine;
            int lineLen = line.Length;
            int curIdx = lineLen + __instance.m_caretBlinkOffsetFromEnd;

            curIdx = System.Math.Clamp(curIdx, 0, lineLen);

            if (
                BepInEx.Unity.IL2CPP.UnityEngine.Input.GetKeyInt(
                    BepInEx.Unity.IL2CPP.UnityEngine.KeyCode.F
                )
            )
            {
                Plugin.Log.LogDebug("^F");

                int i = curIdx;

                while (i < lineLen && line[i] != ' ')
                    i++;
                while (i < lineLen && line[i] == ' ')
                    i++;

                __instance.m_caretBlinkOffsetFromEnd = i - lineLen;
            }

            if (
                BepInEx.Unity.IL2CPP.UnityEngine.Input.GetKeyInt(
                    BepInEx.Unity.IL2CPP.UnityEngine.KeyCode.B
                )
            )
            {
                Plugin.Log.LogDebug("^B");

                int i = curIdx - 1;

                while (i >= 0 && line[i] == ' ')
                    i--;
                while (i >= 0 && line[i] != ' ')
                    i--;

                int target = i + 1;
                __instance.m_caretBlinkOffsetFromEnd = target - lineLen;
            }
        }
    }
}
