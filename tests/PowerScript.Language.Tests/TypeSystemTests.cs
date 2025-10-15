using NUnit.Framework;

namespace PowerScript.Language.Tests;

/// <summary>
/// Tests for the type system: FLEX, VAR, INT, STRING, NUMBER, PREC
/// </summary>
[TestFixture]
public class TypeSystemTests : LanguageTestBase
{
    [Test]
    public void FlexType_AllowsTypeChanges()
    {
        var script = @"
FLEX x = 10
x = ""hello""
x = 3.14
PRINT x
";
        Assert.DoesNotThrow(() => ExecuteScript(script));
    }

    [Test]
    public void VarType_InfersFromInitialValue()
    {
        var script = @"
VAR count = 42
PRINT count
";
        Assert.DoesNotThrow(() => ExecuteScript(script));
    }

    [Test]
    public void IntType_HoldsIntegers()
    {
        var script = @"
INT age = 25
INT year = 2025
PRINT age
PRINT year
";
        Assert.DoesNotThrow(() => ExecuteScript(script));
    }

    [Test]
    public void StringType_HoldsStrings()
    {
        var script = @"
STRING name = ""PowerScript""
STRING message = ""Hello, World!""
PRINT name
PRINT message
";
        Assert.DoesNotThrow(() => ExecuteScript(script));
    }

    [Test]
    public void NumberType_HoldsIntegersAndDecimals()
    {
        var script = @"
NUMBER count = 100
NUMBER price = 19.99
PRINT count
PRINT price
";
        Assert.DoesNotThrow(() => ExecuteScript(script));
    }

    [Test]
    public void PrecType_HoldsFloatingPoint()
    {
        var script = @"
PREC pi = 3.14159
PREC temperature = 98.6
PRINT pi
PRINT temperature
";
        Assert.DoesNotThrow(() => ExecuteScript(script));
    }
}
