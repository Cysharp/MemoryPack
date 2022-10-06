export function bar() {
    alert("bar!");
}
export async function hoge() {
    var f = new Foo();
    f.age = 32;
    f.name = "hogemoge";
    f.seq = [1, 10, 200, 300];
    var bin = Foo.serialize(f);
    var blob = new Blob([bin.buffer], { type: "application/x-memorypack" });
    var v = await fetch("http://localhost:5260/api", { method: "POST", body: blob, headers: { "Content-Type": "application/x-memorypack" } });
    var buffer = await v.arrayBuffer();
    var foo = Foo.deserialize(buffer);
    var map = new Map();
    map.set(100, "foo");
}
export class Foo {
    age;
    name;
    seq;
    constructor() {
        this.age = 0;
        this.name = null;
        this.seq = null;
    }
    static serialize(value) {
        let writer = MemoryPackWriter.getInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }
    static serializeCore(writer, value) {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }
        writer.writeObjectHeader(3);
        writer.writeNullableInt32(value.age);
        writer.writeString(value.name);
        writer.writeArray(value.seq, (writer, x) => writer.writeInt32(x));
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
    clearBuffer(count) {
        this.ensureCapacity(count);
        while (count > 4) {
            this.dataView.setUint32(this.offset, 0, true);
            this.offset += 4;
            count -= 4;
        }
        if (count > 2) {
            this.dataView.setUint16(this.offset, 0, true);
            this.offset += 2;
            count -= 2;
        }
        if (count > 1) {
            this.dataView.setUint8(this.offset, 0);
            this.offset += 1;
        }
    }
    writeNullObjectHeader() {
        this.writeUint8(255);
    }
    writeObjectHeader(memberCount) {
        this.writeUint8(memberCount);
    }
    writeCollectionHeader(length) {
        this.writeInt32(length);
    }
    writeNullCollectionHeader() {
        this.writeInt32(-1);
    }
    writeUint8(value) {
        this.ensureCapacity(1);
        this.dataView.setUint8(this.offset, value);
        this.offset += 1;
    }
    writeNullableUint8(value) {
        if (value == null) {
            this.clearBuffer(2);
            return;
        }
        this.writeUint8(1);
        this.writeUint8(value);
    }
    writeUint16(value) {
        this.ensureCapacity(2);
        this.dataView.setUint16(this.offset, value);
        this.offset += 2;
    }
    writeNullableUint16(value) {
        if (value == null) {
            this.clearBuffer(4);
            return;
        }
        this.writeUint16(1);
        this.writeUint16(value);
    }
    writeUint32(value) {
        this.ensureCapacity(4);
        this.dataView.setUint32(this.offset, value);
        this.offset += 4;
    }
    writeNullableUint32(value) {
        if (value == null) {
            this.clearBuffer(8);
            return;
        }
        this.writeUint32(1);
        this.writeUint32(value);
    }
    writeInt8(value) {
        this.ensureCapacity(1);
        this.dataView.setInt8(this.offset, value);
        this.offset += 1;
    }
    writeNullableInt8(value) {
        if (value == null) {
            this.clearBuffer(2);
            return;
        }
        this.writeInt8(1);
        this.writeInt8(value);
    }
    writeInt16(value) {
        this.ensureCapacity(2);
        this.dataView.setInt16(this.offset, value);
        this.offset += 2;
    }
    writeNullableInt16(value) {
        if (value == null) {
            this.clearBuffer(4);
            return;
        }
        this.writeInt16(1);
        this.writeInt16(value);
    }
    writeInt32(value) {
        this.ensureCapacity(4);
        this.dataView.setInt32(this.offset, value, true);
        this.offset += 4;
    }
    writeNullableInt32(value) {
        if (value == null) {
            this.clearBuffer(8);
            return;
        }
        this.writeInt32(1);
        this.writeInt32(value);
    }
    writeFloat32(value) {
        this.ensureCapacity(4);
        this.dataView.setFloat32(this.offset, value, true);
        this.offset += 4;
    }
    writeNullableFloat32(value) {
        if (value == null) {
            this.clearBuffer(8);
            return;
        }
        this.writeFloat32(1);
        this.writeFloat32(value);
    }
    writeFloat64(value) {
        this.ensureCapacity(8);
        this.dataView.setFloat64(this.offset, value, true);
        this.offset += 8;
    }
    writeNullableFloat64(value) {
        if (value == null) {
            this.clearBuffer(16);
            return;
        }
        this.writeFloat64(1);
        this.writeFloat64(value);
    }
    writeString(value) {
        if (value == null) {
            this.writeNullObjectHeader();
            return;
        }
        // [utf8-length, utf16-length, utf8-value]
        this.ensureCapacity(8 + ((value.length + 1) * 3));
        var encodeResult = this.utf8Encoder.encodeInto(value, this.buffer.subarray(this.offset + 8));
        if (encodeResult.written === undefined || encodeResult.read === undefined) {
            throw new Error("failed utf8 TextEncoder.encodeInto.");
        }
        this.dataView.setInt32(this.offset, ~encodeResult.written, true);
        this.dataView.setInt32(this.offset + 4, encodeResult.read, true);
        this.offset += (encodeResult.written + 8);
    }
    writeArray(value, elementWriter) {
        if (value == null) {
            this.writeNullCollectionHeader();
            return;
        }
        this.writeCollectionHeader(value.length);
        var len = value.length;
        for (var i = 0; i < len; i++) {
            elementWriter(this, value[i]);
        }
    }
    writeMap(value, keyWriter, valueWriter) {
        if (value == null) {
            this.writeNullCollectionHeader();
            return;
        }
        this.writeCollectionHeader(value.size);
        value.forEach((v, k) => {
            keyWriter(this, k);
            valueWriter(this, v);
        });
    }
    writeSet(value, elementWriter) {
        if (value == null) {
            this.writeNullCollectionHeader();
            return;
        }
        this.writeCollectionHeader(value.size);
        value.forEach(x => {
            elementWriter(this, x);
        });
    }
    // TODO: UNION
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