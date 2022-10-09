import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";

export async function hoge() {
    var f = new Foo();
    f.age = 32;
    f.name = "hogemoge";
    f.guid = "CA761232-ED42-11CE-BACD-00AA0057B223";
    f.seq = [1, 10, 200, 300];

    var bin = Foo.serialize(f);

    var blob = new Blob([bin.buffer], { type: "application/x-memorypack" })

    var v = await fetch("http://localhost:5260/api", { method: "POST", body: blob, headers: { "Content-Type": "application/x-memorypack" } });



    var buffer = await v.arrayBuffer();

    var foo = Foo.deserialize(buffer);

    var a = Tako.Huga;


    var map = new Map<number, string>();
    map.set(100, "foo");
}

export const enum Tako {
    Hoe = 10,
    Huga = 20,
    Yo = 40
}


export class Foo {
    age: number | null
    name: string | null
    guid: (string)
    seq: number[] | null
    // foo: (string | null)[] | null
    hoge: Tako

    public constructor() {
        this.age = 0;
        this.name = null;
        this.guid = "";
        this.seq = null;
        this.hoge = 0;
    }

    static serialize(value: Foo | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: Foo | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(4);
        writer.writeNullableInt32(value.age);
        writer.writeString(value.name);
        writer.writeGuid(value.guid);
        writer.writeArray(value.seq, (writer, x) => writer.writeInt32(x));
    }

    static deserialize(buffer: ArrayBuffer): Foo | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): Foo | null {
        const [ok, memberCount] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        // TODO: how handle memberCount?

        var value = new Foo();
        value.age = reader.readNullableInt32();
        value.name = reader.readString();
        value.guid = reader.readGuid();
        value.seq = reader.readArray(reader => reader.readInt32());

        return value;
    }
}

export class Nano {
}



type MemoryPackable = Foo | Nano

export class MemoryPackSerializer {
    static Serialize(value: MemoryPackable | null): Uint8Array {
        var writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: MemoryPackable | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
        }
        else if (value instanceof Foo) { // TODO: instanceof......
            Foo.serializeCore(writer, value);
        }
    }
}


export class FooBarBaz {
    myPropertyArray: number[] | null
    myPropertyArray2: (number[] | null)[] | null
    myProperty4: number
    dictman: Map<number, (number | null)[] | null> | null
    setMan: Set<number> | null

    public constructor() {
        this.myPropertyArray = null;
        this.myPropertyArray2 = null;
        this.myProperty4 = 0;
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

        writer.writeObjectHeader(5);
        writer.writeArray(value.myPropertyArray, (writer, x) => writer.writeInt32(x));
        writer.writeArray(value.myPropertyArray2, (writer, x) => writer.writeArray(x, (writer, x) => writer.writeInt32(x)));
        writer.writeInt32(value.myProperty4);
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
        value.myPropertyArray = reader.readArray(reader => reader.readInt32());
        value.myPropertyArray2 = reader.readArray(reader => reader.readArray(reader => reader.readInt32()));
        value.myProperty4 = reader.readInt32();
        value.dictman = reader.readMap(reader => reader.readInt32(), reader => reader.readArray(reader => reader.readNullableInt32()));
        value.setMan = reader.readSet(reader => reader.readInt32());
        return value;

    }
}
