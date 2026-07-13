using System;
using System.Collections.Generic;
using Xunit;
using KeyCode = UnityEngine.KeyCode;

namespace ReadlineTerminalKeybindings.Tests;

public class FindWordStartTests
{
    [Theory]
    [InlineData("hello world", 11, 6)]
    [InlineData("hello world", 6, 0)]
    [InlineData("hello world", 5, 0)]
    [InlineData("hello", 3, 0)]
    [InlineData("hello", 5, 0)]
    [InlineData("hello   world", 13, 8)]
    [InlineData("hello", 0, 0)]
    [InlineData("  hello", 2, 0)]
    [InlineData("   ", 3, 0)]
    [InlineData("", 0, 0)]
    public void ReturnsCorrectWordStart(string line, int fromIndex, int expected) =>
        Assert.Equal(expected, KeyBindingLogic.FindWordStart(line, fromIndex));
}

public class FindWordEndTests
{
    [Theory]
    [InlineData("hello world", 0, 6)]
    [InlineData("hello", 2, 5)]
    [InlineData("hello world", 5, 6)]
    [InlineData("hello world", 6, 11)]
    [InlineData("hello   world", 5, 8)]
    [InlineData("hello", 0, 5)]
    [InlineData("", 0, 0)]
    [InlineData("hello", 5, 5)]
    public void ReturnsCorrectWordEnd(string line, int fromIndex, int expected) =>
        Assert.Equal(expected, KeyBindingLogic.FindWordEnd(line, fromIndex));
}

public class ApplyKeyBindingTests
{
    private static Func<KeyCode, bool> PressOnly(params KeyCode[] keys)
    {
        var set = new HashSet<KeyCode>(keys);
        return set.Contains;
    }

    private static Func<KeyCode, bool> PressNone() => _ => false;

