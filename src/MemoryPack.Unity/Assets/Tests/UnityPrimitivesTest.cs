using Assets.Scripts.MemoryPackObjects;
using MemoryPack;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tests
{
    public class UnityPrimitivesTest
    {
        private T Convert<T>(T value)
        {
            return MemoryPackSerializer.Deserialize<T>(MemoryPackSerializer.Serialize(value));
        }

        private void ConvertEqual<T>(T value)
        {
            var v = MemoryPackSerializer.Deserialize<T>(MemoryPackSerializer.Serialize(value));
            Assert.AreEqual(value, v);
        }

        private void ConvertCollectionEqual<T>(T[] value)
        {
            var v = MemoryPackSerializer.Deserialize<T[]>(MemoryPackSerializer.Serialize(value));
            CollectionAssert.AreEqual(value, v);
        }

        [Test]
        public void Vector()
        {
            var v3 = new Vector3(10.2f, 12.34f, 1.9f);
            ConvertEqual(v3);

            var v3Array = new[]{
                new Vector3(10.2f, 12.34f, 1.9f),
                new Vector3(30.2f, 11.34f, 10.9f),
                new Vector3(980.2f, 1231.34f, 4.9f)
            };
            ConvertCollectionEqual(v3Array);

            var vv = new Vector3ArrayValue()
            {
                Array = v3Array
            };

            var vv2 = Convert(vv);
            CollectionAssert.AreEqual(vv2.Array, vv.Array);
        }
    }
}
