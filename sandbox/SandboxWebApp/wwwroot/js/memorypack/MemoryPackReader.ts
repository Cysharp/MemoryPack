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