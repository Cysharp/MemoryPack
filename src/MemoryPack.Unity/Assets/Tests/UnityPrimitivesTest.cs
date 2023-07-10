using System;
using System.Linq;
using Assets.Scripts.MemoryPackObjects;
using MemoryPack;
using NUnit.Framework;
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

        private void ConvertEqual<T>(T value, byte[] raw)
        {
            var actual = MemoryPackSerializer.Serialize(value);
            CollectionAssert.AreEqual(raw, actual);

            var v = MemoryPackSerializer.Deserialize<T>(actual);
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

        [Test]
        public void KeyFrame()
        {
            var kf1 = new Keyframe(1.23f, 4.56f);
            ConvertEqual(kf1);

            var kf2 = new Keyframe(1.23f, 4.56f, 7.89f, 10.123f);
            ConvertEqual(kf2);

            var kf3 = new Keyframe(1.23f, 4.56f, 7.89f, 10.123f, 9.87f, 6.54f);
            ConvertEqual(kf3);

            var kfArr = new Keyframe[3]
            {
                kf1, kf2, kf3
            };
            CollectionAssert.AreEqual(kfArr, Convert(kfArr));
        }

        [Test]
        public void AnimationCurve()
        {
            var curve = new AnimationCurve
            {
                preWrapMode = WrapMode.Loop,
                postWrapMode = WrapMode.ClampForever,
                keys = new Keyframe[3]
                {
                    new Keyframe(1.23f, 4.56f),
                    new Keyframe(1.23f, 4.56f, 7.8f, 0.9f),
                    new Keyframe(1.23f, 4.56f, 7.8f, 0.9f, 1f, 3.5f),
                },
            };

            string hexString2 = "03-02-00-00-00-08-00-00-00-03-00-00-00-A4-70-9D-3F-85-EB-91-40-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-A4-70-9D-3F-85-EB-91-40-9A-99-F9-40-66-66-66-3F-00-00-00-00-00-00-00-00-00-00-00-00-A4-70-9D-3F-85-EB-91-40-9A-99-F9-40-66-66-66-3F-03-00-00-00-00-00-80-3F-00-00-60-40";
            byte[] byteArray2 = hexString2.Split('-').Select(s => System.Convert.ToByte(s, 16)).ToArray();
            ConvertEqual(curve, byteArray2);
        }
    }
}
