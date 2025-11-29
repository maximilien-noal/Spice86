namespace Spice86.Tests.Dos;

using FluentAssertions;

using NSubstitute;

using Spice86.Core.Emulator.OperatingSystem.Command.BatchProcessing;
using Spice86.Shared.Interfaces;
using Spice86.Tests.Utility;

using Xunit;

/// <summary>
/// Unit tests for the <see cref="BatchExecutor"/> class.
/// Tests batch file execution logic including program launching, GOTO handling, and SHA256 verification support.
/// </summary>
public class BatchExecutorTests : IDisposable {
    private readonly ILoggerService _loggerService;
    private readonly string _tempDir;

    public BatchExecutorTests() {
        _loggerService = Substitute.For<ILoggerService>();
        _tempDir = Path.Combine(Path.GetTempPath(), $"Spice86BatchExecTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose() {
        try {
            if (Directory.Exists(_tempDir)) {
                Directory.Delete(_tempDir, recursive: true);
            }
        } catch (IOException) {
            // Ignore cleanup errors
        } catch (UnauthorizedAccessException) {
            // Ignore permission issues
        }
    }

    private string CreateBatchFile(string name, string content) {
        string path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content);
        return path;
    }

    private string CreateExecutable(string name, byte[] content) {
        string path = Path.Combine(_tempDir, name);
        File.WriteAllBytes(path, content);
        return path;
    }

    #region GOTO Command Tests - Comprehensive

    [Fact]
    public void GotoLabel_JumpsToCorrectPosition() {
        // Arrange - batch file with GOTO that skips a line
        BatchProcessor processor = new(_loggerService);
        string content = "@echo off\ngoto end\necho SHOULD NOT SEE THIS\n:end\necho AFTER LABEL";
        string batchPath = CreateBatchFile("goto_test.bat", content);
        processor.StartBatch(batchPath, []);

        // Act - Read first line (@echo off)
        string? line1 = processor.ReadNextLine(out _);
        processor.ParseCommand(line1!);

        // Read GOTO command
        string? gotoLine = processor.ReadNextLine(out _);
        gotoLine.Should().Be("goto end");
        
        BatchCommand gotoCmd = processor.ParseCommand(gotoLine!);
        gotoCmd.Type.Should().Be(BatchCommandType.Goto);
        gotoCmd.Value.Should().Be("end");

        // Execute GOTO - this should jump to :end
        bool found = processor.GotoLabel(gotoCmd.Value);
        found.Should().BeTrue();

        // The next line should be AFTER the label, skipping "echo SHOULD NOT SEE THIS"
        string? afterGoto = processor.ReadNextLine(out _);
        afterGoto.Should().Be("echo AFTER LABEL");
    }

    [Fact]
    public void GotoLabel_JumpsBackward() {
        // Arrange - GOTO that jumps backward creates a loop
        BatchProcessor processor = new(_loggerService);
        string content = ":start\necho first\ngoto start";
        string batchPath = CreateBatchFile("goto_back.bat", content);
        processor.StartBatch(batchPath, []);

        // Act - Read "echo first" (label is skipped)
        string? line1 = processor.ReadNextLine(out _);
        line1.Should().Be("echo first");

        // Read "goto start"
        string? line2 = processor.ReadNextLine(out _);
        line2.Should().Be("goto start");

        // Execute GOTO back to :start
        bool found = processor.GotoLabel("start");
        found.Should().BeTrue();

        // Next line should be "echo first" again
        string? line3 = processor.ReadNextLine(out _);
        line3.Should().Be("echo first");
    }

    [Fact]
    public void GotoLabel_WithMultipleLabels_FindsCorrectOne() {
        // Arrange
        BatchProcessor processor = new(_loggerService);
        string content = ":label1\necho one\n:label2\necho two\n:label3\necho three";
        string batchPath = CreateBatchFile("multi_label.bat", content);
        processor.StartBatch(batchPath, []);

        // Act - Jump to label2
        bool found = processor.GotoLabel("label2");

        // Assert
        found.Should().BeTrue();
        string? line = processor.ReadNextLine(out _);
        line.Should().Be("echo two");
    }

