using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using R8.RedisHelper.Utils;

namespace R8.RedisHelper.Tests;

public class OptimizerTests
{
    [Fact]
    public void Test1()
    {
        var obj = new
        {
            Name = "Arash",
            LastName = "Shabbeh",
            Hobbies = new[] { "Programming", "Video Games" },
            Age = (int)33,
            DateOfBirth = new DateTime(1990, 05, 23),
            NationalCode = (long)994392942934,
            Dbl = (double)1.2,
            IsMarried = false,
            IsSingle = true,
            EnumTest = LogLevel.Debug,
            Obligations = (string?)null,
            ArrayOfNumbers = new[] { 1, 2, 3, 4, 5 },
            ArrayOfObjects = new[] { new { Test = 1 }, new { Test = 2 } },
            Skills = new Dictionary<string, int>
            {
                { "C#", 10 },
                { ".NET", 10 }
            },
            CustomImmutable = new
            {
                Test = true
            }
        };

        var optimizedDict = obj.ToOptimizedDictionary();

        optimizedDict.Should().NotBeNull();
        optimizedDict.Should().HaveCount(14);
        optimizedDict.Should().ContainKey("name");
        optimizedDict["name"].Should().Be("Arash");
        optimizedDict.Should().ContainKey("lastName");
        optimizedDict["lastName"].Should().Be("Shabbeh");
        optimizedDict.Should().ContainKey("hobbies");
        optimizedDict["hobbies"].Should().Be("[\"Programming\",\"Video Games\"]");
        optimizedDict.Should().ContainKey("age");
        optimizedDict["age"].Should().Be(33);
        optimizedDict.Should().ContainKey("dateOfBirth");
        optimizedDict["dateOfBirth"].Should().Be(643420800L);
        optimizedDict.Should().ContainKey("nationalCode");
        optimizedDict["nationalCode"].Should().Be(994392942934L);
        optimizedDict.Should().ContainKey("dbl");
        optimizedDict["dbl"].Should().Be(1.2d);
        optimizedDict.Should().ContainKey("isMarried");
        optimizedDict["isMarried"].Should().Be(0);
        optimizedDict.Should().ContainKey("isSingle");
        optimizedDict["isSingle"].Should().Be(1);
        optimizedDict.Should().ContainKey("enumTest");
        optimizedDict["enumTest"].Should().Be(1);
        optimizedDict.Should().NotContainKey("obligations");
        optimizedDict.Should().ContainKey("arrayOfNumbers");
        optimizedDict["arrayOfNumbers"].Should().Be("[1,2,3,4,5]");
        optimizedDict.Should().ContainKey("arrayOfObjects");
        optimizedDict["arrayOfObjects"].Should().Be("[{\"test\":1},{\"test\":2}]");
        optimizedDict.Should().ContainKey("skills");
        optimizedDict["skills"].Should().Be("{\"c#\":10,\".NET\":10}");
        optimizedDict.Should().ContainKey("customImmutable");
        optimizedDict["customImmutable"].Should().Be("{\"test\":true}");
    }

    [Fact]
    public void Test2()
    {
        var jsonObj = new
        {
            Name = "Arash",
            LastName = "Shabbeh",
            Hobbies = new[] { "Programming", "Video Games" },
            Age = (int)33,
            DateOfBirth = new DateTime(1990, 05, 23),
            NationalCode = (long)994392942934,
            Dbl = (double)1.2,
            IsMarried = false,
            IsSingle = true,
            EnumTest = LogLevel.Debug,
            Obligations = (string?)null,
            ArrayOfNumbers = new[] { 1, 2, 3, 4, 5 },
            Skills = new Dictionary<string, int>
            {
                { "C#", 10 },
                { ".NET", 10 }
            },
            CustomImmutable = new
            {
                Test = true
            }
        };
        var obj = new
        {
            json = JsonSerializer.Deserialize<JsonElement>(jsonObj.JsonSerialize())
        };
        var optimizedDict = obj.ToOptimizedDictionary();

        optimizedDict.Should().NotBeNull();
        optimizedDict.Should().HaveCount(1);
        optimizedDict.Should().ContainKey("json");

        var objects = JsonSerializer.Deserialize<Dictionary<string, JsonNode>>(optimizedDict["json"].ToString(), Helpers.JsonSerializerDefaultOptions);
        objects.Should().NotBeNull();
        objects.Should().ContainKey("name");
        objects["name"].GetValue<string>().Should().Be("Arash");
        objects.Should().ContainKey("lastName");
        objects["lastName"].GetValue<string>().Should().Be("Shabbeh");
        objects.Should().ContainKey("hobbies");
        objects["hobbies"].Deserialize<string[]>().Should().Contain("Programming", "Video Games");
        objects.Should().ContainKey("age");
        objects["age"].GetValue<int>().Should().Be(33);
        objects.Should().ContainKey("dateOfBirth");
        objects["dateOfBirth"].GetValue<long>().Should().Be(643420800L);
        objects.Should().ContainKey("nationalCode");
        objects["nationalCode"].GetValue<long>().Should().Be(994392942934L);
        objects.Should().ContainKey("dbl");
        objects["dbl"].GetValue<double>().Should().Be(1.2);
        objects.Should().ContainKey("isMarried");
        objects["isMarried"].GetValue<bool>().Should().Be(false);
        objects.Should().ContainKey("isSingle");
        objects["isSingle"].GetValue<bool>().Should().Be(true);
        objects.Should().ContainKey("enumTest");
        objects["enumTest"].GetValue<int>().Should().Be((int)LogLevel.Debug);
        objects.Should().NotContainKey("obligations");
        objects.Should().ContainKey("arrayOfNumbers");
        objects["arrayOfNumbers"].Deserialize<int[]>().Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });
        objects.Should().ContainKey("skills");
        objects["skills"].Deserialize<Dictionary<string, int>>().Should().BeEquivalentTo(new Dictionary<string, int> { { "c#", 10 }, { ".NET", 10 } });
        objects.Should().ContainKey("customImmutable");
        objects["customImmutable"].AsObject().Should().Contain(x => x.Key == "test" && x.Value.GetValue<bool>() == true);
    }
}