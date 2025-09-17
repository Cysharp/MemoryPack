using ConsoleAppFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SandboxConsoleApp;

// check the System.Text.Json's constructor select rule.

// For a class, if the only constructor is a parameterized one, that constructor will be used.
// For a struct, or a class with multiple constructors, specify the one to use by applying the [JsonConstructor] attribute.
// When the attribute is not used, a public parameterless constructor is always used if present.

// MemoryPack choose class/struct as same rule.
// If has no explicit constructor, use parameterless one.
// If has a one parameterless/parameterized constructor, choose it.
// If has multiple constructor, should apply [MemoryPackConstructor] attribute (no automatically choose one), otherwise generator error it.

// The parameter names of a parameterized constructor must match the property names.
// Matching is case-insensitive, and the constructor parameter must match the actual property name.

class SystemTextJsonChecker
{
    //[RootCommand]
    public void JsonConstructorSelector()
    {
#if false

#endif
        var one = JsonSerializer.Serialize(new One());
        var two = JsonSerializer.Serialize(new Two(1, 2));
        var three = JsonSerializer.Serialize(new Three());
        var four = JsonSerializer.Serialize(new Four(1));
        var five = JsonSerializer.Serialize(new Five(1, 2));
        var six = JsonSerializer.Serialize(new Six(1, 2));
        var seven = JsonSerializer.Serialize(new Seven(1, 2));

        Console.WriteLine("---");

        JsonSerializer.Deserialize<One>(one);
        JsonSerializer.Deserialize<Two>(two);

        // if exists parameterized one, used this
        JsonSerializer.Deserialize<Three>(three);

        // class exists two constructor, no.
        // JsonSerializer.Deserialize<Four>(four);

        // struct choosed default
        JsonSerializer.Deserialize<Five>(five);
        JsonSerializer.Deserialize<Six>(six);

        // choosed [JsonContructor](multiple JsonConstructor no)
        JsonSerializer.Deserialize<Seven>(seven);
    }

    [Command("")]
    public void PrivateSerialization()
    {
        // private field/property can not annnotate JsonInclude
        var v = JsonSerializer.Serialize(new PrivateOK(99, 100)
        {
            PublicField = 1000,
            PublicProp = 9999
        });
        Console.WriteLine(v);
    }
}

public class One
{
    public One()
    {
        Console.WriteLine("Called One");
    }
}

public class Two
{
    public int X { get; }
    public int Y { get; }

    public Two(int x, int y)
    {
        Console.WriteLine("Called Two");
        this.X = x;
        this.Y = y;
    }
}

public class Three
{

    public int X { get; }
    public int Y { get; }

    public Three()
    {
        Console.WriteLine("Called Three One");
    }

    public Three(int x, int y)
    {
        Console.WriteLine("Called Three Two");
        this.X = x;
        this.Y = y;
    }
}

public class Four
{

    public int X { get; }
    public int Y { get; }

    public Four(int x)
    {
        Console.WriteLine("Called Four Single");
        this.X = x;
    }

    public Four(int x, int y)
    {
        Console.WriteLine("Called Four Two");
        this.X = x;
        this.Y = y;
    }
}




public struct Five
{
    public int X { get; }
    public int Y { get; }

    public Five(int x, int y)
    {
        Console.WriteLine("Called Five");
        this.X = x;
        this.Y = y;
    }
}



public struct Six
{
    public int X { get; }
    public int Y { get; }

    public Six()
    {

    }

    public Six(int x, int y)
    {
        Console.WriteLine("Called Six");
        this.X = x;
        this.Y = y;
    }
}


public struct Seven
{
    public int X { get; }
    public int Y { get; }

    public Seven()
    {

    }

    [JsonConstructor]
    public Seven(int x, int y)
    {
        Console.WriteLine("Called Six");
        this.X = x;
        this.Y = y;
    }
}

public class PrivateOK
{
    public int PublicField;
    public int PublicProp { get; set; }

    //[JsonInclude]
    //private int privateField;

    [JsonInclude]
    public int PrivateGetter { private get; set; }

    [JsonInclude]
    public int PrivateSetter { get; }


    // [JsonInclude]
    // int PrivateBoth { get; set; }

    public PrivateOK(int privateSetter, int privateBoth)
    {
        this.PrivateSetter = privateSetter;
        // this.PrivateBoth = privateBoth;
    }

}
