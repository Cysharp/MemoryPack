using MemoryPack;
using Microsoft.AspNetCore.Mvc;

namespace SandboxWebApp.Controllers;

[Route("api/")]
public class MemoryPackController : Controller
{
    [HttpPost]
    public AllConvertableType Post([FromBody] AllConvertableType value)
    // public Person Post([FromBody] Person value)
    {
        return value;
    }

    [Route("nullableFloat")]
    [HttpPost]
    public NullableFloatTest PostNullableTest([FromBody] NullableFloatTest input)
    {
        var ret = new NullableFloatTest
        {
            // If you're curious about the '* 1.0' part, DM me :-)
            NullableFloat = input.NullableFloat * 1.0F,
            NullableDouble = input.NullableDouble * 1.0D
        };

        return ret;
    }

    [Route("vector3")]
    [HttpPost]
    public Vector3 PostVector3([FromBody] Vector3 value) => value;

    [Route("colorTag")]
    [HttpPost]
    public ColorTag PostColorTag([FromBody] ColorTag value) => value;

    [Route("gameObject")]
    [HttpPost]
    public GameObject PostGameObject([FromBody] GameObject value) => value;
}
