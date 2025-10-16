using NUnit.Framework;

namespace PowerScript.StandardLibrary.Tests;

[TestFixture]
public class MathLibraryTests : StandardLibraryTestBase
{
    private const string CoreLibPath = "stdlib/Core.ps";
    private const string LibPath = "stdlib/Math.ps";

    [Test]
    public void POWER_TwoToThePowerOfThree_ReturnsEight()
    {
        var code = $@"
            LINK ""{CoreLibPath}""
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = POWER(2, 3)
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("8"));
    }

    [Test]
    public void IS_EVEN_EvenNumber_ReturnsOne()
    {
        var code = $@"
            LINK ""{CoreLibPath}""
            LINK ""{LibPath}""
            LINK System
            
            FLEX result = IS_EVEN(4)
            FLEX str = result -> ToString()
            FLEX dummy = Console -> WriteLine(str)
        ";

        var output = ExecuteCode(code);
        Assert.That(output, Is.EqualTo("1"));
    }
}
