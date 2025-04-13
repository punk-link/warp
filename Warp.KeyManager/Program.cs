using System.CommandLine;
using System.Security.Cryptography;

namespace Warp.KeyManager;

class Program
{
    static int Main(string[] args)
    {
        var rootCommand = new RootCommand("KeyDb encryption key management tool");
        
        var outputOption = new Option<string>("--output", "Path where to store the key");
        var base64Option = new Option<bool>("--base64", "Output the key as Base64 string");
        
        var generateCommand = new Command("generate", "Generate a new encryption key");
        generateCommand.AddOption(outputOption);
        generateCommand.AddOption(base64Option);
        
        generateCommand.SetHandler(GenerateKey, outputOption, base64Option);
        
        rootCommand.Add(generateCommand);
        
        return rootCommand.Invoke(args);
    }
    

    static void GenerateKey(string output, bool base64)
    {
        using var aes = Aes.Create();
        // Generate a deterministic key for development (NOT FOR PRODUCTION)
        // A 32-byte array to be used as AES-256 key. 256 bits = 32 bytes
        aes.KeySize = 256;
        aes.GenerateKey();

        if (string.IsNullOrEmpty(output))
        {
            Console.WriteLine("No output path specified. Key will not be saved.");
            return;
        }
        
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        if (base64)
        {
            var keyBase64 = Convert.ToBase64String(aes.Key);
            File.WriteAllText(output, keyBase64);
            Console.WriteLine($"Key (Base64): {keyBase64}");
        }
        else
        {
            File.WriteAllBytes(output, aes.Key);
        }

        Console.WriteLine($"Key saved to {output}");
    }
}
