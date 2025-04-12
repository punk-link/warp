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
        aes.KeySize = 256;
        aes.GenerateKey();
        
        if (base64)
        {
            var keyBase64 = Convert.ToBase64String(aes.Key);
            Console.WriteLine($"Key (Base64): {keyBase64}");
            
            if (!string.IsNullOrEmpty(output))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                File.WriteAllText(output, keyBase64);
                Console.WriteLine($"Key saved to {output}");
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(output))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                File.WriteAllBytes(output, aes.Key);
                Console.WriteLine($"Key saved to {output}");
            }
            else
            {
                Console.WriteLine("Binary key generated but not saved (no output path specified)");
            }
        }
    }
}
