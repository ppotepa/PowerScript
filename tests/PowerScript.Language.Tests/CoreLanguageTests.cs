using NUnit.Framework;

namespace PowerScript.Language.Tests;

/// <summary>
/// Core language feature tests
/// </summary>
[TestFixture]
public class CoreLanguageTests : LanguageTestBase
{
    // ========== TYPE SYSTEM ==========

    [Test]
    [Category("TypeSystem")]
    public void IntType_Declaration()
    {
        var script = @"INT age = 25
PRINT age";
        Assert.DoesNotThrow(() => ExecuteScript(script));
        Assert.That(GetOutput(), Does.Contain("25"));
    }

    [Test]
    [Category("TypeSystem")]
    public void StringType_Declaration()
    {
        var script = @"STRING name = ""PowerScript""
PRINT name";
        Assert.DoesNotThrow(() => ExecuteScript(script));
        Assert.That(GetOutput(), Does.Contain("POWERSCRIPT"));
    }

    [Test]
    [Category("TypeSystem")]
    public void NumberType_Declaration()
    {
        var script = @"NUMBER count = 100
PRINT count";
        Assert.DoesNotThrow(() => ExecuteScript(script));
        Assert.That(GetOutput(), Does.Contain("100"));
    }

    [Test]
    [Category("TypeSystem")]
    public void VarType_Declaration()
    {
        var script = @"VAR x = 42
PRINT x";
        Assert.DoesNotThrow(() => ExecuteScript(script));
        Assert.That(GetOutput(), Does.Contain("42"));
    }

    // ========== ARITHMETIC ==========

    [Test]
    [Category("Arithmetic")]
    public void Addition_Works()
    {
        var script = @"INT a = 10
INT b = 20
INT sum = a + b
PRINT sum";
        Assert.DoesNotThrow(() => ExecuteScript(script));
    }

    [Test]
    [Category("Arithmetic")]
    public void Subtraction_Works()
    {
        var script = @"INT a = 50
INT b = 20
INT diff = a - b
PRINT diff";
        Assert.DoesNotThrow(() => ExecuteScript(script));
    }

    [Test]
    [Category("Arithmetic")]
    public void Multiplication_Works()
    {
        var script = @"INT a = 6
INT b = 7
INT product = a * b
PRINT product";
        Assert.DoesNotThrow(() => ExecuteScript(script));
    }

    // ========== CONTROL FLOW ==========

    [Test]
    [Category("ControlFlow")]
    public void IfStatement_Executes()
    {
        var script = @"INT x = 10
IF x > 5 {
    PRINT ""Greater""
}";
        Assert.DoesNotThrow(() => ExecuteScript(script));
    }

    [Test]
    [Category("ControlFlow")]
    public void WhileLoop_Executes()
    {
        var script = @"INT counter = 0
WHILE counter < 3 {
    counter = counter + 1
}
PRINT counter";
        Assert.DoesNotThrow(() => ExecuteScript(script));
    }

    [Test]
    [Category("ControlFlow")]
    public void ForLoop_Executes()
    {
        var script = @"INT sum = 0
FOR i = 1 TO 5 {
    sum = sum + i
}
PRINT sum";
        Assert.DoesNotThrow(() => ExecuteScript(script));
    }

    // ========== STRINGS ==========

    [Test]
    [Category("Strings")]
    public void StringConcatenation_Works()
    {
        var script = @"STRING first = ""Hello""
STRING second = ""World""
STRING combined = first + "" "" + second
PRINT combined";
        Assert.DoesNotThrow(() => ExecuteScript(script));
    }

    [Test]
    [Category("Strings")]
    public void EmptyString_Works()
    {
        var script = @"STRING empty = """"
PRINT empty
PRINT ""Done""";
        Assert.DoesNotThrow(() => ExecuteScript(script));
    }

    // ========== ARRAYS ==========

    [Test]
    [Category("Arrays")]
    public void ArrayDeclaration_Works()
    {
        var script = @"VAR numbers = [1, 2, 3, 4, 5]
PRINT numbers";
        Assert.DoesNotThrow(() => ExecuteScript(script));
    }

    [Test]
    [Category("Arrays")]
    public void ArrayAccess_Works()
    {
        var script = @"VAR numbers = [10, 20, 30]
VAR first = numbers[0]
PRINT first";
        Assert.DoesNotThrow(() => ExecuteScript(script));
    }

    // ========== PRINT STATEMENT ==========

    [Test]
    [Category("Print")]
    public void PrintString_Works()
    {
        var script = @"PRINT ""Hello, World!""";
        Assert.DoesNotThrow(() => ExecuteScript(script));
        Assert.That(GetOutput(), Does.Contain("HELLO, WORLD!"));
    }

    [Test]
    [Category("Print")]
    public void PrintVariable_Works()
    {
        var script = @"INT x = 42
PRINT x";
        Assert.DoesNotThrow(() => ExecuteScript(script));
        Assert.That(GetOutput(), Does.Contain("42"));
    }

    [Test]
    [Category("Print")]
    public void PrintEmptyLine_Works()
    {
        var script = @"PRINT """"";
        Assert.DoesNotThrow(() => ExecuteScript(script));
    }
}
