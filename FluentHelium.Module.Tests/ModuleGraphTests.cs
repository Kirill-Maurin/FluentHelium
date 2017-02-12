using System;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Xunit;
using static FluentHelium.Bdd.GivenWhenThenExtensions;

namespace FluentHelium.Module.Tests
{
    public class ModuleGraphTests
    {
        [Fact]
        public void SingleModuleGraphTest()
        {
            Given(() =>
            {
                var a = typeof(object).ToFakeModuleDescriptor("A", typeof (int));
                return new[] {a};
            }).
            When(_ => _.ToModuleGraph((c, t, i) => i.First())).
            Then(_ => _.Input.Count.Should().Be(1)).
                And(_ => _.Input.First().Key.Should().Be(typeof (object))).
                And(_ => _.Input.First().Count().Should().Be(1)).
                And(_ => _.Output.First().Key.Should().Be(typeof (int))).
                And(_ => _.Output.First().Count().Should().Be(1));
        }

        [Fact]
        public void DoubleModuleGraphTest()
        {
            Given(() =>
            {
                var a = typeof(object).ToFakeModuleDescriptor("A");
                var b = typeof(object).ToFakeProducerModuleDescriptor("B");
                return new[] { a, b };
            }).
            When(_ => _.ToModuleGraph((c, t, i) => i.First())).
            Then(_ => _.Input.Count.Should().Be(0)).
                And(_ => _.Output.First().Key.Should().Be(typeof(object))).
                And(_ => _.Output.First().Count().Should().Be(1)).
                And(_ => _.Order.Count.Should().Be(2)).
                And(_ => _.Order.Select(m => m.Name).Should().Equal(new []{"B", "A"}));
        }

        [Fact]
        public void SimpleDependencyCycleTest()
        {
            Given(() =>
            {
                var a = typeof (object).ToFakeModuleDescriptor("A", typeof (int));
                var b = typeof (int).ToFakeModuleDescriptor("B", typeof (object));
                return new[] { a, b };
            }).
            When(_ => _.ToModuleGraph((c, t, i) => i.First())).
            Then(_ => _.Input.Count.Should().Be(0)).
                And(_ => _.Output.Count().Should().Be(2)).
                And(_ => _.Cycle.Count.Should().Be(2));
        }

        [Fact]
        public void ComplexModuleTest()
        {
            Given(() =>
            {
                var a = typeof (object).ToFakeModuleDescriptor("A", typeof (int)).ToModule(d => d.ToUsable());
                var b = typeof (int).ToFakeModuleDescriptor("B", typeof (double)).ToModule(d => d.ToUsable());
                return new[] {a, b};
            }).
            When(_ => _.
                Select(m => m.Descriptor).
                ToModuleGraph((c, t, i) => i.First()).
                ToModule((t, i) => t == typeof (double) ? i.First() : null, "C", Guid.Empty, _.ToImmutableDictionary(m => m.Descriptor)).
                Descriptor).
            Then(_ => _.Input.Count.Should().Be(1)).
                And(_ => _.Input.First().Should().Be(typeof(object))).
                And(_ => _.Output.Count.Should().Be(1)).
                And(_ => _.Output.First().Should().Be(typeof(double)));
        }

        [Fact]
        public void PlantUmlTest()
        {
            Given(() =>
            {
                var a = typeof (object).ToFakeModuleDescriptor("A", typeof (int));
                var b = typeof (int).ToFakeModuleDescriptor("B", typeof (double));
                return new[] {a, b};
            }).
            When(_ => _.ToModuleGraph((c, t, i) => i.First()).ToPlantUml()).
            Then(_ => _.Should().StartWith("[B] -> [A] : Int32"));
        }

        [Fact]
        public void GivenTwoDependedModulesController_WhenActivateDependedModule_ThenBothModulesActive()
        {
            Given(() => new []
            {
                typeof (object).ToFakeModuleDescriptor("A", typeof (int)),
                typeof (int).ToFakeModuleDescriptor("B", typeof (double)),
            }).
                And(_ =>
                {
                    var modules = _.ToImmutableDictionary(d => d, d => d.ToFakeModule());
                    var graph = _.ToModuleGraph((c, t, i) => i.First());
                    return graph.ToModuleController(modules, graph.Input.Select(g => g.Key).ToFakeProvider());
                }).
            When((_, mock) => { _.GetProvider(mock[1]); }).
            Then((_, mock) => _.IsActive(mock[0]).Should().BeTrue()).
                And((_, mock) => _.IsActive(mock[1]).Should().BeTrue());
        }

        [Fact]
        public void GivenTwoDependedModulesController_WhenActivateAndDeactivateDependedModule_ThenBothModulesInactive()
        {
            Given(() => new[]
            {
                typeof (object).ToFakeModuleDescriptor("A", typeof (int)),
                typeof (int).ToFakeModuleDescriptor("B", typeof (double)),
            }).
                And(_ =>
                {
                    var modules = _.ToImmutableDictionary(d => d, d => d.ToFakeModule());
                    var graph = _.ToModuleGraph((c, t, i) => i.First());
                    return graph.ToModuleController(modules, graph.Input.Select(g => g.Key).ToFakeProvider());
                }).
            When((_, mock) => { _.GetProvider(mock[1]).Dispose(); }).
            Then((_, mock) => _.IsActive(mock[0]).Should().BeFalse()).
                And((_, mock) => _.IsActive(mock[1]).Should().BeFalse());
        }
    }
}
