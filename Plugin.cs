using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using KeyCode = BepInEx.Unity.IL2CPP.UnityEngine.KeyCode;
using UnityInput = BepInEx.Unity.IL2CPP.UnityEngine.Input;

namespace ReadlineTerminalKeybindings;

[BepInPlugin(
    "online.121ducks.rudovc.ReadlineTerminalKeybindings",
    "Readline Terminal Keybindings",
    "0.0.1"
)]
[BepInDependency("com.dak.MTFO")]
[BepInProcess("GTFO.exe")]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log = null!;

    // Readline key reference (for future use):
    // Ctrl: A(move to start), E(move to end), F(forward), B(backward),
    //       L(clear screen), U(clear line), P(prev), N(next), K(delete to end), W(delete word)
    // Alt:  F(forward word), B(backward word), P(prev), N(next), D(delete word)

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
    private static readonly HashSet<KeyCode> s_heldKeys = [];

    private static bool GetKeyDown(KeyCode key)
    {
        var isPressed = UnityInput.GetKeyInt(key);
        if (isPressed)
        {
            return s_heldKeys.Add(key);
        }

        s_heldKeys.Remove(key);
        return false;
    }

    [HarmonyPrefix]
    public static void CheckCtrlHeldDown(LevelGeneration.LG_ComputerTerminal instance)
    {
        var ctrlHeld = UnityInput.GetKeyInt(KeyCode.LeftControl);

        if (ctrlHeld)
        {
            Plugin.Log.LogDebug("^ is being held down");

            if (GetKeyDown(KeyCode.A))
            {
                Plugin.Log.LogDebug("^A");

                Plugin.Log.LogDebug(
                    $"setting offset from {instance.m_caretBlinkOffsetFromEnd} to {-instance.m_currentLine.Length}"
                );

                instance.m_caretBlinkOffsetFromEnd = -instance.m_currentLine.Length;
            }

            if (GetKeyDown(KeyCode.E))
            {
                Plugin.Log.LogDebug("^E");

                Plugin.Log.LogDebug($"setting offset from {instance.m_caretBlinkOffsetFromEnd} to {0}");

                instance.m_caretBlinkOffsetFromEnd = 0;
            }

            if (GetKeyDown(KeyCode.U))
            {
                Plugin.Log.LogDebug("^U");

                Plugin.Log.LogDebug($"Setting current line {instance.m_currentLine} to empty");

                instance.m_currentLine = "";
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

            return;
        }

        // Causes a weird bug where the cursor jumps randomly after being used once
        // bool altHeld = BepInEx.Unity.IL2CPP.UnityEngine.Input.GetKeyInt(
        //     BepInEx.Unity.IL2CPP.UnityEngine.KeyCode.LeftAlt
        // );
        //
        // if (altHeld)
        // {
        //     string line = __instance.m_currentLine;
        //     int lineLen = line.Length;
        //     int curIdx = lineLen + __instance.m_caretBlinkOffsetFromEnd;
        //
        //     curIdx = System.Math.Clamp(curIdx, 0, lineLen);
        //
        //     if (GetKeyDown(BepInEx.Unity.IL2CPP.UnityEngine.KeyCode.F))
        //     {
        //         Plugin.Log.LogDebug("^F");
        //
        //         int i = curIdx;
        //
        //         while (i < lineLen && line[i] != ' ')
        //             i++;
        //         while (i < lineLen && line[i] == ' ')
        //             i++;
        //
        //         __instance.m_caretBlinkOffsetFromEnd = i - lineLen;
        //     }
        //
        //     if (GetKeyDown(BepInEx.Unity.IL2CPP.UnityEngine.KeyCode.B))
        //     {
        //         Plugin.Log.LogDebug("^B");
        //
        //         int i = curIdx - 1;
        //
        //         while (i >= 0 && line[i] == ' ')
        //             i--;
        //         while (i >= 0 && line[i] != ' ')
        //             i--;
        //
        //         int target = i + 1;
        //         __instance.m_caretBlinkOffsetFromEnd = target - lineLen;
        //     }
        //
        //     return;
        // }

        s_heldKeys.Clear();
    }
}
