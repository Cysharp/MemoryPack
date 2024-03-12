namespace MemoryPack.Tests;

public class MemberNameMatchTest
{
    [Fact]
    public void MatchMemberNameTest()
    {
        byte[] serialized = MemoryPackSerializer.Serialize(new MemberNameMatchTestClass(1, 2, 3));
        MemberNameMatchTestClass deserialized = MemoryPackSerializer.Deserialize<MemberNameMatchTestClass>(serialized)!;
        Assert.Equal(1, deserialized.PascalCase);
        Assert.Equal(2, deserialized.LowerCamelCase);
        Assert.Equal(3, deserialized.SnakeCase);
    }
}

[MemoryPackable]
public partial class MemberNameMatchTestClass
{
    public int PascalCase;
    public int LowerCamelCase;
    public int SnakeCase;
    public MemberNameMatchTestClass(int PascalCase, int lowerCamelCase, int snake_case) {
        this.PascalCase = PascalCase;
        LowerCamelCase = lowerCamelCase;
        SnakeCase = snake_case;
    }
}
