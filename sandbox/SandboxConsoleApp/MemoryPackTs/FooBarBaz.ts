import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";
import { Sonota1 } from "./Sonota1.js"; 

export class FooBarBaz {
    yoStarDearYomoda: string | null;
    myPropertyArray: number[] | null;
    myPropertyArray2: (number[] | null)[] | null;
    myProperty4: number | null;
    dictman: Map<number, (number | null)[] | null> | null;
    setMan: Set<number> | null;
    sonotaProp: Sonota1 | null;

    public constructor() {
        this.yoStarDearYomoda = "";
        this.myPropertyArray = null;
        this.myPropertyArray2 = null;
        this.myProperty4 = null;
        this.dictman = null;
        this.setMan = null;
        this.sonotaProp = null;

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

        writer.writeObjectHeader(7);
        writer.writeString(value.yoStarDearYomoda);
        writer.writeArray(value.myPropertyArray, (writer, x) => writer.writeInt32(x));
        writer.writeArray(value.myPropertyArray2, (writer, x) => writer.writeArray(x, (writer, x) => writer.writeInt32(x)));
        writer.writeNullableInt32(value.myProperty4);
        writer.writeMap(value.dictman, (writer, x) => writer.writeInt32(x), (writer, x) => writer.writeArray(x, (writer, x) => writer.writeNullableInt32(x)));
        writer.writeSet(value.setMan, (writer, x) => writer.writeInt32(x));
        Sonota1.serializeCore(writer, value.sonotaProp);

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
        if (count == 7) {
            value.yoStarDearYomoda = reader.readString();
            value.myPropertyArray = reader.readArray(reader => reader.readInt32());
            value.myPropertyArray2 = reader.readArray(reader => reader.readArray(reader => reader.readInt32()));
            value.myProperty4 = reader.readNullableInt32();
            value.dictman = reader.readMap(reader => reader.readInt32(), reader => reader.readArray(reader => reader.readNullableInt32()));
            value.setMan = reader.readSet(reader => reader.readInt32());
            value.sonotaProp = Sonota1.deserializeCore(reader);

        }
        else if (count > 7) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.yoStarDearYomoda = reader.readString(); if (count == 1) return value;
            value.myPropertyArray = reader.readArray(reader => reader.readInt32()); if (count == 2) return value;
            value.myPropertyArray2 = reader.readArray(reader => reader.readArray(reader => reader.readInt32())); if (count == 3) return value;
            value.myProperty4 = reader.readNullableInt32(); if (count == 4) return value;
            value.dictman = reader.readMap(reader => reader.readInt32(), reader => reader.readArray(reader => reader.readNullableInt32())); if (count == 5) return value;
            value.setMan = reader.readSet(reader => reader.readInt32()); if (count == 6) return value;
            value.sonotaProp = Sonota1.deserializeCore(reader); if (count == 7) return value;

        }
        return value;
    }
}
