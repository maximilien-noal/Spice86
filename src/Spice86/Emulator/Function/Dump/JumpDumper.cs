﻿namespace Spice86.Emulator.Function.Dump;

using Errors;

using Newtonsoft.Json;

using Serilog;

using System;
using System.IO;

public class JumpDumper {
    private static readonly ILogger _logger = Program.Logger.ForContext<JumpDumper>();

    public void Dump(JumpHandler jumpHandler, string destinationFilePath) {
        using var printWriter = new StreamWriter(destinationFilePath);
        string jsonString = JsonConvert.SerializeObject(jumpHandler);
        printWriter.WriteLine(jsonString);
    }

    public JumpHandler ReadFromFileOrCreate(string? filePath) {
        if (String.IsNullOrEmpty(filePath)) {
            _logger.Information("No file specified");
            return new JumpHandler();
        }
        if (!File.Exists(filePath)) {
            _logger.Information("File doesn't exists");
            return new JumpHandler();
        }
        try {
            if(string.IsNullOrWhiteSpace(filePath) == false && File.Exists(filePath)) {
                return JsonConvert.DeserializeObject<JumpHandler>(File.ReadAllText(filePath)) ?? new();
            }
            else {
                return new();
            }
        } catch (JsonException e) {
            throw new UnrecoverableException($"File {filePath} is not valid", e);
        }
    }
}