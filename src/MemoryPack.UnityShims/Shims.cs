#pragma warning disable SA1307 // Field should begin with upper-case letter
#pragma warning disable SA1300 // Field should begin with upper-case letter
#pragma warning disable IDE1006 // Field should begin with upper-case letter
#pragma warning disable SA1649 // type name matches file name
#pragma warning disable SA1401 // Fields should be private (we need fields rather than auto-properties for .NET Native compilation to work).

using MemoryPack;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UnityEngine;

[MemoryPackable]
public partial struct Vector2
{
    public float x;
    public float y;

    public Vector2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator +(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x + b.x, a.y + b.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator -(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x - b.x, a.y - b.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator *(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x * b.x, a.y * b.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator /(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x / b.x, a.y / b.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator -(Vector2 a)
    {
        return new Vector2(0f - a.x, 0f - a.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator *(Vector2 a, float d)
    {
        return new Vector2(a.x * d, a.y * d);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator *(float d, Vector2 a)
    {
        return new Vector2(a.x * d, a.y * d);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator /(Vector2 a, float d)
    {
        return new Vector2(a.x / d, a.y / d);
    }
}


[MemoryPackable]
public partial struct Vector3
{
    public float x;
    public float y;
    public float z;

    public Vector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 operator +(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 operator -(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 operator -(Vector3 a)
    {
        return new Vector3(0f - a.x, 0f - a.y, 0f - a.z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 operator *(Vector3 a, float d)
    {
        return new Vector3(a.x * d, a.y * d, a.z * d);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 operator *(float d, Vector3 a)
    {
        return new Vector3(a.x * d, a.y * d, a.z * d);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 operator /(Vector3 a, float d)
    {
        return new Vector3(a.x / d, a.y / d, a.z / d);
    }
}

[MemoryPackable]
public partial struct Vector4
{
    public float x;
    public float y;
    public float z;
    public float w;

    public Vector4(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator +(Vector4 a, Vector4 b)
    {
        return new Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator -(Vector4 a, Vector4 b)
    {
        return new Vector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator -(Vector4 a)
    {
        return new Vector4(0f - a.x, 0f - a.y, 0f - a.z, 0f - a.w);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator *(Vector4 a, float d)
    {
        return new Vector4(a.x * d, a.y * d, a.z * d, a.w * d);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator *(float d, Vector4 a)
    {
        return new Vector4(a.x * d, a.y * d, a.z * d, a.w * d);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator /(Vector4 a, float d)
    {
        return new Vector4(a.x / d, a.y / d, a.z / d, a.w / d);
    }
}

[MemoryPackable]
public partial struct Quaternion
{
    public float x;
    public float y;
    public float z;
    public float w;

    public Quaternion(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }
}

[MemoryPackable]
public partial struct Color
{
    public float r;
    public float g;
    public float b;
    public float a;

    public Color(float r, float g, float b)
        : this(r, g, b, 1.0f)
    {
    }

    public Color(float r, float g, float b, float a)
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }
}

[MemoryPackable]
public partial struct Bounds
{
    public Vector3 center;
    public Vector3 extents;

    public Vector3 size
    {
        get
        {
            return this.extents * 2f;
        }

        set
        {
            this.extents = value * 0.5f;
        }
    }

    public Vector3 max
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return center + extents;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            SetMinMax(min, value);
        }
    }

    public Vector3 min
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return center - extents;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            SetMinMax(value, max);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetMinMax(Vector3 min, Vector3 max)
    {
        extents = (max - min) * 0.5f;
        center = min + extents;
    }
}

[MemoryPackable]
public partial struct Rect
{
    public float x;

    public float y;

    public float width;

    public float height;

    public Rect(float x, float y, float width, float height)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }

    public Rect(Vector2 position, Vector2 size)
    {
        this.x = position.x;
        this.y = position.y;
        this.width = size.x;
        this.height = size.y;
    }

    public Rect(Rect source)
    {
        this.x = source.x;
        this.y = source.y;
        this.width = source.width;
        this.height = source.height;
    }
}

// same order as ReadMe's SerializableAnimationCurve
[MemoryPackable]
public sealed partial class AnimationCurve
{
    public WrapMode preWrapMode;
    public WrapMode postWrapMode;
    public Keyframe[] keys = default!;

    [MemoryPack.MemoryPackIgnore]
    public int length
    {
        get { return this.keys.Length; }
    }
}

[MemoryPackable]
public partial struct Keyframe
{
    public float time;

    public float value;

    public float inTangent;

    public float outTangent;

    public int weightedMode;

    public float inWeight;

    public float outWeight;

    public Keyframe(float time, float value)
    {
        this.time = time;
        this.value = value;
        this.inTangent = 0f;
        this.outTangent = 0f;
    }

    public Keyframe(float time, float value, float inTangent, float outTangent)
    {
        this.time = time;
        this.value = value;
        this.inTangent = inTangent;
        this.outTangent = outTangent;
    }

    public Keyframe(float time, float value, float inTangent, float outTangent, float inWeight, float outWeight)
    {
        this.time = time;
        this.value = value;
        this.inTangent = inTangent;
        this.outTangent = outTangent;
        this.weightedMode = 3;
        this.inWeight = inWeight;
        this.outWeight = outWeight;
    }
}

public enum WrapMode
{
    Once = 1,
    Loop = 2,
    PingPong = 4,
    Default = 0,
    ClampForever = 8,
    Clamp = 1,
}

[MemoryPackable]
public partial struct Matrix4x4
{
    public float m00;
    public float m10;
    public float m20;
    public float m30;
    public float m01;
    public float m11;
    public float m21;
    public float m31;
    public float m02;
    public float m12;
    public float m22;
    public float m32;
    public float m03;
    public float m13;
    public float m23;
    public float m33;
}

[MemoryPackable]
public sealed partial class Gradient
{
    public GradientColorKey[] colorKeys = default!;

    public GradientAlphaKey[] alphaKeys = default!;

    public GradientMode mode;
}

public partial struct GradientColorKey
{
    public Color color;
    public float time;

    public GradientColorKey(Color col, float time)
    {
        this.color = col;
        this.time = time;
    }
}

[MemoryPackable]
public partial struct GradientAlphaKey
{
    public float alpha;
    public float time;

    public GradientAlphaKey(float alpha, float time)
    {
        this.alpha = alpha;
        this.time = time;
    }
}

public enum GradientMode
{
    Blend,
    Fixed,
}

[MemoryPackable]
[StructLayout(LayoutKind.Explicit)]
public partial struct Color32
{
    [FieldOffset(0)]
    private int rgba;

    [FieldOffset(0)]
    public byte r;

    [FieldOffset(1)]
    public byte g;

    [FieldOffset(2)]
    public byte b;

    [FieldOffset(3)]
    public byte a;

    public Color32(byte r, byte g, byte b, byte a)
    {
        this.rgba = 0;
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }
}

[MemoryPackable]
public sealed partial class RectOffset
{
    public int left;

    public int right;

    public int top;

    public int bottom;

    public RectOffset()
    {
    }

    [MemoryPack.MemoryPackConstructor]
    public RectOffset(int left, int right, int top, int bottom)
    {
        this.left = left;
        this.right = right;
        this.top = top;
        this.bottom = bottom;
    }
}

[MemoryPackable]
public partial struct LayerMask
{
    public int value;
}

[MemoryPackable]
public partial struct Vector2Int
{
    public int x;
    public int y;

    public Vector2Int(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

[MemoryPackable]
public partial struct Vector3Int
{
    public int x;
    public int y;
    public int z;

    public Vector3Int(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static Vector3Int operator *(Vector3Int a, int d)
    {
        return new Vector3Int(a.x * d, a.y * d, a.z * d);
    }
}

[MemoryPackable]
public partial struct RangeInt
{
    public int start;
    public int length;

    public RangeInt(int start, int length)
    {
        this.start = start;
        this.length = length;
    }
}

[MemoryPackable]
public partial struct RectInt
{
    public int x;

    public int y;

    public int width;

    public int height;

    public RectInt(int x, int y, int width, int height)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }

    public RectInt(Vector2Int position, Vector2Int size)
    {
        this.x = position.x;
        this.y = position.y;
        this.width = size.x;
        this.height = size.y;
    }

    public RectInt(RectInt source)
    {
        this.x = source.x;
        this.y = source.y;
        this.width = source.width;
        this.height = source.height;
    }
}


[MemoryPackable]
public partial struct BoundsInt
{
    public Vector3Int position;

    public Vector3Int size;

    public BoundsInt(Vector3Int position, Vector3Int size)
    {
        this.position = position;
        this.size = size;
    }
}
