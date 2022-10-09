import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";
export async function hoge() {
    var f = new Foo();
    f.age = 32;
    f.name = "hogemoge";
    f.guid = "CA761232-ED42-11CE-BACD-00AA0057B223";
    f.seq = [1, 10, 200, 300];
    var bin = Foo.serialize(f);
    var blob = new Blob([bin.buffer], { type: "application/x-memorypack" });
    var v = await fetch("http://localhost:5260/api", { method: "POST", body: blob, headers: { "Content-Type": "application/x-memorypack" } });
    var buffer = await v.arrayBuffer();
    var foo = Foo.deserialize(buffer);
    var a = 20 /* Tako.Huga */;
    var map = new Map();
    map.set(100, "foo");
}
export class Foo {
    age;
    name;
    guid;
    seq;
    // foo: (string | null)[] | null
    hoge;
    constructor() {
        this.age = 0;
        this.name = null;
        this.guid = "";
        this.seq = null;
        this.hoge = 0;
    }
    static serialize(value) {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }
    static serializeCore(writer, value) {
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
    static deserialize(buffer) {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }
    static deserializeCore(reader) {
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
export class MemoryPackSerializer {
    static Serialize(value) {
        var writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }
    static serializeCore(writer, value) {
        if (value == null) {
            writer.writeNullObjectHeader();
        }
        else if (value instanceof Foo) { // TODO: instanceof......
            Foo.serializeCore(writer, value);
        }
    }
}
export class FooBarBaz {
    myPropertyArray;
    myPropertyArray2;
    myProperty4;
    dictman;
    setMan;
    constructor() {
        this.myPropertyArray = null;
        this.myPropertyArray2 = null;
        this.myProperty4 = 0;
        this.dictman = null;
        this.setMan = null;
    }
    static serialize(value) {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }
    static serializeCore(writer, value) {
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
    static deserialize(buffer) {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }
    static deserializeCore(reader) {
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
//# sourceMappingURL=file.js.map