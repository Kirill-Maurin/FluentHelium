using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Xunit;
using static FluentHelium.Bdd.GivenWhenThenExtensions;
using static FluentHelium.Module.DependencyBuilder;
using static FluentHelium.Module.Module;

namespace FluentHelium.Module.Tests
{
    public sealed class ModuleGraphTests
    {
        [Fact]
        public void SingleModuleGraphTest() => 
            Given(() =>
            {
                var a = typeof(object).ToModuleDescriptor("A", typeof(int));
                return new[] { a };
            }).
            When(_ => _.ToModuleGraph(Simple.ToBuilder(External))).
            Then(_ => _.Input.Count.Should().Be(1)).
                And(_ => _.Input.First().Key.Should().Be(typeof(object))).
                And(_ => _.Input.First().Count().Should().Be(1)).
                And(_ => _.Output.First().Key.Should().Be(typeof(int))).
                And(_ => _.Output.First().Count().Should().Be(1));

        [Fact]
        public void DoubleModuleGraphTest() =>
            Given(() =>
            {
                var a = typeof(object).ToModuleDescriptor("A");
                var b = typeof(object).ToProducerModuleDescriptor("B");
                return new[] { a, b };
            }).
            When(_ => _.ToSimpleModuleGraph()).
            Then(_ => _.Input.Count.Should().Be(0)).
                And(_ => _.Output.First().Key.Should().Be(typeof(object))).
                And(_ => _.Output.First().Count().Should().Be(1)).
                And(_ => _.Order.Count.Should().Be(2)).
                And(_ => _.Order.Select(m => m.Name).Should().Equal("B", "A"));
        

        [Fact]
        public void SimpleDependencyCycleTest() =>
            Given(() =>
            {
                var a = typeof (object).ToModuleDescriptor("A", typeof (int));
                var b = typeof (int).ToModuleDescriptor("B", typeof (object));
                var c = typeof(int).ToConsumerModuleDescriptor("C");
                return new[] { c, a, b };
            }).
            When(_ => _.ToSimpleModuleGraph()).
            Then(_ => _.Input.Count.Should().Be(0)).
                And(_ => _.Output.Count().Should().Be(2)).
                And(_ => _.Cycle.Count.Should().Be(2));
        

        [Fact]
        public void ComplexModuleTest() => 
            Given(() => new []
            {
                typeof (object).ToModuleDescriptor("A", typeof (int)).ToModule(d => d.ToUsable()),
                typeof (int).ToModuleDescriptor("B", typeof (double)).ToModule(d => d.ToUsable()),
            }).
            When(_ => _.
                Select(m => m.Descriptor).
                ToSimpleModuleGraph().
                ToSuperModule((t, i) => t == typeof (double) ? i.First() : null, "C", Guid.Empty, _).
                Descriptor).
            Then(_ => _.Input.Count.Should().Be(1)).
                And(_ => _.Input.First().Should().Be(typeof(object))).
                And(_ => _.Output.Count.Should().Be(1)).
                And(_ => _.Output.First().Should().Be(typeof(double)));
        

        [Fact]
        public void OptionDependencySuccessTest() =>
            Given(() =>
            {
                var a = typeof(object).ToModuleDescriptor("A", typeof(int)).ToModule(d => d.ToUsable());
                var b = typeof(Option<int>).ToModuleDescriptor("B", typeof(double)).ToModule(d => d.ToUsable());
                return new[] {a, b};
            }).
            When(_ => _.
                Select(m => m.Descriptor).
                ToModuleGraph(Optional.Or(Simple).ToBuilder(External))).
            Then(_ => _.Input.Count.Should().Be(1)).
                And(_ => _.Input.First().Key.Should().Be(typeof(object))).
                And(_ => _.Input.First().Count().Should().Be(1)).
                And(_ => _.Output.First().Key.Should().Be(typeof(int))).
                And(_ => _.Output.Count.Should().Be(2));

        [Fact]
        public void MultipleDependencySuccessResolveTest() => 
            Given(() =>
            {
                var a = CreateSimpleModule("A", () => 42);
                var b = CreateSimpleModule("B", (IEnumerable<int> items) => (double)items.Sum());
                var c = CreateSimpleModule<IEnumerable<int>>("C", () => new[] { 21, 84 });
                return new[] { a, b, c };
            }).
            When(_ => _.
                Select(m => m.Descriptor).
                ToModuleGraph(Optional.Or(Multiple).Or(Simple).ToBuilder(External)).
                ToModuleController(
                    _.ToImmutableDictionary(m => m.Descriptor),
                    DependencyProvider.Empty).
                GetProvider(_[1].Descriptor).
                Unwrap(p => p.Resolve<double>())).
            Then(_ => _.Do(v => v.Should().Be(147)));

        [Fact]
        public void OptionDependencyValueSuccessResolveTest() => 
            Given(() =>
            {
                var a = CreateSimpleModule("A", () => 42);
                var b = CreateSimpleModule<Option<int>, double>("B", i => i.GetValue(24));
                return new[] {a, b};
            }).
            When(_ => _.
                Select(m => m.Descriptor).
                ToModuleGraph(Optional.Or(Simple).ToBuilder(External)).
                ToModuleController(
                    _.ToImmutableDictionary(m => m.Descriptor),
                    DependencyProvider.Empty).
                GetProvider(_[1].Descriptor).
                Unwrap(p => p.Resolve<double>())).
            Then(_ => _.Do(v => v.Should().Be(42)));

