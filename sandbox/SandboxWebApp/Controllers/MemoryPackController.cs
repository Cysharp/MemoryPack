using MemoryPack;
using Microsoft.AspNetCore.Mvc;

namespace SandboxWebApp.Controllers;

[Route("api/")]
public class MemoryPackController : Controller
{
    [HttpPost]
    public AllConvertableType Post([FromBody] AllConvertableType value)
    {
        return value;
    }
}
