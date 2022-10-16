using MemoryPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleOne : MonoBehaviour
{
    void Start()
    {
        var bin = MemoryPackSerializer.Serialize(new MyPerson { Age = 9999, Name = "hogemogeふがふが" });
        Debug.Log("Payload size:" + bin.Length);
        var v2 = MemoryPackSerializer.Deserialize<MyPerson>(bin);
        Debug.Log("OK Deserialzie:" + v2.Age + ":" + v2.Name);
    }
}

[MemoryPackable]
public partial class MyPerson
{
    public int Age { get; set; }
    public string Name { get; set; }
}
