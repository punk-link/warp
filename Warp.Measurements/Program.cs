using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

//var summary = BenchmarkRunner.Run<MemoryBenchmarkDemo>();
//Console.WriteLine(summary);
Console.ReadLine();


[MemoryDiagnoser]
public class MemoryBenchmarkDemo
{
    /*[GlobalSetup]
    public void Setup()
    {
        for (var i = 0; i < 10_000; i++)
        {
            var result = IdCoder.Encode(Guid.CreateVersion7());
            _guids.Add(result);
        }
    }


    [Benchmark]
    public void Regular()
    {
        foreach (var guid in _guids)
            _ = IdCoder.Decode(guid);
    }


    [Benchmark]
    public void Stack()
    {
        foreach (var guid in _guids)
            _ = IdCoder.Decode1(guid);
    }


    private readonly List<string> _guids = [];*/
}