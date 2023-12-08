using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Jobs;

namespace R8.RedisMapper.Benchmark;

[MemoryDiagnoser]
[InliningDiagnoser(true, true)]
[TailCallDiagnoser]
[EtwProfiler]
[ConcurrencyVisualizerProfiler]
[NativeMemoryProfiler]
[ThreadingDiagnoser]
[ExceptionDiagnoser]
[SimpleJob(RuntimeMoniker.Net60)]
[SimpleJob(RuntimeMoniker.Net70)]
[RPlotExporter]
public class OptimizerBenchmark
{
    // private DateTime _expected;
    // private CachedPropertyInfo _propertyInfo;
    // private obj1 _model;
    //
    // [GlobalSetup]
    // public void Setup()
    // {
    //     _expected = DateTime.UtcNow;
    //
    //     _model = new obj1();
    //     _propertyInfo = _model.GetType().GetPublicProperties(Array.Empty<string>(), new RedisFieldCamelCaseFormatter())[0];
    // }
    //
    // [Benchmark]
    // public void SetValue() => _propertyInfo.Property.SetValue(_model, _expected);
    //
    // [Benchmark]
    // public void SetDateTime() => _propertyInfo.Property.SetDateTime(_model, _expected);
    //
    // [Benchmark]
    // public DateTime GetValue() => (DateTime)_propertyInfo.Property.GetValue(_model);
    //
    // [Benchmark]
    // public DateTime GetDateTime() => _propertyInfo.Property.GetDateTime(_model).Value;
}

public class obj1
{
    public DateTime Date { get; set; }
}