using NUnit.Framework;

namespace PowerScript.StandardLibrary.Tests;

[TestFixture]
public class ObjectSyntaxTests : StandardLibraryTestBase
{
    [Test]
    public void BasicObject_Creation()
    {
        string code = @"
LINK System
FLEX person = {name = ""John"", age = 30}
FLEX str = person -> ToString()
FLEX dummy = Console -> WriteLine(str)
";
        var output = ExecuteCode(code);
        
        Assert.That(output, Does.Contain("NAME"));
        Assert.That(output, Does.Contain("John"));
        Assert.That(output, Does.Contain("AGE"));
        Assert.That(output, Does.Contain("30"));
    }

    [Test]
    public void PropertyAccess_BasicProperty()
    {
        string code = @"
LINK System
FLEX person = {name = ""Alice"", age = 25}
FLEX name = person.name
FLEX nameStr = name -> ToString()
FLEX dummy1 = Console -> WriteLine(nameStr)
FLEX age = person.age
FLEX ageStr = age -> ToString()
FLEX dummy2 = Console -> WriteLine(ageStr)
";
        var output = ExecuteCode(code);
        
        Assert.That(output, Does.Contain("Alice"));
        Assert.That(output, Does.Contain("25"));
    }

    [Test]
    public void TypeAnnotation_CreatesTypedObject()
    {
        string code = @"
LINK System
FLEX employee = {name = ""Bob"", role = ""Developer""} AS Employee
FLEX str = employee -> ToString()
FLEX dummy = Console -> WriteLine(str)
";
        var output = ExecuteCode(code);
        
        Assert.That(output, Does.Contain("NAME"));
        Assert.That(output, Does.Contain("Bob"));
        Assert.That(output, Does.Contain("EMPLOYEE"));
    }

    [Test]
    public void StrictType_MarkedWithExclamation()
    {
        string code = @"
LINK System
FLEX point = {x = 10, y = 20} AS Point!
FLEX str = point -> ToString()
FLEX dummy1 = Console -> WriteLine(str)
FLEX x = point.x
FLEX xStr = x -> ToString()
FLEX dummy2 = Console -> WriteLine(xStr)
FLEX y = point.y
FLEX yStr = y -> ToString()
FLEX dummy3 = Console -> WriteLine(yStr)
";
        var output = ExecuteCode(code);
        
        Assert.That(output, Does.Contain("POINT!"));
        Assert.That(output, Does.Contain("10"));
        Assert.That(output, Does.Contain("20"));
    }

    [Test]
    public void EmptyObject_Creation()
    {
        string code = @"
LINK System
FLEX empty = {}
FLEX str = empty -> ToString()
FLEX dummy = Console -> WriteLine(str)
";
        var output = ExecuteCode(code);
        
        Assert.That(output, Does.Contain("{}"));
    }

    [Test]
    public void ObjectWithExpressions_EvaluatesValues()
    {
        string code = @"
LINK System
FLEX data = {value = 5 + 10, text = ""hello""}
FLEX val = data.value
FLEX valStr = val -> ToString()
FLEX dummy1 = Console -> WriteLine(valStr)
FLEX txt = data.text
FLEX txtStr = txt -> ToString()
FLEX dummy2 = Console -> WriteLine(txtStr)
";
        var output = ExecuteCode(code);
        
        Assert.That(output, Does.Contain("15"));
        Assert.That(output, Does.Contain("hello"));
    }

    [Test]
    public void PropertyChaining_MultipleAccess()
    {
        string code = @"
LINK System
FLEX outer = {inner = {value = 42}}
FLEX inner = outer.inner
FLEX str = inner -> ToString()
FLEX dummy = Console -> WriteLine(str)
";
        var output = ExecuteCode(code);
        
        // The inner object should be printed
        Assert.That(output, Does.Contain("VALUE"));
        Assert.That(output, Does.Contain("42"));
    }

    [Test]
    public void MultipleProperties_AllAccessible()
    {
        string code = @"
LINK System
FLEX config = {host = ""localhost"", port = 8080, secure = 1}
FLEX host = config.host
FLEX hostStr = host -> ToString()
FLEX dummy1 = Console -> WriteLine(hostStr)
FLEX port = config.port
FLEX portStr = port -> ToString()
FLEX dummy2 = Console -> WriteLine(portStr)
FLEX secure = config.secure
FLEX secureStr = secure -> ToString()
FLEX dummy3 = Console -> WriteLine(secureStr)
";
        var output = ExecuteCode(code);
        
        Assert.That(output, Does.Contain("localhost"));
        Assert.That(output, Does.Contain("8080"));
        Assert.That(output, Does.Contain("1"));
    }

    [Test]
    public void ObjectInVariable_CanBeReassigned()
    {
        string code = @"
LINK System
FLEX obj = {a = 1}
FLEX val1 = obj.a
FLEX str1 = val1 -> ToString()
FLEX dummy1 = Console -> WriteLine(str1)
FLEX obj = {a = 2}
FLEX val2 = obj.a
FLEX str2 = val2 -> ToString()
FLEX dummy2 = Console -> WriteLine(str2)
";
        var output = ExecuteCode(code);
        
        Assert.That(output, Does.Contain("1"));
        Assert.That(output, Does.Contain("2"));
    }

    [Test]
    public void ObjectProperties_CaseInsensitive()
    {
        string code = @"
LINK System
FLEX item = {Name = ""Test"", VALUE = 100}
FLEX name = item.name
FLEX nameStr = name -> ToString()
FLEX dummy1 = Console -> WriteLine(nameStr)
FLEX value = item.value
FLEX valueStr = value -> ToString()
FLEX dummy2 = Console -> WriteLine(valueStr)
";
        var output = ExecuteCode(code);
        
        Assert.That(output, Does.Contain("Test"));
        Assert.That(output, Does.Contain("100"));
    }
}
