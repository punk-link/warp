using System.CommandLine;
using Warp.CodeGen.Generators;
using Warp.CodeGen.Utilities;

namespace Warp.CodeGen;

/// <summary>
/// Entry point for the code generation process
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var jsonFileOption = new Option<FileInfo?>(name: "--json", description: "The JSON file containing logging events definitions")
        {
            IsRequired = true
        };

        var constantsOutputOption = new Option<FileInfo?>(name: "--constants", description: "The output path for the LoggingConstants.cs file")
        {
            IsRequired = true
        };

        var messagesOutputOption = new Option<FileInfo?>(name: "--messages", description: "The output path for the LogMessages.cs file");
        
        var domainErrorsOutputOption = new Option<FileInfo?>(name: "--domain-errors", description: "The output path for the DomainErrors.cs file");

        var rootCommand = new RootCommand("Generates code files from JSON logging definitions");
        rootCommand.AddOption(jsonFileOption);
        rootCommand.AddOption(constantsOutputOption);
        rootCommand.AddOption(messagesOutputOption);
        rootCommand.AddOption(domainErrorsOutputOption);

        rootCommand.SetHandler((jsonFile, constantsOutput, messagesOutput, domainErrorsOutput) =>
        {
            if (jsonFile is null || !jsonFile.Exists)
            {
                Console.Error.WriteLine($"Error: JSON file not found or invalid.");
                return Task.FromResult(1);
            }

            var loggingConfig = LoggingConfigUtilities.LoadFromFile(jsonFile.FullName);
            if (loggingConfig == null)
                return Task.FromResult(1);

            if (constantsOutput is not null)
                ConstantsGenerator.Generate(loggingConfig, constantsOutput.FullName);

            if (messagesOutput is not null)
                LogMessagesGenerator.Generate(loggingConfig, messagesOutput.FullName);
                
            if (domainErrorsOutput is not null)
                DomainErrorGenerator.Generate(loggingConfig, domainErrorsOutput.FullName);

            return Task.FromResult(0);
        }, jsonFileOption, constantsOutputOption, messagesOutputOption, domainErrorsOutputOption);

        return await rootCommand.InvokeAsync(args);
    }
}