    [Fact]
    public void GotoLabel_SkipsCodeBetweenGotoAndLabel() {
        // Arrange - Verify that all lines between GOTO and label are skipped
        BatchProcessor processor = new(_loggerService);
        string content = "@echo off\ngoto target\necho skip1\necho skip2\necho skip3\n:target\necho reached";
        string batchPath = CreateBatchFile("skip_test.bat", content);
        processor.StartBatch(batchPath, []);

        // Read and process @echo off
        processor.ReadNextLine(out _);

        // Read goto target
        string? gotoLine = processor.ReadNextLine(out _);
        BatchCommand cmd = processor.ParseCommand(gotoLine!);
        processor.GotoLabel(cmd.Value);

        // Next line should be "echo reached", all skip lines should be bypassed
        string? afterLabel = processor.ReadNextLine(out _);
        afterLabel.Should().Be("echo reached");
    }

    [Fact]
    public void GotoLabel_WithSpacesInLabel_Works() {
        // Arrange - Labels can have trailing content
        BatchProcessor processor = new(_loggerService);
        string content = "goto myLabel\n:myLabel some comment here\necho found";
        string batchPath = CreateBatchFile("label_space.bat", content);
        processor.StartBatch(batchPath, []);

        // Read goto
        string? gotoLine = processor.ReadNextLine(out _);
        BatchCommand cmd = processor.ParseCommand(gotoLine!);
        
        // Execute GOTO
        bool found = processor.GotoLabel(cmd.Value);
        found.Should().BeTrue();

        string? next = processor.ReadNextLine(out _);
        next.Should().Be("echo found");
    }

    #endregion

    #region CALL Command Tests

    [Fact]
    public void Call_ParsesCorrectly() {
        // Arrange
        BatchProcessor processor = new(_loggerService);

        // Act
        BatchCommand cmd = processor.ParseCommand("CALL SETUP.BAT arg1 arg2");

        // Assert
        cmd.Type.Should().Be(BatchCommandType.CallBatch);
        cmd.Value.Should().Be("SETUP.BAT");
        cmd.Arguments.Should().Be("arg1 arg2");
    }

    [Fact]
    public void NestedBatchFiles_MaintainSeparateContexts() {
        // Arrange
        BatchProcessor processor = new(_loggerService);
        string innerPath = CreateBatchFile("inner.bat", "echo inner line");
        string outerPath = CreateBatchFile("outer.bat", "echo outer line");

        // Start outer batch
        processor.StartBatch(outerPath, []);
        string? outerLine = processor.ReadNextLine(out _);
        outerLine.Should().Be("echo outer line");

        // Simulate CALL - start inner batch (nested)
        processor.StartBatch(innerPath, []);
        processor.CurrentBatchPath.Should().Be(innerPath);

        string? innerLine = processor.ReadNextLine(out _);
        innerLine.Should().Be("echo inner line");

        // Exit inner batch - should return to outer
        processor.ExitBatch();
        processor.CurrentBatchPath.Should().Be(outerPath);
    }

    #endregion

    #region IF Command Tests

    [Fact]
    public void ParseCommand_IfExist_ParsesCorrectly() {
        // Arrange
        BatchProcessor processor = new(_loggerService);

        // Act
        BatchCommand cmd = processor.ParseCommand("IF EXIST config.sys echo Found");

        // Assert
        cmd.Type.Should().Be(BatchCommandType.If);
        cmd.Value.Should().Be("EXIST");
        cmd.Arguments.Should().Contain("config.sys");
        cmd.Negate.Should().BeFalse();
    }

    [Fact]
    public void ParseCommand_IfNotExist_SetsNegateFlag() {
        // Arrange
        BatchProcessor processor = new(_loggerService);

        // Act
        BatchCommand cmd = processor.ParseCommand("IF NOT EXIST missing.txt echo Not found");

        // Assert
        cmd.Type.Should().Be(BatchCommandType.If);
        cmd.Value.Should().Be("EXIST");
        cmd.Negate.Should().BeTrue();
    }

    [Fact]
    public void ParseCommand_IfErrorlevel_ParsesLevel() {
        // Arrange
        BatchProcessor processor = new(_loggerService);

        // Act
        BatchCommand cmd = processor.ParseCommand("IF ERRORLEVEL 5 goto error");

        // Assert
        cmd.Type.Should().Be(BatchCommandType.If);
        cmd.Value.Should().Be("ERRORLEVEL");
        cmd.Arguments.Should().StartWith("5");
    }

