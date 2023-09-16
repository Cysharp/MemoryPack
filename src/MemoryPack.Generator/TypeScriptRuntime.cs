namespace MemoryPack.Generator;

public static class TypeScriptRuntime
{
    public static readonly string MemoryPackWriter = """
// Code of MemoryPack
const nullCollection = -1;
const union = 254;
const nullObject = 255;

// DateTimeOffset.FromUnixTimeMilliseconds(0).Ticks
const unixEpochTicks = 621355968000000000n;

// 01-62 bit represents ticks
// 63-64 bit represents DateTimeKind(we trim kind)
const dateTimeMask = 0b00111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111n;

export class MemoryPackWriter {
    static singletonWriter: MemoryPackWriter;

    public static getSharedInstance(): MemoryPackWriter {
        if (this.singletonWriter == null) {
            this.singletonWriter = new MemoryPackWriter();
        }
        this.singletonWriter.clear();
        return this.singletonWriter;
    }

    private buffer: Uint8Array
    private dataView: DataView;
    private utf8Encoder: TextEncoder | null;
    private offset: number

    public constructor(initialCapacity: number = 256) {
        this.buffer = new Uint8Array(initialCapacity);
        this.dataView = new DataView(this.buffer.buffer);
        this.utf8Encoder = null;
        this.offset = 0;
    }

    private ensureCapacity(count: number) {
        if (this.buffer.length - this.offset < count) {
            var nextCapacity = this.buffer.length;
            var to = this.offset + count;

            while (nextCapacity < to) {
                nextCapacity = nextCapacity * 2;
            }

            var nextBuffer = new Uint8Array(nextCapacity);
            nextBuffer.set(this.buffer);

            this.buffer = nextBuffer;
            this.dataView = new DataView(this.buffer.buffer);
        }
    }

    private clearBuffer(count: number): void {
        this.ensureCapacity(count);
        while (count >= 4) {
            this.dataView.setUint32(this.offset, 0, true);
            this.offset += 4;
            count -= 4;
        }
        if (count >= 2) {
            this.dataView.setUint16(this.offset, 0, true);
            this.offset += 2;
            count -= 2;
        }
        if (count >= 1) {
            this.dataView.setUint8(this.offset, 0);
            this.offset += 1;
        }
    }

    public writeObjectHeader(memberCount: number) {
        this.writeUint8(memberCount);
    }

    public writeNullObjectHeader(): void {
        this.writeUint8(nullObject);
    }

    public writeUnionHeader(tag: number) {
        if (tag < 250) {
            this.writeUint8(tag);
        } else {
            this.writeUint8(250);
            this.writeUint16(tag);
        }
    }

    public writeNullUnionHeader(): void {
        this.writeUint8(nullObject);
    }

    public writeCollectionHeader(length: number) {
        this.writeInt32(length);
    }

    public writeNullCollectionHeader() {
        this.writeInt32(nullCollection);
    }

    public writeBoolean(value: boolean): void {
        this.writeUint8(value ? 1 : 0);
    }

    public writeNullableBoolean(value: boolean | null): void {
        if (value == null) {
            this.clearBuffer(2);
            return;
        }
        this.writeNullableUint8(value ? 1 : 0);
    }

    public writeUint8(value: number): void {
        this.ensureCapacity(1);
        this.dataView.setUint8(this.offset, value);
        this.offset += 1;
    }

    public writeNullableUint8(value: number | null): void {
        if (value == null) {
            this.clearBuffer(2);
            return;
        }

        this.writeUint8(1);
        this.writeUint8(value);
    }

    public writeUint16(value: number): void {
        this.ensureCapacity(2);
        this.dataView.setUint16(this.offset, value, true);
        this.offset += 2;
    }

    public writeNullableUint16(value: number | null): void {
        if (value == null) {
            this.clearBuffer(4);
            return;
        }

        this.writeUint16(1);
        this.writeUint16(value);
    }

    public writeUint32(value: number): void {
        this.ensureCapacity(4);
        this.dataView.setUint32(this.offset, value, true);
        this.offset += 4;
    }

    public writeNullableUint32(value: number | null): void {
        if (value == null) {
            this.clearBuffer(8);
            return;
        }

        this.writeUint32(1);
        this.writeUint32(value);
    }

    public writeInt8(value: number): void {
        this.ensureCapacity(1);
        this.dataView.setInt8(this.offset, value);
        this.offset += 1;
    }

    public writeNullableInt8(value: number | null): void {
        if (value == null) {
            this.clearBuffer(2);
            return;
        }

        this.writeInt8(1);
        this.writeInt8(value);
    }

    public writeInt16(value: number): void {
        this.ensureCapacity(2);
        this.dataView.setInt16(this.offset, value, true);
        this.offset += 2;
    }

    public writeNullableInt16(value: number | null): void {
        if (value == null) {
            this.clearBuffer(4);
            return;
        }

        this.writeInt16(1);
        this.writeInt16(value);
    }

    public writeInt32(value: number): void {
        this.ensureCapacity(4);
        this.dataView.setInt32(this.offset, value, true);
        this.offset += 4;
    }

    public writeNullableInt32(value: number | null): void {
        if (value == null) {
            this.clearBuffer(8);
            return;
        }
        this.writeInt32(1);
        this.writeInt32(value);
    }

    public writeInt64(value: bigint): void {
        this.ensureCapacity(8);
        this.dataView.setBigInt64(this.offset, value, true);
        this.offset += 8;
    }

    public writeNullableInt64(value: bigint | null): void {
        if (value == null) {
            this.clearBuffer(16);
            return;
        }

        this.writeInt64(1n);
        this.writeInt64(value);
    }

    public writeUint64(value: bigint): void {
        this.ensureCapacity(8);
        this.dataView.setBigUint64(this.offset, value, true);
        this.offset += 8;
    }

    public writeNullableUint64(value: bigint | null): void {
        if (value == null) {
            this.clearBuffer(16);
            return;
        }

        this.writeUint64(1n);
        this.writeUint64(value);
    }

    public writeFloat32(value: number): void {
        this.ensureCapacity(4);
        this.dataView.setFloat32(this.offset, value, true);
        this.offset += 4;
    }

    public writeNullableFloat32(value: number | null): void {
        if (value == null) {
            this.clearBuffer(8);
            return;
        }

        this.writeFloat32(1);
        this.writeFloat32(value);
    }

    public writeFloat64(value: number): void {
        this.ensureCapacity(8);
        this.dataView.setFloat64(this.offset, value, true);
        this.offset += 8;
    }

    public writeNullableFloat64(value: number | null): void {
        if (value == null) {
            this.clearBuffer(16);
            return;
        }

        this.writeFloat64(1);
        this.writeFloat64(value);
    }

    public writeString(value: string | null): void {
        if (value == null) {
            this.writeNullCollectionHeader();
            return;
        }

        // (int ~utf8-byte-count, int utf16-length, utf8-bytes)
        this.ensureCapacity(8 + ((value.length + 1) * 3));

        if (this.utf8Encoder == null) {
            this.utf8Encoder = new TextEncoder();
        }

        var encodeResult = this.utf8Encoder.encodeInto(value, this.buffer.subarray(this.offset + 8));
        if (encodeResult.written === undefined || encodeResult.read === undefined) {
            throw new Error("failed utf8 TextEncoder.encodeInto.");
        }
        this.dataView.setInt32(this.offset, ~encodeResult.written, true);
        this.dataView.setInt32(this.offset + 4, encodeResult.read, true);

        this.offset += (encodeResult.written + 8);
    }

    public writeArray<T>(value: ArrayLike<T> | null, elementWriter: (writer: MemoryPackWriter, element: T) => void): void {
        if (value == null) {
            this.writeNullCollectionHeader();
            return;
        }

        this.writeCollectionHeader(value.length);
        var len = value.length;
        for (var i = 0; i < len; i++) {
            elementWriter(this, value[i]);
        }
    }

    public writeMap<K, V>(value: Map<K, V> | null, keyWriter: (writer: MemoryPackWriter, key: K) => void, valueWriter: (writer: MemoryPackWriter, value: V) => void): void {
        if (value == null) {
            this.writeNullCollectionHeader();
            return;
        }

        this.writeCollectionHeader(value.size);
        value.forEach((v, k) => {
            keyWriter(this, k);
            valueWriter(this, v);
        });
    }

    public writeSet<T>(value: Set<T> | null, elementWriter: (writer: MemoryPackWriter, element: T) => void): void {
        if (value == null) {
            this.writeNullCollectionHeader();
            return;
        }

        this.writeCollectionHeader(value.size);
        value.forEach(x => {
            elementWriter(this, x);
        });
    }

    public writeGuid(value: string): void {
        // e.g. "CA761232-ED42-11CE-BACD-00AA0057B223"

        // int _a;
        // short _b;
        // short _c;
        // byte _d;
        // byte _e;
        // byte _f;
        // byte _g;
        // byte _h;
        // byte _i;
        // byte _j;
        // byte _k;

        const b1 = parseInt(value.slice(0, 2), 16);
        const b2 = parseInt(value.slice(2, 4), 16);
        const b3 = parseInt(value.slice(4, 6), 16);
        const b4 = parseInt(value.slice(6, 8), 16);
        const a = b1 << 24 | b2 << 16 | b3 << 8 | b4;

        const b5 = parseInt(value.slice(9, 11), 16);
        const b6 = parseInt(value.slice(11, 13), 16);
        const b = b5 << 8 | b6;

        const b7 = parseInt(value.slice(14, 16), 16);
        const b8 = parseInt(value.slice(16, 18), 16);
        const c = b7 << 8 | b8;

        const d = parseInt(value.slice(19, 21), 16);
        const e = parseInt(value.slice(21, 23), 16);

        const f = parseInt(value.slice(24, 26), 16);
        const g = parseInt(value.slice(26, 28), 16);
        const h = parseInt(value.slice(28, 30), 16);
        const i = parseInt(value.slice(30, 32), 16);
        const j = parseInt(value.slice(32, 34), 16);
        const k = parseInt(value.slice(34, 36), 16);

        this.ensureCapacity(16);

        this.dataView.setInt32(this.offset, a, true);
        this.dataView.setInt16(this.offset + 4, b, true);
        this.dataView.setInt16(this.offset + 6, c, true);
        this.dataView.setUint8(this.offset + 8, d);
        this.dataView.setUint8(this.offset + 9, e);
        this.dataView.setUint8(this.offset + 10, f);
        this.dataView.setUint8(this.offset + 11, g);
        this.dataView.setUint8(this.offset + 12, h);
        this.dataView.setUint8(this.offset + 13, i);
        this.dataView.setUint8(this.offset + 14, j);
        this.dataView.setUint8(this.offset + 15, k);

        this.offset += 16;
    }

    public writeNullableGuid(value: string | null): void {
        if (value == null) {
            this.clearBuffer(20);
            return;
        }

        this.writeInt32(1);
        this.writeGuid(value);
    }

    public writeDate(value: Date): void {
        // Date.getTime is UTC Unix time of millisecond
        // .NET Ticks(ulong dateData) is 100-nanosecond from 1/1/0001 12:00am
        const unixMillisecond = BigInt(value.getTime());
        const ticks = unixMillisecond * 10000n + unixEpochTicks;
        this.writeUint64(ticks & dateTimeMask);
    }

    public writeNullableDate(value: Date | null): void {
        if (value == null) {
            this.clearBuffer(16);
            return;
        }

        this.writeInt64(1n);
        this.writeDate(value);
    }

    public writeUint8Array(value: Uint8Array | null): void {
        if (value == null) {
            this.writeNullCollectionHeader();
            return;
        }

        this.ensureCapacity(value.length + 4);
        this.dataView.setInt32(this.offset, value.length, true);
        this.buffer.set(value, this.offset + 4);
        this.offset += (value.length + 4);
    }

    public clear(): void {
        this.offset = 0;
    }

    public getSpan(): Uint8Array {
        return this.buffer.subarray(0, this.offset);
    }

    public toArray(): Uint8Array {
        return this.buffer.slice(0, this.offset);
    }
}

""";

