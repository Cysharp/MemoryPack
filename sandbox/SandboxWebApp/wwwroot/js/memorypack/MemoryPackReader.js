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
const dateTimeMask = 4611686018427387903n;
export class MemoryPackReader {
    buffer;
    dataView;
    utf8Decoder;
    utf16Decoder;
    offset;
    constructor(buffer) {
        this.buffer = buffer;
        this.dataView = new DataView(this.buffer);
        this.utf8Decoder = null;
        this.utf16Decoder = null;
        this.offset = 0;
    }
    tryReadObjectHeader() {
        const memberCount = this.readUint8();
        return (memberCount == nullObject)
            ? [false, 0]
            : [true, memberCount];
    }
    tryReadUnionHeader() {
        const tag = this.readUint8();
        return (tag == nullObject)
            ? [false, 0]
            : [true, tag];
    }
    tryReadCollectionHeader() {
        const length = this.readInt32();
        if (length == nullCollection) {
            return [false, 0];
        }
        if ((this.buffer.byteLength - this.offset) < length) {
            throw new Error("header length is too large, length: " + length);
        }
        return [true, length];
    }
    readBoolean() {
        return this.readUint8() != 0;
    }
    readNullableBoolean() {
        if (this.readUint8() == FALSE) {
            this.offset += 1;
            return null;
        }
        return this.readUint8() == TRUE;
    }
    readUint8() {
        const v = this.dataView.getUint8(this.offset);
        this.offset += 1;
        return v;
    }
    readNullableUint8() {
        if (this.readUint8() == FALSE) {
            this.offset += 1;
            return null;
        }
        return this.readUint8();
    }
    readUint16() {
        const v = this.dataView.getUint16(this.offset, true);
        this.offset += 2;
        return v;
    }
    readNullableUint16() {
        if (this.readUint16() == FALSE) {
            this.offset += 2;
            return null;
        }
        return this.readUint16();
    }
    readUint32() {
        const v = this.dataView.getUint32(this.offset, true);
        this.offset += 4;
        return v;
    }
    readNullableUint32() {
        if (this.readUint32() == FALSE) {
            this.offset += 4;
            return null;
        }
        return this.readUint32();
    }
    readInt8() {
        const v = this.dataView.getInt8(this.offset);
        this.offset += 1;
        return v;
    }
    readNullableInt8() {
        if (this.readInt8() == FALSE) {
            this.offset += 1;
            return null;
        }
        return this.readInt8();
    }
    readInt16() {
        const v = this.dataView.getInt16(this.offset, true);
        this.offset += 2;
        return v;
    }
    readNullableInt16() {
        if (this.readInt16() == FALSE) {
            this.offset += 2;
            return null;
        }
        return this.readInt16();
    }
    readInt32() {
        const v = this.dataView.getInt32(this.offset, true);
        this.offset += 4;
        return v;
    }
    readNullableInt32() {
        if (this.readInt32() == FALSE) {
            this.offset += 4;
            return null;
        }
        return this.readInt32();
    }
    readInt64() {
        const v = this.dataView.getBigInt64(this.offset, true);
        this.offset += 8;
        return v;
    }
    readNullableInt64() {
        if (this.readInt64() == 0n) {
            this.offset += 8;
            return null;
        }
        return this.readInt64();
    }
    readUint64() {
        const v = this.dataView.getBigUint64(this.offset, true);
        this.offset += 8;
        return v;
    }
    readNullableUint64() {
        if (this.readUint64() == 0n) {
            this.offset += 8;
            return null;
        }
        return this.readUint64();
    }
    readFloat32() {
        const v = this.dataView.getFloat32(this.offset, true);
        this.offset += 4;
        return v;
    }
    readNullableFloat32() {
        if (this.readFloat32() == FALSE) {
            this.offset += 4;
            return null;
        }
        return this.readFloat32();
    }
    readFloat64() {
        const v = this.dataView.getFloat64(this.offset, true);
        this.offset += 8;
        return v;
    }
    readNullableFloat64() {
        if (this.readFloat64() == FALSE) {
            this.offset += 8;
            return null;
        }
        return this.readFloat64();
    }
    readString() {
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
    readArray(elementReader) {
        const [ok, length] = this.tryReadCollectionHeader();
        if (!ok) {
            return null;
        }
        const result = new Array(length);
        for (var i = 0; i < result.length; i++) {
            result[i] = elementReader(this);
        }
        return result;
    }
    readMap(keyReader, valueReader, unmanagedStruct) {
        const [ok, length] = this.tryReadCollectionHeader();
        if (!ok) {
            return null;
        }
        const result = new Map();
        for (var i = 0; i < length; i++) {
            if (!unmanagedStruct) {
                const [headerOk, headerLength] = this.tryReadObjectHeader();
                if (!headerOk || headerLength != 2) {
                    throw new Error("Invalid header in map elements deserialize.");
                }
            }
            const key = keyReader(this);
            const value = valueReader(this);
            result.set(key, value);
        }
        return result;
    }
    readSet(elementReader) {
        const [ok, length] = this.tryReadCollectionHeader();
        if (!ok) {
            return null;
        }
        const result = new Set();
        for (var i = 0; i < length; i++) {
            result.add(elementReader(this));
        }
        return result;
    }
    readGuid() {
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
    readNullableGuid() {
        if (this.readInt32() == FALSE) {
            this.offset += 16;
            return null;
        }
        return this.readGuid();
    }
    readDate() {
        // Date.getTime is UTC Unix time of millisecond
        // .NET Ticks(ulong dateData) is 100-nanosecond from 1/1/0001 12:00am
        const ticks = this.readUint64() & dateTimeMask;
        const unixMillisecond = (ticks - unixEpochTicks) / 10000n;
        return new Date(Number(unixMillisecond));
    }
    readNullableDate() {
        if (this.readInt64() == 0n) {
            this.offset += 8;
            return null;
        }
        return this.readDate();
    }
    readUint8Array() {
        const [ok, length] = this.tryReadCollectionHeader();
        if (!ok) {
            return null;
        }
        const span = this.buffer.slice(this.offset, this.offset + length); // slice is copy
        this.offset += length;
        return new Uint8Array(span);
    }
}
//# sourceMappingURL=MemoryPackReader.js.map