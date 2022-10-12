import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";
import { NoMarkByteEnum } from "./NoMarkByteEnum.js"; 
import { NumberedUShortEnum } from "./NumberedUShortEnum.js"; 
import { NestedObject } from "./NestedObject.js"; 
import { IMogeUnion } from "./IMogeUnion.js"; 

export class AllConvertableType {
    myBool: boolean;
    myByte: number;
    mySByte: number;
    myShort: number;
    myInt: number;
    myLong: bigint;
    myUShort: number;
    myUInt: number;
    myULong: bigint;
    myFloat: number;
    myDouble: number;
    myGuid: string;
    myDate: Date;
    myEnum1: NoMarkByteEnum;
    myEnum2: NumberedUShortEnum;
    nullMyBool: boolean | null;
    nullMyByte: number | null;
    nullMySByte: number | null;
    nullMyShort: number | null;
    nullMyInt: number | null;
    nullMyLong: bigint | null;
    nullMyUShort: number | null;
    nullMyUInt: number | null;
    nullMyULong: bigint | null;
    nullMyFloat: number | null;
    nullMyDouble: number | null;
    nullMyGuid: string | null;
    nullMyDate: Date | null;
    nullMyEnum1: number | null;
    nullMyEnum2: number | null;
    myString: string | null;
    myBytes: Uint8Array | null;
    myIntArray: number[] | null;
    myStringArray: (string | null)[] | null;
    myList: number[] | null;
    myDictionary: Map<number, number> | null;
    mySet: Set<number> | null;
    myNestedNested: (Map<number, Set<(string | null)[] | null> | null> | null)[] | null;
    dictCheck2: Map<NoMarkByteEnum, boolean> | null;
    dictCheck3: Map<string, number | null> | null;
    dictCheck4X: Map<number, string | null> | null;
    nested1: NestedObject | null;
    union1: IMogeUnion | null;

    constructor() {
        this.myBool = false;
        this.myByte = 0;
        this.mySByte = 0;
        this.myShort = 0;
        this.myInt = 0;
        this.myLong = 0n;
        this.myUShort = 0;
        this.myUInt = 0;
        this.myULong = 0n;
        this.myFloat = 0;
        this.myDouble = 0;
        this.myGuid = "00000000-0000-0000-0000-000000000000";
        this.myDate = new Date(0);
        this.myEnum1 = 0;
        this.myEnum2 = 0;
        this.nullMyBool = null;
        this.nullMyByte = null;
        this.nullMySByte = null;
        this.nullMyShort = null;
        this.nullMyInt = null;
        this.nullMyLong = null;
        this.nullMyUShort = null;
        this.nullMyUInt = null;
        this.nullMyULong = null;
        this.nullMyFloat = null;
        this.nullMyDouble = null;
        this.nullMyGuid = null;
        this.nullMyDate = null;
        this.nullMyEnum1 = null;
        this.nullMyEnum2 = null;
        this.myString = null;
        this.myBytes = null;
        this.myIntArray = null;
        this.myStringArray = null;
        this.myList = null;
        this.myDictionary = null;
        this.mySet = null;
        this.myNestedNested = null;
        this.dictCheck2 = null;
        this.dictCheck3 = null;
        this.dictCheck4X = null;
        this.nested1 = null;
        this.union1 = null;

    }

