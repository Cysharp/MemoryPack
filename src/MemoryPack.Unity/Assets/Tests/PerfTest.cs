using MemoryPack;
using MemoryPack.Tests.Models;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;

public class PerfTest
{
    Person person;
    Primitives primitives;
    NestCase nestCase;

    string personJson;
    string primitivesJson;
    string nestCaseJson;

    byte[] personMempack;
    byte[] primitivesMempack;
    byte[] nestCaseMempack;

    public PerfTest()
    {
        person = new Person { Age = 888, Name = "aaaaaaaaa" };
        primitives = new Primitives()
        {
            Bool = false,
            Byte = 12,
            Char = 'z',
            Double = 1231.214,
            Float = 314.532f,
            Int = 9999,
            Long = 99999999,
            Short = 12,
            String = "hogemogehugahuga"
        };

        var inner = new Inner { Int = 9999999, String = "hogemoge", Double = 1321.2 };
        nestCase = new NestCase
        {
            A = inner,
            B = inner,
            C = inner,
            D = inner,
            E = inner,
            F = inner,
            G = inner,
            H = inner,
            I = inner,
        };

        personJson = JsonUtility.ToJson(person);
        primitivesJson = JsonUtility.ToJson(primitives);
        nestCaseJson = JsonUtility.ToJson(nestCase);

        personMempack = MemoryPackSerializer.Serialize(person);
        primitivesMempack = MemoryPackSerializer.Serialize(primitives);
        nestCaseMempack = MemoryPackSerializer.Serialize(nestCase);

        Debug.Log(JsonUtility.FromJson<Person>(personJson).Name == person.Name);
        Debug.Log(JsonUtility.FromJson<Primitives>(primitivesJson).Short == primitives.Short);
        Debug.Log(JsonUtility.FromJson<NestCase>(nestCaseJson).E.Double == nestCase.E.Double);

        Debug.Log(MemoryPackSerializer.Deserialize<Person>(personMempack).Name == person.Name);
        Debug.Log(MemoryPackSerializer.Deserialize<Primitives>(primitivesMempack).Short == primitives.Short);
        Debug.Log(MemoryPackSerializer.Deserialize<NestCase>(nestCaseMempack).E.Double == nestCase.E.Double);
    }

    [Test, Performance]
    public void Serialize_Person_JsonUtility()
    {
        Measure.Method(() =>
        {
            JsonUtility.ToJson(person);
        })
        .WarmupCount(10)
        .IterationsPerMeasurement(10000)
        .MeasurementCount(10)
        .Run();
    }

    [Test, Performance]
    public void Serialize_Primitives_JsonUtility()
    {
        Measure.Method(() =>
        {
            JsonUtility.ToJson(primitives);
        })
        .WarmupCount(10)
        .IterationsPerMeasurement(10000)
        .MeasurementCount(10)
        .Run();
    }

    [Test, Performance]
    public void Serialize_Nestcase_JsonUtility()
    {
        Measure.Method(() =>
        {
            JsonUtility.ToJson(nestCase);
        })
        .WarmupCount(10)
        .IterationsPerMeasurement(10000)
        .IterationsPerMeasurement(10000)
        .MeasurementCount(10)
        .Run();
    }

    [Test, Performance]
    public void Serialize_Person_MemoryPack()
    {
        Measure.Method(() =>
        {
            MemoryPackSerializer.Serialize(person);
        })
        .WarmupCount(10)
        .IterationsPerMeasurement(10000)
        .MeasurementCount(10)
        .Run();
    }

    [Test, Performance]
    public void Serialize_Primitives_MemoryPack()
    {
        Measure.Method(() =>
        {
            MemoryPackSerializer.Serialize(primitives);
        })
        .WarmupCount(10)
        .IterationsPerMeasurement(10000)
        .MeasurementCount(10)
        .Run();
    }

    [Test, Performance]
    public void Serialize_Nestcase_MemoryPack()
    {
        Measure.Method(() =>
        {
            MemoryPackSerializer.Serialize(nestCase);
        })
        .WarmupCount(10)
        .IterationsPerMeasurement(10000)
        .MeasurementCount(10)
        .Run();
    }

    //



    [Test, Performance]
    public void Deserialize_Person_JsonUtility()
    {
        Measure.Method(() =>
        {
            JsonUtility.FromJson<Person>(personJson);
        })
        .WarmupCount(10)
        .IterationsPerMeasurement(10000)
        .MeasurementCount(10)
        .Run();
    }

    [Test, Performance]
    public void Deserialize_Primitives_JsonUtility()
    {
        Measure.Method(() =>
        {
            JsonUtility.FromJson<Primitives>(primitivesJson);
        })
        .WarmupCount(10)
        .IterationsPerMeasurement(10000)
        .MeasurementCount(10)
        .Run();
    }

    [Test, Performance]
    public void Deserialize_Nestcase_JsonUtility()
    {
        Measure.Method(() =>
        {
            JsonUtility.FromJson<NestCase>(nestCaseJson);
        })
        .WarmupCount(10)
        .IterationsPerMeasurement(10000)
        .IterationsPerMeasurement(10000)
        .MeasurementCount(10)
        .Run();
    }

    [Test, Performance]
    public void Deserialize_Person_MemoryPack()
    {
        Measure.Method(() =>
        {
            MemoryPackSerializer.Deserialize<Person>(personMempack);
        })
        .WarmupCount(10)
        .IterationsPerMeasurement(10000)
        .MeasurementCount(10)
        .Run();
    }

    [Test, Performance]
    public void Deserialize_Primitives_MemoryPack()
    {
        Measure.Method(() =>
        {
            MemoryPackSerializer.Deserialize<Primitives>(primitivesMempack);
        })
        .WarmupCount(10)
        .IterationsPerMeasurement(10000)
        .MeasurementCount(10)
        .Run();
    }

    [Test, Performance]
    public void Deserialize_Nestcase_MemoryPack()
    {
        Measure.Method(() =>
        {
            MemoryPackSerializer.Deserialize<NestCase>(nestCaseMempack);
        })
        .WarmupCount(10)
        .IterationsPerMeasurement(10000)
        .MeasurementCount(10)
        .Run();
    }
}