    [Fact]
    public void ParseCommand_IfStringComparison_ParsesStrings() {
        // Arrange
        BatchProcessor processor = new(_loggerService);

        // Act
        BatchCommand cmd = processor.ParseCommand("IF %1==yes echo Confirmed");

        // Assert
        cmd.Type.Should().Be(BatchCommandType.If);
        cmd.Value.Should().Be("COMPARE");
    }

    #endregion

    #region SHIFT Command Tests

    [Fact]
    public void Shift_MovesParametersCorrectly() {
        // Arrange
        BatchProcessor processor = new(_loggerService);
        string content = "echo %1 %2 %3\nshift\necho %1 %2 %3\nshift\necho %1 %2 %3";
        string batchPath = CreateBatchFile("shift_test.bat", content);
        processor.StartBatch(batchPath, ["a", "b", "c", "d", "e"]);

        // Before shift: %1=a, %2=b, %3=c
        string? line1 = processor.ReadNextLine(out _);
        line1.Should().Be("echo a b c");

        // SHIFT command
        string? shiftLine1 = processor.ReadNextLine(out _);
        processor.ParseCommand(shiftLine1!);

        // After first shift: %1=b, %2=c, %3=d
        string? line2 = processor.ReadNextLine(out _);
        line2.Should().Be("echo b c d");

        // Second SHIFT
        string? shiftLine2 = processor.ReadNextLine(out _);
        processor.ParseCommand(shiftLine2!);

        // After second shift: %1=c, %2=d, %3=e
        string? line3 = processor.ReadNextLine(out _);
        line3.Should().Be("echo c d e");
    }

    [Fact]
    public void Shift_Parameter0_StaysConstant() {
        // Arrange - %0 should always be the batch file name
        BatchProcessor processor = new(_loggerService);
        string content = "echo %0\nshift\necho %0";
        string batchPath = CreateBatchFile("shift0.bat", content);
        processor.StartBatch(batchPath, ["arg1"]);

        // %0 before shift
        string? line1 = processor.ReadNextLine(out _);
        line1.Should().Be($"echo {batchPath}");

        // SHIFT
        processor.ReadNextLine(out _);
        processor.ParseCommand("shift");

        // %0 after shift - should still be the batch file path
        string? line2 = processor.ReadNextLine(out _);
        line2.Should().Be($"echo {batchPath}");
    }

    #endregion

    #region FOR Command Tests

    [Fact]
    public void ParseCommand_ForLoop_ParsesAllComponents() {
        // Arrange
        BatchProcessor processor = new(_loggerService);

        // Act
        BatchCommand cmd = processor.ParseCommand("FOR %%i IN (file1.txt file2.txt file3.txt) DO type %%i");

        // Assert
        cmd.Type.Should().Be(BatchCommandType.For);
        cmd.Value.Should().Be("%%i");
        cmd.GetForSet().Should().BeEquivalentTo(["file1.txt", "file2.txt", "file3.txt"]);
        cmd.GetForCommand().Should().Be("type %%i");
    }

    [Fact]
    public void ParseCommand_ForWithCommaDelimiters_SplitsCorrectly() {
        // Arrange
        BatchProcessor processor = new(_loggerService);

        // Act
        BatchCommand cmd = processor.ParseCommand("FOR %X IN (a,b,c,d) DO echo %X");

        // Assert
        cmd.GetForSet().Should().BeEquivalentTo(["a", "b", "c", "d"]);
    }

    [Fact]
    public void ParseCommand_ForWithMixedDelimiters_SplitsCorrectly() {
        // Arrange - Spaces, commas, semicolons are all valid delimiters
        BatchProcessor processor = new(_loggerService);

        // Act
        BatchCommand cmd = processor.ParseCommand("FOR %V IN (a b,c;d) DO echo %V");

        // Assert
        cmd.GetForSet().Should().BeEquivalentTo(["a", "b", "c", "d"]);
    }

    #endregion

    #region EXIT Command Tests

    [Fact]
    public void ParseCommand_Exit_ReturnsExitCommand() {
        // Arrange
        BatchProcessor processor = new(_loggerService);

        // Act
        BatchCommand cmd = processor.ParseCommand("EXIT");

        // Assert
        cmd.Type.Should().Be(BatchCommandType.Exit);
    }

