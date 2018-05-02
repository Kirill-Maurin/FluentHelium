using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess;
using FluentHelium.Base;
using TaskExtensions = FluentHelium.Base.TaskExtensions;

namespace FluentHelium.Benchmarks
{

    public sealed class Runtimes : ManualConfig
    {
        public Runtimes() => Add(Job.Default.With(InProcessToolchain.Instance));
    }

    [KeepBenchmarkFiles]
    [Config(typeof(Runtimes))]
    public sealed class Benchmarks
    {
        [Benchmark]
        public Exception AwaitCanceledBencmark() => TestTask(TaskExtensions.Canceled<Unit>()).Exception;

        [Benchmark]
        public Exception AwaitCanceledResultBencmark() => TestWithResult(TaskExtensions.Canceled<Unit>()).Exception;

        private async Task<Unit> TestTask(Task<Unit> inner) => await inner.ConfigureAwait(false);

        private async Task<Result<Unit>> TestResult(Task<Unit> inner) => await inner.ToResult();
        private Task<Unit> TestWithResult(Task<Unit> inner) => TestResult(inner).ToResult().ToTask();
    }

    public static class Program
    {
        public static void Main(string[] args) => BenchmarkRunner.Run<Benchmarks>();
    }
}
