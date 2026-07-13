using System;
using KeyCode = UnityEngine.KeyCode;

namespace ReadlineTerminalKeybindings;

internal static class KeyBindingLogic
{
    internal static int FindWordStart(string line, int fromIndex)
    {
        var i = fromIndex - 1;
        while (i >= 0 && line[i] == ' ')
            i--;
        while (i >= 0 && line[i] != ' ')
            i--;
        return i + 1;
    }

    internal static int FindWordEnd(string line, int fromIndex)
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
        Func<KeyCode, bool> isKeyPressed
    )
    {
        var lineLen = currentLine.Length;
        var curIdx = Math.Clamp(lineLen + caretOffset, 0, lineLen);

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
}
