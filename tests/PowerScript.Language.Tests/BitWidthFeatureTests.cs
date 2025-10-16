using NUnit.Framework;

namespace PowerScript.Language.Tests;

/// <summary>
/// Tests for bit-width specification feature - the newest PowerScript feature!
/// </summary>
[TestFixture]
public class BitWidthFeatureTests : LanguageTestBase
{
    [Test]
    public void BitWidth_8Bit_Int()
    {
        var script = @"
INT[8] byte = 255
PRINT byte
";
        Assert.DoesNotThrow(() => ExecuteScript(script));
        Assert.That(GetOutput(), Does.Contain("255"));
    }

    [Test]
    public void BitWidth_16Bit_Int()
    {
        var script = @"
INT[16] short = 30000
PRINT short
";
        Assert.DoesNotThrow(() => ExecuteScript(script));
        Assert.That(GetOutput(), Does.Contain("30000"));
    }

    [Test]
    public void BitWidth_32Bit_Int()
    {
        var script = @"
INT[32] int32 = 2000000000
PRINT int32
";
        Assert.DoesNotThrow(() => ExecuteScript(script));
        Assert.That(GetOutput(), Does.Contain("2000000000"));
    }

    [Test]
    public void BitWidth_Custom_3Bit()
    {
        var script = @"
INT[3] tiny = 7
PRINT tiny
";
        Assert.DoesNotThrow(() => ExecuteScript(script));
        Assert.That(GetOutput(), Does.Contain("7"));
    }

    [Test]
    public void BitWidth_Custom_10Bit()
    {
        var script = @"
INT[10] medium = 1023
PRINT medium
";
        Assert.DoesNotThrow(() => ExecuteScript(script));
        Assert.That(GetOutput(), Does.Contain("1023"));
    }

    [Test]
    public void BitWidth_Number_8Bit()
    {
        var script = @"
NUMBER[8] small = 127
PRINT small
";
        Assert.DoesNotThrow(() => ExecuteScript(script));
        Assert.That(GetOutput(), Does.Contain("127"));
    }

    [Test]
    public void BitWidth_Arithmetic()
    {
        var script = @"
INT[8] a = 100
INT[8] b = 50
INT[8] sum = a + b
PRINT sum
";
        Assert.DoesNotThrow(() => ExecuteScript(script));
        Assert.That(GetOutput(), Does.Contain("150")); // Fixed: arithmetic now works correctly
    }

    [Test]
    public void BitWidth_MixedWithNonBitWidth()
    {
        var script = @"
INT standard = 42
INT[8] specified = 100
PRINT standard
PRINT specified
";
        Assert.DoesNotThrow(() => ExecuteScript(script));
        var output = GetOutput();
        Assert.That(output, Does.Contain("42"));
        Assert.That(output, Does.Contain("100"));
    }
}