    [Fact]
    public void CtrlA_MidLine_MovesOffsetToEnd()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "hello world",
            -5,
            true,
            false,
            PressOnly(KeyCode.A)
        );

        Assert.Equal("hello world", line);
        Assert.Equal(-11, offset);
    }

    [Fact]
    public void CtrlA_AlreadyAtStart_StaysAtStart()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "hello",
            0,
            true,
            false,
            PressOnly(KeyCode.A)
        );

        Assert.Equal("hello", line);
        Assert.Equal(-5, offset);
    }

    [Fact]
    public void CtrlA_EmptyLine_NoChange()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "",
            0,
            true,
            false,
            PressOnly(KeyCode.A)
        );

        Assert.Equal("", line);
        Assert.Equal(0, offset);
    }

    [Fact]
    public void CtrlE_MidLine_MovesOffsetToZero()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "hello",
            -3,
            true,
            false,
            PressOnly(KeyCode.E)
        );

        Assert.Equal("hello", line);
        Assert.Equal(0, offset);
    }

    [Fact]
    public void CtrlE_AlreadyAtEnd_StaysAtEnd()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "hello",
            0,
            true,
            false,
            PressOnly(KeyCode.E)
        );

        Assert.Equal("hello", line);
        Assert.Equal(0, offset);
    }

    [Fact]
    public void CtrlU_ClearsLine()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "hello world",
            -3,
            true,
            false,
            PressOnly(KeyCode.U)
        );

        Assert.Equal("", line);
        Assert.Equal(0, offset);
    }

    [Fact]
    public void CtrlU_AlreadyEmpty_NoChange()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "",
            0,
            true,
            false,
            PressOnly(KeyCode.U)
        );

        Assert.Equal("", line);
        Assert.Equal(0, offset);
    }

    [Fact]
    public void CtrlW_DeleteLastWord()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "hello world",
            0,
            true,
            false,
            PressOnly(KeyCode.W)
        );

        Assert.Equal("hello ", line);
        Assert.Equal(0, offset);
    }

    [Fact]
    public void CtrlW_DeleteWord_MidLine()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "hello world",
            -5,
            true,
            false,
            PressOnly(KeyCode.W)
        );

        Assert.Equal("world", line);
        Assert.Equal(-5, offset);
    }

    [Fact]
    public void CtrlW_DeleteWord_MidWord()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "hello world",
            -3,
            true,
            false,
            PressOnly(KeyCode.W)
        );

        Assert.Equal("hello rld", line);
        Assert.Equal(-3, offset);
    }

    [Fact]
    public void CtrlW_SingleWord_DeletesAll()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "hello",
            0,
            true,
            false,
            PressOnly(KeyCode.W)
        );

        Assert.Equal("", line);
        Assert.Equal(0, offset);
    }

    [Fact]
    public void CtrlW_EmptyLine_NoChange()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "",
            0,
            true,
            false,
            PressOnly(KeyCode.W)
        );

        Assert.Equal("", line);
        Assert.Equal(0, offset);
    }

    [Fact]
    public void CtrlW_LeadingSpaces_DeletesSpaces()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "  hello",
            0,
            true,
            false,
            PressOnly(KeyCode.W)
        );

        Assert.Equal("  ", line);
        Assert.Equal(0, offset);
    }

    [Fact]
    public void CtrlK_MidLine_TruncatesToEnd()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "hello world",
            -5,
            true,
            false,
            PressOnly(KeyCode.K)
        );

        Assert.Equal("hello ", line);
        Assert.Equal(0, offset);
    }

    [Fact]
    public void CtrlK_AtEnd_NoChange()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "hello",
            0,
            true,
            false,
            PressOnly(KeyCode.K)
        );

        Assert.Equal("hello", line);
        Assert.Equal(0, offset);
    }

    [Fact]
    public void CtrlK_AtStart_DeletesAll()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "hello",
            -5,
            true,
            false,
            PressOnly(KeyCode.K)
        );

        Assert.Equal("", line);
        Assert.Equal(0, offset);
    }

    [Fact]
    public void CtrlK_EmptyLine_NoChange()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "",
            0,
            true,
            false,
            PressOnly(KeyCode.K)
        );

        Assert.Equal("", line);
        Assert.Equal(0, offset);
    }

    [Fact]
    public void AltF_MidLine_MovesToEndOfWord()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "hello world",
            -5,
            false,
            true,
            PressOnly(KeyCode.F)
        );

        Assert.Equal("hello world", line);
        Assert.Equal(0, offset);
    }

    [Fact]
    public void AltF_AtWordStart_SkipsWord()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "hello world",
            -11,
            false,
            true,
            PressOnly(KeyCode.F)
        );

        Assert.Equal("hello world", line);
        Assert.Equal(-5, offset);
    }

    [Fact]
    public void AltF_AtEnd_StaysAtEnd()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "hello",
            0,
            false,
            true,
            PressOnly(KeyCode.F)
        );

        Assert.Equal("hello", line);
        Assert.Equal(0, offset);
    }

    [Fact]
    public void AltF_MultipleSpaces_SkipsToNextWord()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "hello   world",
            -8,
            false,
            true,
            PressOnly(KeyCode.F)
        );

        Assert.Equal("hello   world", line);
        Assert.Equal(-5, offset);
    }

    [Fact]
    public void AltB_AtEnd_MovesToStartOfLastWord()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "hello world",
            0,
            false,
            true,
            PressOnly(KeyCode.B)
        );

        Assert.Equal("hello world", line);
        Assert.Equal(-5, offset);
    }

    [Fact]
    public void AltB_MidWord_MovesToStartOfWord()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "hello world",
            -3,
            false,
            true,
            PressOnly(KeyCode.B)
        );

        Assert.Equal("hello world", line);
        Assert.Equal(-5, offset);
    }

    [Fact]
    public void AltB_AtStartOfSecondWord_MovesToFirstWord()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "hello world",
            -5,
            false,
            true,
            PressOnly(KeyCode.B)
        );

        Assert.Equal("hello world", line);
        Assert.Equal(-11, offset);
    }

    [Fact]
    public void AltB_AlreadyAtStart_StaysAtStart()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "hello",
            -5,
            false,
            true,
            PressOnly(KeyCode.B)
        );

        Assert.Equal("hello", line);
        Assert.Equal(-5, offset);
    }

    [Fact]
    public void NoModifier_PassesThroughUnchanged()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "hello world",
            -3,
            false,
            false,
            PressNone()
        );

        Assert.Equal("hello world", line);
        Assert.Equal(-3, offset);
    }

    [Fact]
    public void CtrlHeld_NoKeyPressed_PassesThrough()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding("hello", -2, true, false, PressNone());

        Assert.Equal("hello", line);
        Assert.Equal(-2, offset);
    }

    [Fact]
    public void AltHeld_NoKeyPressed_PassesThrough()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding("hello", -2, false, true, PressNone());

        Assert.Equal("hello", line);
        Assert.Equal(-2, offset);
    }

    [Fact]
    public void NegativeOffsetBeyondLine_ClampsToStart()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding(
            "hello",
            -100,
            true,
            false,
            PressOnly(KeyCode.A)
        );

        Assert.Equal("hello", line);
        Assert.Equal(-5, offset);
    }

    [Fact]
    public void ZeroLengthLine_AnyOffset_NoOp()
    {
        var (line, offset) = KeyBindingLogic.ApplyKeyBinding("", -5, false, false, PressNone());

        Assert.Equal("", line);
        Assert.Equal(-5, offset);
    }
}