    static serialize(value: AllConvertableType | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: AllConvertableType | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(43);
        writer.writeBoolean(value.myBool);
        writer.writeUint8(value.myByte);
        writer.writeInt8(value.mySByte);
        writer.writeInt16(value.myShort);
        writer.writeInt32(value.myInt);
        writer.writeInt64(value.myLong);
        writer.writeUint16(value.myUShort);
        writer.writeUint32(value.myUInt);
        writer.writeUint64(value.myULong);
        writer.writeFloat32(value.myFloat);
        writer.writeFloat64(value.myDouble);
        writer.writeGuid(value.myGuid);
        writer.writeDate(value.myDate);
        writer.writeUint8(value.myEnum1);
        writer.writeUint16(value.myEnum2);
        writer.writeNullableBoolean(value.nullMyBool);
        writer.writeNullableUint8(value.nullMyByte);
        writer.writeNullableInt8(value.nullMySByte);
        writer.writeNullableInt16(value.nullMyShort);
        writer.writeNullableInt32(value.nullMyInt);
        writer.writeNullableInt64(value.nullMyLong);
        writer.writeNullableUint16(value.nullMyUShort);
        writer.writeNullableUint32(value.nullMyUInt);
        writer.writeNullableUint64(value.nullMyULong);
        writer.writeNullableFloat32(value.nullMyFloat);
        writer.writeNullableFloat64(value.nullMyDouble);
        writer.writeNullableGuid(value.nullMyGuid);
        writer.writeNullableDate(value.nullMyDate);
        writer.writeNullableUint8(value.nullMyEnum1);
        writer.writeNullableUint16(value.nullMyEnum2);
        writer.writeString(value.myString);
        writer.writeUint8Array(value.myBytes);
        writer.writeArray(value.myIntArray, (writer, x) => writer.writeInt32(x));
        writer.writeArray(value.myStringArray, (writer, x) => writer.writeString(x));
        writer.writeArray(value.myList, (writer, x) => writer.writeInt32(x));
        writer.writeMap(value.myDictionary, (writer, x) => writer.writeInt32(x), (writer, x) => writer.writeInt32(x));
        writer.writeSet(value.mySet, (writer, x) => writer.writeInt32(x));
        writer.writeArray(value.myNestedNested, (writer, x) => writer.writeMap(x, (writer, x) => writer.writeInt32(x), (writer, x) => writer.writeSet(x, (writer, x) => writer.writeArray(x, (writer, x) => writer.writeString(x)))));
        writer.writeMap(value.dictCheck2, (writer, x) => writer.writeUint8(x), (writer, x) => writer.writeBoolean(x));
        writer.writeMap(value.dictCheck3, (writer, x) => writer.writeGuid(x), (writer, x) => writer.writeNullableInt32(x));
        writer.writeMap(value.dictCheck4X, (writer, x) => writer.writeInt32(x), (writer, x) => writer.writeString(x));
        NestedObject.serializeCore(writer, value.nested1);
        IMogeUnion.serializeCore(writer, value.union1);

    }

