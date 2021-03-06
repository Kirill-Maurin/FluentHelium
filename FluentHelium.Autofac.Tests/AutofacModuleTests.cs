﻿using System;
using Autofac;
using FluentAssertions;
using FluentHelium.Base;
using FluentHelium.Module;
using NSubstitute;
using Xunit;
using static FluentHelium.Bdd.GivenWhenThenExtensions;
using static NSubstitute.Substitute;

namespace FluentHelium.Autofac.Tests
{
    public sealed class AutofacModuleTests
    {
        [Fact]
        public void GivenSimpleModule_WhenRegisterResolveResolveRelease_ThenModuleActivatedOnce() => 
            Given(CreateModule((m, dp, md, dd) => m)).
            When(RegisterResolveResolveRelease).
            ThenMock(_ => _.Received(1).Activate(Arg.Any<IDependencyProvider>()));

        [Fact]
        public void GivenSimpleModule_WhenRegisterResolveResolveRelease_ThenCorrectlyResolved() => 
            Given(CreateModule((m, dp, md, dd) => m)).
            When(RegisterResolveResolveRelease).
            Then(_ => _.Should().BeOfType<Input>());

        [Fact]
        public void GivenSimpleModule_WhenRegisterResolveResolveRelease_ThenModuleDeactivatedOnce() => 
            Given(CreateModule((m, dp, md, dd) => new { Module = m, Deactivator = md })).
            When(_ => RegisterResolveResolveRelease(_.Module)).
            ThenMock(_ => _.Deactivator.Received(1).Dispose());

        [Fact]
        public void GivenSimpleModule_WhenRegisterResolveResolveRelease_ThenDependencyDeactivatedOnce() => 
            Given(CreateModule((m, dp, md, dd) => new { Module = m, Deactivator = dd })).
            When(_ => RegisterResolveResolveRelease(_.Module)).
            ThenMock(_ => _.Deactivator.Received(1).Dispose());

        [Fact]
        public void GivenSimpleModule_WhenRegisterResolveResolveRelease_ThenDependencyResolvedOnce() => 
            Given(CreateModule((m, dp, md, dd) => new { Module = m, Provider = dp })).
            When(_ => RegisterResolveResolveRelease(_.Module)).
            ThenMock(_ => _.Provider.Received(1).Resolve(typeof(object)));

        [Fact]
        public void GivenAutofacModule_WhenActivateResolve_ThenDependencyResolvedCorrectly() => 
            Given(CreateAutofacModule()).
            When(_ => _.Activate(typeof(Input).ToDependencyProvider(t => ((object)new Input()).ToUsable())).Unwrap(p => p.Resolve<object>())).
            Then(_ => _.Do(o => o.Should().BeOfType<Input>()));

        static ContainerBuilder CreateContainerBuilder(IModule module)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new Input());
            builder.RegisterFluentHeliumModule<object>(module);
            return builder;
        }

        static IModule CreateAutofacModule() 
            => typeof(Input).ToModuleDescriptor(
                "Test Autofac module",
                Guid.Parse("{047D58A7-1950-4937-B0FA-5B5935E3EF7D}"),
                typeof(object)).
            ToAutofacModule(b =>
            {
                b.Register(c => (object) c.Resolve<Input>());
            });

        static T CreateModule<T>(Func<IModule, IDependencyProvider, IDisposable, IDisposable, T> toResult)
        {
            var module = For<IModule>();
            module.Descriptor.Returns(CreateModuleDescriptor());
            var provider = For<IDependencyProvider>();
            var moduleDeactivator = For<IDisposable>();
            var dependencyDeactivator = For<IDisposable>();
            module.Activate(Arg.Any<IDependencyProvider>()).Returns(o =>
            {
                var input = o.Arg<IDependencyProvider>().Resolve<Input>();
                provider.Resolve(typeof (object)).Returns(input.Select(i => (object)i).Wrap(d => dependencyDeactivator));
                return provider.ToUsable(moduleDeactivator);
            });
            return toResult(module, provider, moduleDeactivator, dependencyDeactivator);
        }

        static object RegisterResolveResolveRelease(IModule module)
        {
            object resolved;
            using (var container = CreateContainerBuilder(module).Build())
            {
                container.Resolve<object>();
                resolved = container.Resolve<object>();
            }
            return resolved;
        }

        static IModuleDescriptor CreateModuleDescriptor() => typeof (Input).ToModuleDescriptor(
            "Test module",
            Guid.Parse("{6C7026AD-BD0E-4DC7-888B-6CA29B07C43F}"),
            typeof (object));

        sealed class Input
        {
        }
    }
}
