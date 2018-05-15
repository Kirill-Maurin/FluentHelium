#r "E:\FluentHelium\.build\Debug\FluentHelium.Module.dll"
#r "C:\Users\Leo\.nuget\packages\system.collections.immutable\1.4.0\lib\netstandard1.0\System.Collections.Immutable.dll"
#r "C:\Users\Leo\.nuget\packages\system.reactive.interfaces\3.0.0\lib\netstandard1.0\System.Reactive.Interfaces.dll"
#r "C:\Users\Leo\.nuget\packages\system.reactive.linq\3.0.0\lib\netstandard1.0\System.Reactive.Linq.dll"
#r "C:\Users\Leo\.nuget\packages\system.reactive.core\3.0.0\lib\netstandard1.0\System.Reactive.Core.dll"
using System.Runtime.InteropServices.ComTypes;
using FluentHelium.Module;
using FluentHelium.Base;
using static FluentHelium.Module.Module;
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
Write(graphWithCycle.ToPlantUml());
WriteLine(graphWithCycle);
var graphWithExternal = new[] { consumer, converter }.Select(m => m.Descriptor).ToSimpleModuleGraph();
Write(graphWithExternal.ToPlantUml());
WriteLine(graphWithExternal);
var superModule = graphWithExternal.ToSimpleSuperModule("Надмодуль", consumer, converter);
WriteLine(superModule.ToPlantUml());
WriteLine(superModule);

module.Activate(DependencyProvider.Empty).Using(p =>
{
    superModule.Activate(p).Using(superProvider =>
    {
        WriteLine("После активации надмодуля");
        WriteLine($"Надмодуль: {superModule}");
        WriteLine($"Преобразователь: {converter}");
        WriteLine($"Потребитель: {consumer}");
    });
});
WriteLine();
WriteLine("После деактивации надмодуля");
WriteLine($"Надмодуль: {superModule}");
WriteLine($"Преобразователь: {converter}");
WriteLine($"Потребитель: {consumer}");

var lazyModule = graph.ToSimpleLazyModule("Ленивый модуль", module, converter, consumer);
WriteLine(lazyModule.ToPlantUml());
WriteLine(lazyModule);

lazyModule.Activate(DependencyProvider.Empty).Using(p =>
{
    WriteLine();
    WriteLine("После активации ленивого модуля");
    WriteLine($"Ленивый модуль: {lazyModule}");
    WriteLine($"Преобразователь: {converter}");
    WriteLine($"Источник: {module}");
    WriteLine($"Потребитель: {consumer}");
    p.Resolve<double>().Using(i =>
    {
        WriteLine();
        WriteLine("После запроса ресурса от ленивого модуля");
        WriteLine($"Ленивый модуль: {lazyModule}");
        WriteLine($"Преобразователь: {converter}");
        WriteLine($"Источник: {module}");
        WriteLine($"Потребитель: {consumer}");
    });
    WriteLine();
    WriteLine("После высвобождения ресурса");
    WriteLine($"Ленивый модуль: {lazyModule}");
    WriteLine($"Преобразователь: {converter}");
    WriteLine($"Источник: {module}");
    WriteLine($"Потребитель: {consumer}");
});
WriteLine("После деактивации ленивого модуля");
WriteLine($"Ленивый модуль: {lazyModule}");
