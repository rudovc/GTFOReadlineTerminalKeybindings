using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using LevelGeneration;
using KeyCode = UnityEngine.KeyCode;
using UnityInput = UnityEngine.Input;

namespace ReadlineTerminalKeybindings;

[BepInPlugin(
    "online.121ducks.rudovc.ReadlineTerminalKeybindings",
    "Readline Terminal Keybindings",
    "0.1.0"
)]
[BepInProcess("GTFO.exe")]
public class Plugin : BasePlugin
{
    internal new static ManualLogSource Log = null!;

    // Ctrl: A(start), E(end), F(forward), B(backward), U(clear line), K(kill to end), W(delete word)
    // Alt:  F(forward word), B(backward word)

    public override void Load()
    {
        Log = base.Log;
        Log.LogInfo($"Readline Terminal Keybindings is loaded");

        var harmony = new Harmony("online.121ducks.rudovc.readline_terminal_keybindings");
        harmony.PatchAll();

        Log.LogInfo("Patched Readline bindings successfully");
    }
}

[HarmonyPatch(typeof(LG_ComputerTerminal), "Update")]
public static class ReadlineTerminalKeybindingsPatch
{
    private static readonly HashSet<KeyCode> s_heldKeys = [];
    private static string s_previousLine = "";

    private static bool GetKeyDown(KeyCode key)
    {
        var isPressed = UnityInput.GetKeyDown(key);
        if (isPressed)
            return s_heldKeys.Add(key);

        s_heldKeys.Remove(key);
        return false;
    }

    [HarmonyPrefix]
    public static void CheckReadlineBindings(LG_ComputerTerminal __instance)
    {
        var currentLine = __instance.m_currentLine;

        if (s_previousLine.Length > 0 && currentLine.Length == 0)
            __instance.m_caretBlinkOffsetFromEnd = 0;
        s_previousLine = currentLine;

        var offset = __instance.m_caretBlinkOffsetFromEnd;
        var ctrlHeld = UnityInput.GetKeyDown(KeyCode.LeftControl);
        var altHeld = UnityInput.GetKeyDown(KeyCode.LeftAlt);

        var (newLine, newOffset) = KeyBindingLogic.ApplyKeyBinding(
            currentLine,
            offset,
            ctrlHeld,
            altHeld,
            GetKeyDown
        );

        __instance.m_currentLine = newLine;
        __instance.m_caretBlinkOffsetFromEnd = newOffset;

        if (!ctrlHeld && !altHeld)
            s_heldKeys.Clear();
    }
}
