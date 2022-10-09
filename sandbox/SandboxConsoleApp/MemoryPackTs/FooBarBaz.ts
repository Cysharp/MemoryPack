import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";

export class FooBarBaz {
    yoStarDearYomoda: string
    myPropertyArray: number[] | null
    myPropertyArray2: (number[] | null)[] | null
    myProperty4: number | null
    dictman: Map<number, (number | null)[] | null> | null
    setMan: Set<number> | null

    public constructor() {
        this.yoStarDearYomoda = "";
        this.myPropertyArray = null;
        this.myPropertyArray2 = null;
        this.myProperty4 = null;
        this.dictman = null;
        this.setMan = null;

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

        writer.writeObjectHeader(6);
        writer.writeString(value.yoStarDearYomoda);
        writer.writeArray(value.myPropertyArray, (writer, x) => writer.writeInt32(x));
        writer.writeArray(value.myPropertyArray2, (writer, x) => writer.writeArray(x, (writer, x) => writer.writeInt32(x)));
        writer.writeNullableInt32(value.myProperty4);
        writer.writeMap(value.dictman, (writer, x) => writer.writeInt32(x), (writer, x) => writer.writeArray(x, (writer, x) => writer.writeNullableInt32(x)));
        writer.writeSet(value.setMan, (writer, x) => writer.writeInt32(x));

    }

    static deserialize(buffer: ArrayBuffer): FooBarBaz | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): FooBarBaz | null {
        const [ok, memberCount] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        var value = new FooBarBaz();
        value.yoStarDearYomoda = reader.readString();
        value.myPropertyArray = reader.readArray(reader => reader.readInt32());
        value.myPropertyArray2 = reader.readArray(reader => reader.readArray(reader => reader.readInt32()));
        value.myProperty4 = reader.readNullableInt32();
        value.dictman = reader.readMap(reader => reader.readInt32(), reader => reader.readArray(reader => reader.readNullableInt32()));
        value.setMan = reader.readSet(reader => reader.readInt32());
        return value;

    }
}
