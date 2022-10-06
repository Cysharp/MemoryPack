export function bar() {
    alert("bar!");
}
export async function hoge() {
    var f = new Foo();
    f.age = null;
    f.name = "hogemoge";
    // var v = await fetch("http://localhost:5260/api");
    var bin = Foo.serialize(f);
    var blob = new Blob([bin.buffer], { type: "application/x-memorypack" });
    var v = await fetch("http://localhost:5260/api", { method: "POST", body: blob, headers: { "Content-Type": "application/x-memorypack" } });
    var buffer = await v.arrayBuffer();
    var foo = Foo.deserialize(buffer);
}
export class Foo {
    age;
    name;
    constructor() {
    }
    static serialize(value) {
        let writer = MemoryPackWriter.getInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }
    static serializeCore(writer, value) {
        writer.writeObjectHeader(2);
        writer.writeInt32(value.age);
        writer.writeString(value.name);
        writer.writeString(null);
    }
    static deserialize(buffer) {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }
    static deserializeCore(reader) {
        let memberCount = reader.readObjectHeader();
        var value = new Foo();
        value.age = reader.readInt32();
        return value;
    }
}
export class MemoryPackWriter {
    // pooled writer
    static singletonWriter;
    static getInstance() {
        if (this.singletonWriter == null) {
            this.singletonWriter = new MemoryPackWriter();
        }
        this.singletonWriter.clear();
        return this.singletonWriter;
    }
    buffer;
    dataView;
    utf8Encoder;
    offset;
    // TODO: depth
    constructor(initialCapacity = 256) {
        this.buffer = new Uint8Array(initialCapacity);
        this.dataView = new DataView(this.buffer.buffer);
        this.utf8Encoder = new TextEncoder();
        this.offset = 0;
    }
    ensureCapacity(count) {
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
    writeNullObjectHeader() {
        this.writeUint8(255);
    }
    writeObjectHeader(memberCount) {
        this.writeUint8(memberCount);
    }
    writeUint8(value) {
        this.ensureCapacity(1);
        this.dataView.setUint8(this.offset, value);
        this.offset += 1;
    }
    writeInt32(value) {
        this.ensureCapacity(4);
        this.dataView.setInt32(this.offset, value, true);
        this.offset += 4;
    }
    writeString(value) {
        if (value == null) {
            this.writeNullObjectHeader();
            return;
        }
        // [utf8-length, utf16-length, utf8-value]
        this.ensureCapacity(8 + ((value.length + 1) * 3));
        var encodeResult = this.utf8Encoder.encodeInto(value, this.buffer.subarray(this.offset + 8));
        this.dataView.setInt32(this.offset, ~encodeResult.written, true);
        this.dataView.setInt32(this.offset + 4, encodeResult.read, true);
        this.offset += (encodeResult.written + 8);
    }
    // TODO: ARRAY
    // TODO: GUID
    // TODO: Date
    clear() {
        this.offset = 0;
    }
    getSpan() {
        return this.buffer.subarray(0, this.offset);
    }
    toArray() {
        return this.buffer.slice(0, this.offset);
    }
}
export class MemoryPackReader {
    buffer;
    dataView;
    offset;
    constructor(buffer) {
        this.buffer = buffer;
        this.dataView = new DataView(this.buffer);
        this.offset = 0;
    }
    readObjectHeader() {
        return this.readInt32();
    }
    readInt32() {
        var v = this.dataView.getInt32(this.offset, true);
        this.offset += 4;
        return v;
    }
}
//# sourceMappingURL=file.js.map