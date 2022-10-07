// Code of MemoryPack
const nullCollection = -1;
const union = 254;
const nullObject = 255;

// DateTimeOffset.FromUnixTimeMilliseconds(0).Ticks
const unixEpochTicks = 621355968000000000n;

// 01-62 bit represents ticks
// 63-64 bit represents DateTimeKind(we trim kind)
const dateTimeMask = 0b00111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111n;

export class MemoryPackReader {
    private buffer: ArrayBuffer
    private dataView: DataView
    private utf8Decoder: TextDecoder
    private utf16Decoder: TextDecoder
    private offset: number

    public constructor(buffer: ArrayBuffer) {
        this.buffer = buffer;
        this.dataView = new DataView(this.buffer);
        this.utf8Decoder = new TextDecoder("utf-8", { ignoreBOM: true });
        this.utf16Decoder = new TextDecoder("utf-16", { ignoreBOM: true });
        this.offset = 0;
    }

    public tryReadObjectHeader(): [boolean, number] {
        const memberCount = this.readInt32();
        return (memberCount == nullObject)
            ? [false, 0]
            : [true, memberCount];
    }

    public tryReadUnionHeader(): [boolean, number] {
        const code = this.readUint8();
        if (code != union) {
            return [false, 0];
        }

        const tag = this.readUint8();
        return [true, tag];
    }

    public tryReadCollectionHeader(): [boolean, number] {
        const length = this.readInt32();
        if (length == nullCollection) {
            return [false, 0];
        }

        // TODO: if (Remaining < length) { throw Error }

        return [true, length];
    }

    // TODO: read primitives, nullable primitive

    public readUint8(): number {
        const v = this.dataView.getUint8(this.offset);
        this.offset += 1;
        return v;
    }

    public readInt32(): number {
        const v = this.dataView.getInt32(this.offset, true);
        this.offset += 4;
        return v;
    }

    public readUint64(): bigint {
        const v = this.dataView.getBigUint64(this.offset, true);
        this.offset += 8;
        return v;
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
            const byteLength = length * 2;
            const v = this.utf16Decoder.decode(this.buffer.slice(this.offset, this.offset + byteLength));
            this.offset += byteLength;
            return v;
        }
        else {
            // [utf8-length, utf16-length, utf8-value]
            const utf8Length = ~length;
            const utf16Length = this.readInt32(); // no use

            const v = this.utf8Decoder.decode(this.buffer.slice(this.offset, this.offset + utf8Length));
            this.offset += utf8Length;
            return v;
        }
    }

    public readArray<T>(elementReader: (reader: MemoryPackReader) => T | null): (T | null)[] | null {
        const [ok, length] = this.tryReadCollectionHeader();
        if (!ok) {
            return null;
        }

        const result = new Array<T | null>(length);
        for (var i = 0; i < result.length; i++) {
            result[i] = elementReader(this);
        }
        return result;
    }

    // TODO: readMap, readSet

    // TODO: readNullableGuid

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

    // TODO: readDate, readNullableDate

    public readDate(): Date {
        // Date.getTime is UTC Unix time of millisecond
        // .NET Ticks(ulong dateData) is 100-nanosecond from 1/1/0001 12:00am
        const ticks = this.readUint64() & dateTimeMask;
        const unixMillisecond = (ticks - unixEpochTicks) / 10000n;
        return new Date(Number(unixMillisecond));
    }

    // TODO: readBytes
}