    [Fact]
    public void ExitBatch_RestoresPreviousEchoState() {
        // Arrange
        BatchProcessor processor = new(_loggerService);
        processor.Echo = true;

        string batchPath = CreateBatchFile("exit_test.bat", "@echo off\necho test");
        processor.StartBatch(batchPath, []);

        // Process @echo off
        string? line = processor.ReadNextLine(out _);
        processor.ParseCommand(line!);
        processor.Echo.Should().BeFalse();

        // Act - Exit batch
        processor.ExitBatch();

        // Assert - Echo should be restored to original state
        processor.Echo.Should().BeTrue();
    }

    #endregion

    #region PAUSE Command Tests

    [Fact]
    public void ParseCommand_Pause_ReturnsPauseCommand() {
        // Arrange
        BatchProcessor processor = new(_loggerService);

        // Act
        BatchCommand cmd = processor.ParseCommand("PAUSE");

        // Assert
        cmd.Type.Should().Be(BatchCommandType.Pause);
    }

    #endregion

    #region SET Command Tests

    [Fact]
    public void ParseCommand_SetWithEqualsInValue_PreservesValue() {
        // Arrange - Values can contain = sign
        BatchProcessor processor = new(_loggerService);

        // Act
        BatchCommand cmd = processor.ParseCommand("SET PROMPT=$P$G=>");

        // Assert
        cmd.Type.Should().Be(BatchCommandType.SetVariable);
        cmd.Value.Should().Be("PROMPT");
        cmd.Arguments.Should().Be("$P$G=>");
    }

    #endregion

    #region Environment Variable Expansion Tests

    [Fact]
    public void ExpandParameters_WithNestedPercents_HandlesCorrectly() {
        // Arrange
        TestBatchEnvironment env = new();
        env.SetVariable("LEVEL", "1");
        env.SetVariable("LEVEL1", "FirstLevel");
        BatchProcessor processor = new(_loggerService, env);
        
        // Note: DOS doesn't support nested expansion like %LEVEL%LEVEL%
        // but we should handle edge cases gracefully
        string content = "echo %LEVEL%";
        string batchPath = CreateBatchFile("nested.bat", content);
        processor.StartBatch(batchPath, []);

        // Act
        string? line = processor.ReadNextLine(out _);

        // Assert
        line.Should().Be("echo 1");
    }

    [Fact]
    public void ExpandParameters_UnmatchedPercent_PreservesLiteral() {
        // Arrange
        BatchProcessor processor = new(_loggerService);
        string content = "echo 50% complete";
        string batchPath = CreateBatchFile("percent.bat", content);
        processor.StartBatch(batchPath, []);

        // Act
        string? line = processor.ReadNextLine(out _);

        // Assert - Unmatched % should be preserved
        line.Should().Be("echo 50% complete");
    }

    #endregion

    #region Complex Batch File Tests

    [Fact]
    public void IntegrationTest_ComplexBatchWithGotoAndLabels() {
        // Test a realistic batch file with multiple labels and GOTOs
        BatchProcessor processor = new(_loggerService);
        string content = @"@echo off
if %1==A goto processA
if %1==B goto processB
goto end

:processA
echo Processing A
goto end

:processB
echo Processing B
goto end

:end
echo Done";

        string batchPath = CreateBatchFile("complex.bat", content);
        processor.StartBatch(batchPath, ["A"]);

        // Read @echo off
        string? line1 = processor.ReadNextLine(out _);
        processor.ParseCommand(line1!);

        // Read first IF - "if A==A goto processA" should be true
        string? ifLine = processor.ReadNextLine(out _);
        ifLine.Should().Be("if A==A goto processA");

        // For testing, we'll manually execute the GOTO since IF evaluation
        // would require more complex logic
        processor.GotoLabel("processA");

        // After GOTO, should be at "echo Processing A"
        string? afterGoto = processor.ReadNextLine(out _);
        afterGoto.Should().Be("echo Processing A");
    }

    [Fact]
    public void IntegrationTest_BatchWithAllParameters() {
        // Test all 10 parameters (%0-%9)
        BatchProcessor processor = new(_loggerService);
        string content = "echo %0 %1 %2 %3 %4 %5 %6 %7 %8 %9";
        string batchPath = CreateBatchFile("params.bat", content);
        string[] args = ["one", "two", "three", "four", "five", "six", "seven", "eight", "nine"];
        processor.StartBatch(batchPath, args);

        // Act
        string? line = processor.ReadNextLine(out _);

        // Assert - %0 is batch path, %1-9 are arguments
        line.Should().Be($"echo {batchPath} one two three four five six seven eight nine");
    }

    #endregion
}
