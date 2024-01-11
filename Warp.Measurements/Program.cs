using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Warp.WebApp.Services;


var summary = BenchmarkRunner.Run<MemoryBenchmarkDemo>();
Console.WriteLine(summary);
Console.ReadLine();


[MemoryDiagnoser]
public class MemoryBenchmarkDemo
{
    const string BenchmarkText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed felis lacus, varius in est et, convallis fringilla ipsum. Sed vel purus mauris. Phasellus quis massa a felis egestas condimentum at fermentum ipsum. Curabitur eleifend libero id augue accumsan maximus. Mauris condimentum lacus nibh, lobortis consequat tortor pharetra porta. Mauris sollicitudin nisl non accumsan mollis. Vestibulum dui mauris, aliquam feugiat maximus sit amet, ultrices non nisi. Duis sed euismod justo. Praesent sodales eu tortor vel egestas. Duis ultrices pellentesque nunc vitae congue. Vivamus sed luctus turpis, vitae vestibulum nisl. Nulla id ipsum venenatis, condimentum purus in, pellentesque ex. Nullam varius condimentum est, eu tempus urna egestas nec. Suspendisse porttitor mauris vitae laoreet faucibus.\r\n\r\nCurabitur sollicitudin urna sed mauris feugiat, eu malesuada lacus posuere. Nullam interdum nisi id sem volutpat mattis. Phasellus a sagittis lacus. Nullam sed porta ante. Fusce maximus justo magna, in dapibus mauris imperdiet volutpat. Fusce suscipit, diam sed vehicula commodo, nisi nisl rutrum metus, sit amet finibus orci arcu at neque. Pellentesque a orci turpis. Pellentesque molestie justo sit amet nisi iaculis, quis mollis justo bibendum. Ut luctus ipsum id lacus pulvinar ultrices. Maecenas pharetra pharetra cursus. Vestibulum lacinia egestas efficitur. Mauris fringilla quis ipsum vel efficitur. Vivamus vitae euismod arcu. Vestibulum nec libero vitae leo ullamcorper pretium.\r\n\r\nQuisque imperdiet, velit eget viverra porttitor, odio ligula egestas felis, et maximus lectus dui ultricies ligula. Suspendisse malesuada tristique mi, vitae consequat turpis molestie ut. Sed ornare erat facilisis turpis facilisis dictum. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Sed vel elit ante. Suspendisse sed nisi eu tortor facilisis hendrerit. Proin mauris nisl, maximus a sagittis eu, consequat et turpis. Maecenas ut enim eu diam suscipit volutpat. Sed odio tortor, sollicitudin vitae luctus a, rhoncus id urna. Mauris aliquet ipsum augue, vel ultrices lorem maximus nec. Sed tincidunt at eros id feugiat. Quisque feugiat est eget justo ullamcorper, nec porta ante maximus.\r\n\r\nCras nibh nulla, viverra non gravida laoreet, convallis non lorem. Nunc finibus molestie orci. Praesent nisi lectus, egestas nec dolor eget, viverra tempor ex. Maecenas felis eros, rhoncus non odio vitae, tincidunt suscipit erat. Maecenas placerat tincidunt lorem, at sagittis diam viverra nec. Duis felis tellus, ullamcorper quis euismod ac, sollicitudin at enim. Suspendisse et pulvinar augue, a malesuada justo.\r\n\r\nEtiam in lacus aliquam, auctor neque quis, molestie metus. Pellentesque at auctor ligula. Aenean sollicitudin arcu in dapibus varius. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc sapien diam, cursus non eros quis, eleifend consequat risus. Vestibulum sem nulla, ornare eget arcu at, hendrerit scelerisque lacus. Donec non est sit amet dolor suscipit consequat vitae nec neque. Suspendisse consequat risus velit, sed ullamcorper metus rhoncus vel. Proin eget arcu urna.";

    [Benchmark]
    public string Regular()
    {
        return TextFormatter.Format(BenchmarkText);
    }
}