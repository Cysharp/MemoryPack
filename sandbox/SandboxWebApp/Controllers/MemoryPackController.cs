using MemoryPack;
using Microsoft.AspNetCore.Mvc;

namespace SandboxWebApp.Controllers;

[Route("api/")]
public class MemoryPackController : Controller
{
    [HttpGet]
    public Person Get()
    {
        return new Person { Age = 99, Name = "hoge" };
    }

    [HttpPost]
    public Person Post([FromBody] Person person)
    {
        return person;
    }
}

[MemoryPackable]
public partial class Person
{
    public int Age { get; set; }
    public string? Name { get; set; }
}
