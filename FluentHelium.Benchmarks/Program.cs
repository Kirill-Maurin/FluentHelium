using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess;
using FluentHelium.Base;
using System;
using System.Threading.Tasks;
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
        public Exception AwaitCanceledBenchmark() => TestTask(TaskExtensions.Canceled<Unit>()).Exception;

        [Benchmark]
        public Exception AwaitCanceledResultBenchmark() => TestWithResult(TaskExtensions.Canceled<Unit>()).Exception;

        [Benchmark]
        public Exception SelectCanceledTaskBenchmark() => TestWithTask(TaskExtensions.Canceled<Unit>()).Exception;

        async Task<Unit> TestTask(Task<Unit> inner) => await inner.ConfigureAwait(false);

        async Task<Result<Unit>> TestResult(Task<Unit> inner) => await inner.ToResult();
        Task<Unit> TestWithResult(Task<Unit> inner) => TestResult(inner).ToResult().ToTask();

        Task<Unit> TestWithTask(Task<Unit> inner) => inner.Select(t => t);
    }

    public static class Program
    {
        public static void Main(string[] args) => BenchmarkRunner.Run<Benchmarks>();
    }
}
