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
    private static string s_previousLine = "";

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
    public static void CheckCtrlHeldDown(LevelGeneration.LG_ComputerTerminal __instance)
    {
        var currentLine = __instance.m_currentLine;

        // Reset caret offset on submit/clear (line goes from non-empty to empty)
        if (s_previousLine.Length > 0 && currentLine.Length == 0)
        {
            __instance.m_caretBlinkOffsetFromEnd = 0;
        }
        s_previousLine = currentLine;

        var offset = __instance.m_caretBlinkOffsetFromEnd;

        var ctrlHeld = UnityInput.GetKeyInt(KeyCode.LeftControl);

        if (ctrlHeld)
        {
            Plugin.Log.LogDebug("^ is being held down");

            if (GetKeyDown(KeyCode.A))
            {
                Plugin.Log.LogDebug("^A");

                Plugin.Log.LogDebug(
                    $"setting offset from {offset} to {-currentLine.Length}"
                );

                __instance.m_caretBlinkOffsetFromEnd = -currentLine.Length;
            }

            if (GetKeyDown(KeyCode.E))
            {
                Plugin.Log.LogDebug("^E");

                Plugin.Log.LogDebug(
                    $"setting offset from {offset} to {0}"
                );

                __instance.m_caretBlinkOffsetFromEnd = 0;
            }

            if (GetKeyDown(KeyCode.U))
            {
                Plugin.Log.LogDebug("^U");

                Plugin.Log.LogDebug($"Setting current line {currentLine} to empty");

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

            return;
        }

        var altHeld = UnityInput.GetKeyInt(KeyCode.LeftAlt);

        if (altHeld)
        {
            var lineLen = currentLine.Length;
            var curIdx = System.Math.Clamp(
                lineLen + offset,
                0,
                lineLen
            );

            if (GetKeyDown(KeyCode.F))
            {
                Plugin.Log.LogDebug("A+F");

                var i = curIdx;

                while (i < lineLen && currentLine[i] != ' ')
                    i++;
                while (i < lineLen && currentLine[i] == ' ')
                    i++;

                Plugin.Log.LogDebug(
                    $"setting offset from {offset} to {i - lineLen}"
                );
                __instance.m_caretBlinkOffsetFromEnd = i - lineLen;
            }

            if (GetKeyDown(KeyCode.B))
            {
                Plugin.Log.LogDebug("A+B");

                var i = curIdx - 1;

                while (i >= 0 && currentLine[i] == ' ')
                    i--;
                while (i >= 0 && currentLine[i] != ' ')
                    i--;

                var target = i + 1;
                Plugin.Log.LogDebug(
                    $"setting offset from {offset} to {target - lineLen}"
                );
                __instance.m_caretBlinkOffsetFromEnd = target - lineLen;
            }

            return;
        }

        s_heldKeys.Clear();
    }
}
