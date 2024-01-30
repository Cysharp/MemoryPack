using Assets.Scripts.MemoryPackObjects;
using MemoryPack;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        [Test]
        public void Keyframe()
        {
#if UNITY_EDITOR
            Assert.AreEqual(32, Unsafe.SizeOf<Keyframe>());
#else
            Assert.AreEqual(28, Unsafe.SizeOf<Keyframe>());
#endif

            var keyframe = new Keyframe(1.23f, 4.56f);

            var raw = MemoryPackSerializer.Serialize(keyframe);

#if UNITY_EDITOR
            Assert.AreEqual("A4-70-9D-3F-85-EB-91-40-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00", BitConverter.ToString(raw));
#else
            Assert.AreEqual("A4-70-9D-3F-85-EB-91-40-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00", BitConverter.ToString(raw));
#endif

            var other = MemoryPackSerializer.Deserialize<Keyframe>(raw);
            Assert.AreEqual(1.23f, other.time, 0.001f);
            Assert.AreEqual(4.56f, other.value, 0.001f);
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

            var raw = MemoryPackSerializer.Serialize(curve);
#if UNITY_EDITOR
            Assert.AreEqual("03-02-00-00-00-08-00-00-00-03-00-00-00-A4-70-9D-3F-85-EB-91-40-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-A4-70-9D-3F-85-EB-91-40-9A-99-F9-40-66-66-66-3F-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-A4-70-9D-3F-85-EB-91-40-9A-99-F9-40-66-66-66-3F-00-00-00-00-03-00-00-00-00-00-80-3F-00-00-60-40", BitConverter.ToString(raw));
#else
            Assert.AreEqual("03-02-00-00-00-08-00-00-00-03-00-00-00-A4-70-9D-3F-85-EB-91-40-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-A4-70-9D-3F-85-EB-91-40-9A-99-F9-40-66-66-66-3F-00-00-00-00-00-00-00-00-00-00-00-00-A4-70-9D-3F-85-EB-91-40-9A-99-F9-40-66-66-66-3F-03-00-00-00-00-00-80-3F-00-00-60-40", BitConverter.ToString(raw));
#endif
            
            var curve1 = MemoryPackSerializer.Deserialize<AnimationCurve>(raw);
            Assert.NotNull(curve1);
            Assert.AreEqual(WrapMode.Loop, curve1.preWrapMode);
            Assert.AreEqual(WrapMode.ClampForever, curve1.postWrapMode);

            Assert.AreEqual(3, curve1.keys.Length);
            Assert.AreEqual(4.56f, curve1.keys[curve1!.keys.Length - 1].value, 0.001);
        }
    }
}
