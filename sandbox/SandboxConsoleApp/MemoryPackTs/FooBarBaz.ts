import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";
import { Hoge } from "./Hoge.js"; 
import { Sonota1 } from "./Sonota1.js"; 
import { Sonota2 } from "./Sonota2.js"; 

export class FooBarBaz {
    hogeDozo: Hoge;
    bytesProp: Uint8Array | null;
    yoStarDearYomoda: string | null;
    myPropertyArray: number[] | null;
    myPropertyArray2: (number[] | null)[] | null;
    myProperty4: number | null;
    dictman: Map<number, (number | null)[] | null> | null;
    setMan: Set<number> | null;
    sonotaProp: Sonota1 | null;
    guid: string | null;
    dtt: Date;
    sonotaProp2: Sonota2 | null;

    public constructor() {
        this.hogeDozo = 0;
        this.bytesProp = null;
        this.yoStarDearYomoda = null;
        this.myPropertyArray = null;
        this.myPropertyArray2 = null;
        this.myProperty4 = null;
        this.dictman = null;
        this.setMan = null;
        this.sonotaProp = null;
        this.guid = null;
        this.dtt = new Date(0);
        this.sonotaProp2 = null;

    }

    static serialize(value: FooBarBaz | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: FooBarBaz | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(12);
        writer.writeInt8(value.hogeDozo);
        writer.writeBytes(value.bytesProp);
        writer.writeString(value.yoStarDearYomoda);
        writer.writeArray(value.myPropertyArray, (writer, x) => writer.writeInt32(x));
        writer.writeArray(value.myPropertyArray2, (writer, x) => writer.writeArray(x, (writer, x) => writer.writeInt32(x)));
        writer.writeNullableInt32(value.myProperty4);
        writer.writeMap(value.dictman, (writer, x) => writer.writeInt32(x), (writer, x) => writer.writeArray(x, (writer, x) => writer.writeNullableInt32(x)));
        writer.writeSet(value.setMan, (writer, x) => writer.writeInt32(x));
        Sonota1.serializeCore(writer, value.sonotaProp);
        writer.writeGuid(value.guid);
        writer.writeDate(value.dtt);
        Sonota2.serializeCore(writer, value.sonotaProp2);

    }

    static deserialize(buffer: ArrayBuffer): FooBarBaz | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): FooBarBaz | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        var value = new FooBarBaz();
        if (count == 12) {
            value.hogeDozo = reader.readInt8();
            value.bytesProp = reader.readBytes();
            value.yoStarDearYomoda = reader.readString();
            value.myPropertyArray = reader.readArray(reader => reader.readInt32());
            value.myPropertyArray2 = reader.readArray(reader => reader.readArray(reader => reader.readInt32()));
            value.myProperty4 = reader.readNullableInt32();
            value.dictman = reader.readMap(reader => reader.readInt32(), reader => reader.readArray(reader => reader.readNullableInt32()));
            value.setMan = reader.readSet(reader => reader.readInt32());
            value.sonotaProp = Sonota1.deserializeCore(reader);
            value.guid = reader.readGuid();
            value.dtt = reader.readDate();
            value.sonotaProp2 = Sonota2.deserializeCore(reader);

        }
        else if (count > 12) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.hogeDozo = reader.readInt8(); if (count == 1) return value;
            value.bytesProp = reader.readBytes(); if (count == 2) return value;
            value.yoStarDearYomoda = reader.readString(); if (count == 3) return value;
            value.myPropertyArray = reader.readArray(reader => reader.readInt32()); if (count == 4) return value;
            value.myPropertyArray2 = reader.readArray(reader => reader.readArray(reader => reader.readInt32())); if (count == 5) return value;
            value.myProperty4 = reader.readNullableInt32(); if (count == 6) return value;
            value.dictman = reader.readMap(reader => reader.readInt32(), reader => reader.readArray(reader => reader.readNullableInt32())); if (count == 7) return value;
            value.setMan = reader.readSet(reader => reader.readInt32()); if (count == 8) return value;
            value.sonotaProp = Sonota1.deserializeCore(reader); if (count == 9) return value;
            value.guid = reader.readGuid(); if (count == 10) return value;
            value.dtt = reader.readDate(); if (count == 11) return value;
            value.sonotaProp2 = Sonota2.deserializeCore(reader); if (count == 12) return value;

        }
        return value;
    }
}