    static deserialize(buffer: ArrayBuffer): AllConvertableType | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): AllConvertableType | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        const value = new AllConvertableType();
        if (count == 43) {
            value.myBool = reader.readBoolean();
            value.myByte = reader.readUint8();
            value.mySByte = reader.readInt8();
            value.myShort = reader.readInt16();
            value.myInt = reader.readInt32();
            value.myLong = reader.readInt64();
            value.myUShort = reader.readUint16();
            value.myUInt = reader.readUint32();
            value.myULong = reader.readUint64();
            value.myFloat = reader.readFloat32();
            value.myDouble = reader.readFloat64();
            value.myGuid = reader.readGuid();
            value.myDate = reader.readDate();
            value.myEnum1 = reader.readUint8();
            value.myEnum2 = reader.readUint16();
            value.nullMyBool = reader.readNullableBoolean();
            value.nullMyByte = reader.readNullableUint8();
            value.nullMySByte = reader.readNullableInt8();
            value.nullMyShort = reader.readNullableInt16();
            value.nullMyInt = reader.readNullableInt32();
            value.nullMyLong = reader.readNullableInt64();
            value.nullMyUShort = reader.readNullableUint16();
            value.nullMyUInt = reader.readNullableUint32();
            value.nullMyULong = reader.readNullableUint64();
            value.nullMyFloat = reader.readNullableFloat32();
            value.nullMyDouble = reader.readNullableFloat64();
            value.nullMyGuid = reader.readNullableGuid();
            value.nullMyDate = reader.readNullableDate();
            value.nullMyEnum1 = reader.readNullableUint8();
            value.nullMyEnum2 = reader.readNullableUint16();
            value.myString = reader.readString();
            value.myBytes = reader.readUint8Array();
            value.myIntArray = reader.readArray(reader => reader.readInt32());
            value.myStringArray = reader.readArray(reader => reader.readString());
            value.myList = reader.readArray(reader => reader.readInt32());
            value.myDictionary = reader.readMap(reader => reader.readInt32(), reader => reader.readInt32());
            value.mySet = reader.readSet(reader => reader.readInt32());
            value.myNestedNested = reader.readArray(reader => reader.readMap(reader => reader.readInt32(), reader => reader.readSet(reader => reader.readArray(reader => reader.readString()))));
            value.dictCheck2 = reader.readMap(reader => reader.readUint8(), reader => reader.readBoolean());
            value.dictCheck3 = reader.readMap(reader => reader.readGuid(), reader => reader.readNullableInt32());
            value.dictCheck4X = reader.readMap(reader => reader.readInt32(), reader => reader.readString());
            value.nested1 = NestedObject.deserializeCore(reader);
            value.union1 = IMogeUnion.deserializeCore(reader);

        }
        else if (count > 43) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.myBool = reader.readBoolean(); if (count == 1) return value;
            value.myByte = reader.readUint8(); if (count == 2) return value;
            value.mySByte = reader.readInt8(); if (count == 3) return value;
            value.myShort = reader.readInt16(); if (count == 4) return value;
            value.myInt = reader.readInt32(); if (count == 5) return value;
            value.myLong = reader.readInt64(); if (count == 6) return value;
            value.myUShort = reader.readUint16(); if (count == 7) return value;
            value.myUInt = reader.readUint32(); if (count == 8) return value;
            value.myULong = reader.readUint64(); if (count == 9) return value;
            value.myFloat = reader.readFloat32(); if (count == 10) return value;
            value.myDouble = reader.readFloat64(); if (count == 11) return value;
            value.myGuid = reader.readGuid(); if (count == 12) return value;
            value.myDate = reader.readDate(); if (count == 13) return value;
            value.myEnum1 = reader.readUint8(); if (count == 14) return value;
            value.myEnum2 = reader.readUint16(); if (count == 15) return value;
            value.nullMyBool = reader.readNullableBoolean(); if (count == 16) return value;
            value.nullMyByte = reader.readNullableUint8(); if (count == 17) return value;
            value.nullMySByte = reader.readNullableInt8(); if (count == 18) return value;
            value.nullMyShort = reader.readNullableInt16(); if (count == 19) return value;
            value.nullMyInt = reader.readNullableInt32(); if (count == 20) return value;
            value.nullMyLong = reader.readNullableInt64(); if (count == 21) return value;
            value.nullMyUShort = reader.readNullableUint16(); if (count == 22) return value;
            value.nullMyUInt = reader.readNullableUint32(); if (count == 23) return value;
            value.nullMyULong = reader.readNullableUint64(); if (count == 24) return value;
            value.nullMyFloat = reader.readNullableFloat32(); if (count == 25) return value;
            value.nullMyDouble = reader.readNullableFloat64(); if (count == 26) return value;
            value.nullMyGuid = reader.readNullableGuid(); if (count == 27) return value;
            value.nullMyDate = reader.readNullableDate(); if (count == 28) return value;
            value.nullMyEnum1 = reader.readNullableUint8(); if (count == 29) return value;
            value.nullMyEnum2 = reader.readNullableUint16(); if (count == 30) return value;
            value.myString = reader.readString(); if (count == 31) return value;
            value.myBytes = reader.readUint8Array(); if (count == 32) return value;
            value.myIntArray = reader.readArray(reader => reader.readInt32()); if (count == 33) return value;
            value.myStringArray = reader.readArray(reader => reader.readString()); if (count == 34) return value;
            value.myList = reader.readArray(reader => reader.readInt32()); if (count == 35) return value;
            value.myDictionary = reader.readMap(reader => reader.readInt32(), reader => reader.readInt32()); if (count == 36) return value;
            value.mySet = reader.readSet(reader => reader.readInt32()); if (count == 37) return value;
            value.myNestedNested = reader.readArray(reader => reader.readMap(reader => reader.readInt32(), reader => reader.readSet(reader => reader.readArray(reader => reader.readString())))); if (count == 38) return value;
            value.dictCheck2 = reader.readMap(reader => reader.readUint8(), reader => reader.readBoolean()); if (count == 39) return value;
            value.dictCheck3 = reader.readMap(reader => reader.readGuid(), reader => reader.readNullableInt32()); if (count == 40) return value;
            value.dictCheck4X = reader.readMap(reader => reader.readInt32(), reader => reader.readString()); if (count == 41) return value;
            value.nested1 = NestedObject.deserializeCore(reader); if (count == 42) return value;
            value.union1 = IMogeUnion.deserializeCore(reader); if (count == 43) return value;

        }
        return value;
    }
}
