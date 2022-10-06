
export function bar() {
    alert("bar!");
}

export async function hoge() {
    var f = new Foo();
    f.age = 32;
    f.name = "hogemoge";
    f.seq = [1, 10, 200, 300];

    var bin = Foo.serialize(f);

    var blob = new Blob([bin.buffer], { type: "application/x-memorypack" })

    var v = await fetch("http://localhost:5260/api", { method: "POST", body: blob, headers: { "Content-Type": "application/x-memorypack" } });



    var buffer = await v.arrayBuffer();

    var foo = Foo.deserialize(buffer);

    var map = new Map<number, string>();
    map.set(100, "foo");
}


export class Foo {
    age: number | null
    name: string | null
    seq: number[] | null

    public constructor() {
        this.age = 0;
        this.name = null;
        this.seq = null;
    }

    static serialize(value: Foo | null): Uint8Array {
        let writer = MemoryPackWriter.getInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: Foo | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(3);
        writer.writeNullableInt32(value.age);
        writer.writeString(value.name);
        writer.writeArray(value.seq, (writer, x) => writer.writeInt32(x));
    }

    static deserialize(buffer: ArrayBuffer): Foo {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): Foo {
        let memberCount = reader.readObjectHeader();

        var value = new Foo();
        value.age = reader.readInt32();

        return value;
    }
}



export class MemoryPackWriter {
    static singletonWriter: MemoryPackWriter;

    public static getInstance(): MemoryPackWriter {
        if (this.singletonWriter == null) {
            this.singletonWriter = new MemoryPackWriter();
        }
        this.singletonWriter.clear();
        return this.singletonWriter;
    }

    private buffer: Uint8Array
    private dataView: DataView;
    private utf8Encoder: TextEncoder;
    private offset: number

    public constructor(initialCapacity: number = 256) {
        this.buffer = new Uint8Array(initialCapacity);
        this.dataView = new DataView(this.buffer.buffer);
        this.utf8Encoder = new TextEncoder();
        this.offset = 0;
    }

    private ensureCapacity(count: number) {
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

    private clearBuffer(count: number): void {
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

    public writeNullObjectHeader(): void {
        this.writeUint8(255);
    }

    public writeObjectHeader(memberCount: number) {
        this.writeUint8(memberCount);
    }

    public writeCollectionHeader(length: number) {
        this.writeInt32(length);
    }

    public writeNullCollectionHeader() {
        this.writeInt32(-1);
    }

    public writeUint8(value: number): void {
        this.ensureCapacity(1);
        this.dataView.setUint8(this.offset, value);
        this.offset += 1;
    }

    public writeNullableUint8(value: number | null): void {
        if (value == null) {
            this.clearBuffer(2);
            return;
        }

        this.writeUint8(1);
        this.writeUint8(value);
    }

    public writeUint16(value: number): void {
        this.ensureCapacity(2);
        this.dataView.setUint16(this.offset, value);
        this.offset += 2;
    }

    public writeNullableUint16(value: number | null): void {
        if (value == null) {
            this.clearBuffer(4);
            return;
        }

        this.writeUint16(1);
        this.writeUint16(value);
    }

    public writeUint32(value: number): void {
        this.ensureCapacity(4);
        this.dataView.setUint32(this.offset, value);
        this.offset += 4;
    }

    public writeNullableUint32(value: number | null): void {
        if (value == null) {
            this.clearBuffer(8);
            return;
        }

        this.writeUint32(1);
        this.writeUint32(value);
    }

    public writeInt8(value: number): void {
        this.ensureCapacity(1);
        this.dataView.setInt8(this.offset, value);
        this.offset += 1;
    }

    public writeNullableInt8(value: number | null): void {
        if (value == null) {
            this.clearBuffer(2);
            return;
        }

        this.writeInt8(1);
        this.writeInt8(value);
    }

    public writeInt16(value: number): void {
        this.ensureCapacity(2);
        this.dataView.setInt16(this.offset, value);
        this.offset += 2;
    }

    public writeNullableInt16(value: number | null): void {
        if (value == null) {
            this.clearBuffer(4);
            return;
        }

        this.writeInt16(1);
        this.writeInt16(value);
    }

    public writeInt32(value: number): void {
        this.ensureCapacity(4);
        this.dataView.setInt32(this.offset, value, true);
        this.offset += 4;
    }

    public writeNullableInt32(value: number | null): void {
        if (value == null) {
            this.clearBuffer(8);
            return;
        }
        this.writeInt32(1);
        this.writeInt32(value);
    }

    public writeFloat32(value: number): void {
        this.ensureCapacity(4);
        this.dataView.setFloat32(this.offset, value, true);
        this.offset += 4;
    }

    public writeNullableFloat32(value: number | null): void {
        if (value == null) {
            this.clearBuffer(8);
            return;
        }

        this.writeFloat32(1);
        this.writeFloat32(value);
    }

    public writeFloat64(value: number): void {
        this.ensureCapacity(8);
        this.dataView.setFloat64(this.offset, value, true);
        this.offset += 8;
    }

    public writeNullableFloat64(value: number | null): void {
        if (value == null) {
            this.clearBuffer(16);
            return;
        }

        this.writeFloat64(1);
        this.writeFloat64(value);
    }

    public writeString(value: string | null): void {
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

    public writeArray<T>(value: ArrayLike<T> | null, elementWriter: (writer: MemoryPackWriter, element: T) => void): void {
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

    public writeMap<K, V>(value: Map<K, V> | null, keyWriter: (writer: MemoryPackWriter, key: K) => void, valueWriter: (writer: MemoryPackWriter, value: V) => void): void {
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

    public writeSet<T>(value: Set<T> | null, elementWriter: (writer: MemoryPackWriter, element: T) => void): void {
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

    public clear(): void {
        this.offset = 0;
    }

    public getSpan(): Uint8Array {
        return this.buffer.subarray(0, this.offset);
    }

    public toArray(): Uint8Array {
        return this.buffer.slice(0, this.offset);
    }
}

export class MemoryPackReader {
    private buffer: ArrayBuffer
    private dataView: DataView
    private offset: number

    public constructor(buffer: ArrayBuffer) {
        this.buffer = buffer;
        this.dataView = new DataView(this.buffer);
        this.offset = 0;
    }

    public readObjectHeader(): number {
        return this.readInt32();
    }

    public readInt32(): number {
        var v = this.dataView.getInt32(this.offset, true);
        this.offset += 4;
        return v;
    }
}
