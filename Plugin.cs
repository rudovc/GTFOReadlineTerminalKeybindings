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

    // Readline key reference:
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

    private static int FindWordStart(string line, int fromIndex)
    {
        var i = fromIndex - 1;
        while (i >= 0 && line[i] == ' ')
            i--;
        while (i >= 0 && line[i] != ' ')
            i--;
        return i + 1;
    }

    private static int FindWordEnd(string line, int fromIndex)
    {
        var i = fromIndex;
        while (i < line.Length && line[i] != ' ')
            i++;
        while (i < line.Length && line[i] == ' ')
            i++;
        return i;
    }

    internal static (string newLine, int newOffset) ApplyKeyBinding(
        string currentLine,
        int caretOffset,
        bool ctrlHeld,
        bool altHeld,
        System.Func<KeyCode, bool> isKeyPressed
    )
    {
        var lineLen = currentLine.Length;
        var curIdx = System.Math.Clamp(lineLen + caretOffset, 0, lineLen);

        if (ctrlHeld)
        {
            if (isKeyPressed(KeyCode.A))
                return (currentLine, -lineLen);

            if (isKeyPressed(KeyCode.E))
                return (currentLine, 0);

            if (isKeyPressed(KeyCode.U))
                return ("", 0);

            if (isKeyPressed(KeyCode.W))
            {
                var wordStart = FindWordStart(currentLine, curIdx);
                return (currentLine.Remove(wordStart, curIdx - wordStart), caretOffset);
            }

            if (isKeyPressed(KeyCode.K))
                return (currentLine[..curIdx], 0);

            return (currentLine, caretOffset);
        }

        if (altHeld)
        {
            if (isKeyPressed(KeyCode.F))
                return (currentLine, FindWordEnd(currentLine, curIdx) - lineLen);

            if (isKeyPressed(KeyCode.B))
            {
                var wordStart = FindWordStart(currentLine, curIdx);
                return (currentLine, wordStart - lineLen);
            }

            return (currentLine, caretOffset);
        }

        return (currentLine, caretOffset);
    }

    [HarmonyPrefix]
    public static void CheckReadlineBindings(LevelGeneration.LG_ComputerTerminal __instance)
    {
        var currentLine = __instance.m_currentLine;

        if (s_previousLine.Length > 0 && currentLine.Length == 0)
        {
            __instance.m_caretBlinkOffsetFromEnd = 0;
        }
        s_previousLine = currentLine;

        var offset = __instance.m_caretBlinkOffsetFromEnd;
        var ctrlHeld = UnityInput.GetKeyInt(KeyCode.LeftControl);
        var altHeld = UnityInput.GetKeyInt(KeyCode.LeftAlt);

        var (newLine, newOffset) = ApplyKeyBinding(
            currentLine,
            offset,
            ctrlHeld,
            altHeld,
            GetKeyDown
        );

        __instance.m_currentLine = newLine;
        __instance.m_caretBlinkOffsetFromEnd = newOffset;

        if (!ctrlHeld && !altHeld)
        {
            s_heldKeys.Clear();
        }
    }
}
