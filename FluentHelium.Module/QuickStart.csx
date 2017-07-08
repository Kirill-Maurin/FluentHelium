#r "C:\Projects\FluentHelium\FluentHelium.Module\bin\Debug\netstandard1.1\FluentHelium.Module.dll"
#r "C:\Users\Leo\.nuget\packages\system.reactive.interfaces\3.0.0\lib\netstandard1.0\System.Reactive.Interfaces.dll"
using FluentHelium.Module;
using static FluentHelium.Module.ModuleExtensions;
var module = CreateSimpleModule("Главный ответ", () => 42);
WriteLine(module);
var provider = module.Activate(DependencyProvider.Empty);
WriteLine(provider);
WriteLine(module);
provider.Do(p =>
{
    var resource = p.Resolve<int>();
    WriteLine($"Usable resource: {resource}");
    resource.Using(i => WriteLine($"Resource: {i}"));
    WriteLine($"Resource after using: {resource}");
});

provider.Dispose();
WriteLine(provider);
WriteLine(module);
WriteLine(module.ToPlantUml());
var converter = CreateSimpleModule("Преобразователь", (int input) => (double)input);
var consumer = CreateSimpleModule("Потребитель", (double input) => {});
var graph = new []{ consumer, converter, module }.Select(m => m.Descriptor).ToSimpleModuleGraph();
WriteLine(graph.ToPlantUml());
WriteLine(graph);
var reverse = CreateSimpleModule("Реверс", (double input) => (int)input);
var graphWithCycle = new[] { consumer, converter, reverse }.Select(m => m.Descriptor).ToSimpleModuleGraph();
WriteLine(graphWithCycle.ToPlantUml());
WriteLine(graphWithCycle);