        [Fact]
        public void OptionDependencyValueFailResolveTest() => 
            Given(() =>
            {
                var b = CreateSimpleModule<Option<int>, double>("B", i => i.GetValue(24));
                return new[] { b };
            }).
            When(_ => _.
                Select(m => m.Descriptor).
                ToModuleGraph(Optional.Or(Simple).ToBuilder(External)).
                ToModuleController(
                    _.ToImmutableDictionary(m => m.Descriptor),
                    Option<int>.Nothing.ToDependencyProvider()).
                GetProvider(_[0].Descriptor).
                Unwrap(p => p.Resolve<double>())).
            Then(_ => _.Do(v => v.Should().Be(24)));

        [Fact]
        public void OptionDependencyRefSuccessResolveTest() => 
            Given(() =>
            {
                var a = CreateSimpleModule<object>("A", () => 42);
                var b = CreateSimpleModule<Option<object>, double>("B", i => (int)i.GetValue(24));
                return new[] { a, b };
            }).
            When(_ => _.
                Select(m => m.Descriptor).
                ToModuleGraph(Optional.Or(Simple).ToBuilder(External)).
                ToModuleController(
                    _.ToImmutableDictionary(m => m.Descriptor),
                    DependencyProvider.Empty).
                GetProvider(_[1].Descriptor).
                Unwrap(p => p.Resolve<double>())).
            Then(_ => _.Do(v => v.Should().Be(42)));

        [Fact]
        public void OptionDependencyRefFailResolveTest() => 
            Given(() =>
            {
                var b = CreateSimpleModule<Option<object>, double>("B", i => (int)i.GetValue(24));
                return new[] { b };
            }).
            When(_ => _.
                Select(m => m.Descriptor).
                ToModuleGraph(Optional.Or(Simple).ToBuilder(External)).
                ToModuleController(
                    _.ToImmutableDictionary(m => m.Descriptor),
                    ((object)null).ToOption().ToDependencyProvider()).
                GetProvider(_[0].Descriptor).
                Unwrap(p => p.Resolve<double>())).
            Then(_ => _.Do(v => v.Should().Be(24)));

        [Fact]
        public void OptionDependencyFailTest() => 
            Given(() =>
            {
                var a = typeof(Option<object>).ToModuleDescriptor("A", typeof(int)).ToModule(d => d.ToUsable());
                var b = typeof(int).ToModuleDescriptor("B", typeof(double)).ToModule(d => d.ToUsable());
                return new[] { a, b };
            }).
            When(_ => _.
                Select(m => m.Descriptor).
                ToModuleGraph(Optional.Or(Simple).ToBuilder(External))).
            Then(_ => _.Input.Count.Should().Be(1)).
                And(_ => _.Input.First().Key.Should().Be(typeof(Option<object>))).
                And(_ => _.Input.First().Count().Should().Be(1)).
                And(_ => _.Output.First().Key.Should().Be(typeof(int))).
                And(_ => _.Output.Count.Should().Be(2));

        [Fact]
        public void PlantUmlTest() => 
            Given(() =>
            {
                var a = typeof (object).ToModuleDescriptor("A", typeof (int));
                var b = typeof (int).ToModuleDescriptor("B", typeof (double));
                return new[] { a, b };
            }).
            When(_ => _.ToSimpleModuleGraph().ToPlantUml()).
            Then(_ => _.Should().Contain("[B] ..> [A] : Int32"));

        [Fact]
        public void GivenTwoDependedModulesController_WhenActivateDependedModule_ThenBothModulesActive() => 
            Given(() => new []
            {
                typeof (object).ToModuleDescriptor("A", typeof (int)),
                typeof (int).ToModuleDescriptor("B", typeof (double))
            }).
                And(_ =>
                {
                    var modules = _.ToImmutableDictionary(d => d, d => d.ToFakeModule());
                    var graph = _.ToSimpleModuleGraph();
                    return graph.ToModuleController(modules, graph.Input.Select(g => g.Key).ToFakeProvider());
                }).
            When((_, mock) => { _.GetProvider(mock[1]); }).
            Then((_, mock) => _.IsActive(mock[0]).Should().BeTrue()).
                And((_, mock) => _.IsActive(mock[1]).Should().BeTrue());

        [Fact]
        public void GivenTwoDependedModulesController_WhenActivateAndDeactivateDependedModule_ThenBothModulesInactive() => 
            Given(() => new[]
            {
                typeof (object).ToModuleDescriptor("A", typeof (int)),
                typeof (int).ToModuleDescriptor("B", typeof (double))
            }).
                And(_ =>
                {
                    var modules = _.ToImmutableDictionary(d => d, d => d.ToFakeModule());
                    var graph = _.ToSimpleModuleGraph();
                    return graph.ToModuleController(modules, graph.Input.Select(g => g.Key).ToFakeProvider());
                }).
            When((_, mock) =>  _.GetProvider(mock[1]).Dispose()).
            Then((_, mock) => _.IsActive(mock[0]).Should().BeFalse()).
                And((_, mock) => _.IsActive(mock[1]).Should().BeFalse());
    }
}
