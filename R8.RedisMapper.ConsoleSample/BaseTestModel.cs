namespace R8.RedisMapper.ConsoleSample;

public abstract class BaseTestModel : ITestModel
{
    [RedisFormatter(typeof(Int32ValueSerializer))]
    public int Id { get; set; }
}