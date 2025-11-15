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
        var jsonFileOption = new Option<FileInfo?>("--json")
        {
            Description = "The JSON file containing logging events definitions",
            Required = true
        };

        var constantsOutputOption = new Option<FileInfo?>("--constants")
        {
            Description = "The output path for the LoggingConstants.cs file",
            Required = true
        };

        var messagesOutputOption = new Option<FileInfo?>("--messages")
        { 
            Description = "The output path for the LogMessages.cs file"    
        };
        
        var domainErrorsOutputOption = new Option<FileInfo?>("--domain-errors")
        { 
            Description = "The output path for the DomainErrors.cs file"    
        };

        var rootCommand = new RootCommand("Generates code files from JSON logging definitions")
        {
            jsonFileOption,
            constantsOutputOption,
            messagesOutputOption,
            domainErrorsOutputOption
        };

        rootCommand.SetAction(async parseResults =>
        {
            var jsonFile = parseResults.GetValue(jsonFileOption);
            if (jsonFile is null || !jsonFile.Exists)
            {
                Console.Error.WriteLine($"Error: JSON file not found or invalid.");
                return 1;
            }

            var loggingConfig = LoggingConfigUtilities.LoadFromFile(jsonFile.FullName);
            if (loggingConfig == null)
                return 1;

            var constantsOutput = parseResults.GetValue(constantsOutputOption);
            if (constantsOutput is not null)
                ConstantsGenerator.Generate(loggingConfig, constantsOutput.FullName);

            var messagesOutput = parseResults.GetValue(messagesOutputOption);
            if (messagesOutput is not null)
                LogMessagesGenerator.Generate(loggingConfig, messagesOutput.FullName);

            var domainErrorsOutput = parseResults.GetValue(domainErrorsOutputOption);
            if (domainErrorsOutput is not null)
                DomainErrorGenerator.Generate(loggingConfig, domainErrorsOutput.FullName);

            return 0;
        });
        
        var parseResult = rootCommand.Parse(args);
        return await parseResult.InvokeAsync();
    }
}