    public static readonly string MemoryPackReader = """
// Code of MemoryPack
const nullCollection = -1;
const union = 254;
const nullObject = 255;

// bool
const TRUE = 1;
const FALSE = 0;

// DateTimeOffset.FromUnixTimeMilliseconds(0).Ticks
const unixEpochTicks = 621355968000000000n;

// 01-62 bit represents ticks
// 63-64 bit represents DateTimeKind(we trim kind)
const dateTimeMask = 0b00111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111n;

export class MemoryPackReader {
    private buffer: ArrayBuffer
    private dataView: DataView
    private utf8Decoder: TextDecoder | null
    private utf16Decoder: TextDecoder | null
    private offset: number

    public constructor(buffer: ArrayBuffer) {
        this.buffer = buffer;
        this.dataView = new DataView(this.buffer);
        this.utf8Decoder = null;
        this.utf16Decoder = null;
        this.offset = 0;
    }

    public tryReadObjectHeader(): [boolean, number] {
        const memberCount = this.readUint8();
        return (memberCount == nullObject)
            ? [false, 0]
            : [true, memberCount];
    }

    public tryReadUnionHeader(): [boolean, number] {
        const tag = this.readUint8();
        if (tag < 250) {
            return [true, tag];
        }
        else if (tag == 250) {
            const tag2 = this.readUint16();
            return [true, tag2];
        }
        else {
            return [false, 0];
        }
    }

    public tryReadCollectionHeader(): [boolean, number] {
        const length = this.readInt32();
        if (length == nullCollection) {
            return [false, 0];
        }

        if ((this.buffer.byteLength - this.offset) < length) {
            throw new Error("header length is too large, length: " + length);
        }

        return [true, length];
    }

    public readBoolean(): boolean {
        return this.readUint8() != 0;
    }

    public readNullableBoolean(): boolean | null {
        if (this.readUint8() == FALSE) {
            this.offset += 1;
            return null;
        }
        return this.readUint8() == TRUE;
    }

    public readUint8(): number {
        const v = this.dataView.getUint8(this.offset);
        this.offset += 1;
        return v;
    }

    public readNullableUint8(): number | null {
        if (this.readUint8() == FALSE) {
            this.offset += 1;
            return null;
        }

        return this.readUint8();
    }

    public readUint16(): number {
        const v = this.dataView.getUint16(this.offset, true);
        this.offset += 2;
        return v;
    }

    public readNullableUint16(): number | null {
        if (this.readUint16() == FALSE) {
            this.offset += 2;
            return null;
        }

        return this.readUint16();
    }

    public readUint32(): number {
        const v = this.dataView.getUint32(this.offset, true);
        this.offset += 4;
        return v;
    }

    public readNullableUint32(): number | null {
        if (this.readUint32() == FALSE) {
            this.offset += 4;
            return null;
        }

        return this.readUint32();
    }

    public readInt8(): number {
        const v = this.dataView.getInt8(this.offset);
        this.offset += 1;
        return v;
    }

    public readNullableInt8(): number | null {
        if (this.readInt8() == FALSE) {
            this.offset += 1;
            return null;
        }

        return this.readInt8();
    }

    public readInt16(): number {
        const v = this.dataView.getInt16(this.offset, true);
        this.offset += 2;
        return v;
    }

    public readNullableInt16(): number | null {
        if (this.readInt16() == FALSE) {
            this.offset += 2;
            return null;
        }

        return this.readInt16();
    }

    public readInt32(): number {
        const v = this.dataView.getInt32(this.offset, true);
        this.offset += 4;
        return v;
    }

    public readNullableInt32(): number | null {
        if (this.readInt32() == FALSE) {
            this.offset += 4;
            return null;
        }

        return this.readInt32();
    }

    public readInt64(): bigint {
        const v = this.dataView.getBigInt64(this.offset, true);
        this.offset += 8;
        return v;
    }

    public readNullableInt64(): bigint | null {
        if (this.readInt64() == 0n) {
            this.offset += 8;
            return null;
        }

        return this.readInt64();
    }

    public readUint64(): bigint {
        const v = this.dataView.getBigUint64(this.offset, true);
        this.offset += 8;
        return v;
    }

    public readNullableUint64(): bigint | null {
        if (this.readUint64() == 0n) {
            this.offset += 8;
            return null;
        }

        return this.readUint64();
    }

    public readFloat32(): number {
        const v = this.dataView.getFloat32(this.offset, true);
        this.offset += 4;
        return v;
    }

    public readNullableFloat32(): number | null {
        if (this.readFloat32() == FALSE) {
            this.offset += 4;
            return null;
        }

        return this.readFloat32();
    }

    public readFloat64(): number {
        const v = this.dataView.getFloat64(this.offset, true);
        this.offset += 8;
        return v;
    }

    public readNullableFloat64(): number | null {
        if (this.readFloat64() == FALSE) {
            this.offset += 8;
            return null;
        }

        return this.readFloat64();
    }

    public readString(): string | null {
        const [ok, length] = this.tryReadCollectionHeader();
        if (!ok) {
            return null;
        }

        if (length == 0) {
            return "";
        }

        if (length > 0) {
            if (this.utf16Decoder == null) {
                this.utf16Decoder = new TextDecoder("utf-16", { ignoreBOM: true, fatal: true });
            }

            const byteLength = length * 2;
            const view = new Uint8Array(this.buffer, this.offset, byteLength); // don't use slice, it makes copy
            const v = this.utf16Decoder.decode(view);
            this.offset += byteLength;
            return v;
        }
        else {
            if (this.utf8Decoder == null) {
                this.utf8Decoder = new TextDecoder("utf-8", { ignoreBOM: true, fatal: true });
            }

            // (int ~utf8-byte-count, int utf16-length, utf8-bytes)
            const utf8Length = ~length;
            this.offset += 4; // utf16-length, no use

            const view = new Uint8Array(this.buffer, this.offset, utf8Length); // don't use slice, it makes copy
            const v = this.utf8Decoder.decode(view);
            this.offset += utf8Length;
            return v;
        }
    }

    public readArray<T>(elementReader: (reader: MemoryPackReader) => T): T[] | null {
        const [ok, length] = this.tryReadCollectionHeader();
        if (!ok) {
            return null;
        }

        const result = new Array<T>(length);
        for (var i = 0; i < result.length; i++) {
            result[i] = elementReader(this);
        }
        return result;
    }

    public readMap<K, V>(keyReader: (reader: MemoryPackReader) => K, valueReader: (reader: MemoryPackReader) => V): Map<K, V> | null {
        const [ok, length] = this.tryReadCollectionHeader();
        if (!ok) {
            return null;
        }

        const result = new Map<K, V>();

        for (var i = 0; i < length; i++) {
            const key = keyReader(this);
            const value = valueReader(this);
            result.set(key, value);
        }

        return result;
    }

    public readSet<T>(elementReader: (reader: MemoryPackReader) => T): Set<T> | null {
        const [ok, length] = this.tryReadCollectionHeader();
        if (!ok) {
            return null;
        }

        const result = new Set<T>();
        for (var i = 0; i < length; i++) {
            result.add(elementReader(this));
        }
        return result;
    }

    public readGuid(): string {
        // e.g. "CA761232-ED42-11CE-BACD-00AA0057B223"

        // int _a;
        // short _b;
        // short _c;
        // byte _d;
        // byte _e;
        // byte _f;
        // byte _g;
        // byte _h;
        // byte _i;
        // byte _j;
        // byte _k;

        const b1 = this.readUint8();
        const b2 = this.readUint8();
        const b3 = this.readUint8();
        const b4 = this.readUint8();
        const b5 = this.readUint8();
        const b6 = this.readUint8();
        const b7 = this.readUint8();
        const b8 = this.readUint8();
        const b9 = this.readUint8();
        const b10 = this.readUint8();
        const b11 = this.readUint8();
        const b12 = this.readUint8();
        const b13 = this.readUint8();
        const b14 = this.readUint8();
        const b15 = this.readUint8();
        const b16 = this.readUint8();

        return b4.toString(16).padStart(2, "0") + b3.toString(16).padStart(2, "0") + b2.toString(16).padStart(2, "0") + b1.toString(16).padStart(2, "0") // a
            + "-"
            + b6.toString(16).padStart(2, "0") + b5.toString(16).padStart(2, "0") // b
            + "-"
            + b8.toString(16).padStart(2, "0") + b7.toString(16).padStart(2, "0") // c
            + "-"
            + b9.toString(16).padStart(2, "0") + b10.toString(16).padStart(2, "0") // d e
            + "-"
            + b11.toString(16).padStart(2, "0") + b12.toString(16).padStart(2, "0") // f g
            + b13.toString(16).padStart(2, "0") + b14.toString(16).padStart(2, "0") // h i
            + b15.toString(16).padStart(2, "0") + b16.toString(16).padStart(2, "0"); // j k
    }

    public readNullableGuid(): string | null {
        if (this.readInt32() == FALSE) {
            this.offset += 16;
            return null;
        }

        return this.readGuid();
    }

    public readDate(): Date {
        // Date.getTime is UTC Unix time of millisecond
        // .NET Ticks(ulong dateData) is 100-nanosecond from 1/1/0001 12:00am
        const ticks = this.readUint64() & dateTimeMask;
        const unixMillisecond = (ticks - unixEpochTicks) / 10000n;
        return new Date(Number(unixMillisecond));
    }

    public readNullableDate(): Date | null {
        if (this.readInt64() == 0n) {
            this.offset += 8;
            return null;
        }

        return this.readDate();
    }

    public readUint8Array(): Uint8Array | null {
        const [ok, length] = this.tryReadCollectionHeader();
        if (!ok) {
            return null;
        }

        const span = this.buffer.slice(this.offset, this.offset + length); // slice is copy
        this.offset += length;
        return new Uint8Array(span);
    }
}

""";
}
