
async function hoge() {

    var f = new Foo();

    var bin = Foo.serialize(f);

    var blob = new Blob([bin.buffer], { type: "application/x-memorypack" })

    var v = await fetch("http://hogehoge/", { body: blob });

    var buffer = await v.arrayBuffer();

    var foo = Foo.deserialize(buffer);


}


class Foo {
    age: number
    name: string

    static serialize(value: Foo): Uint8Array {
        let writer = MemoryPackWriter.getInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: Foo): void {
        writer.writeObjectHeader(2);
        writer.writeInt32(value.age);
        writer.writeString(value.name);
    }

    static deserialize(buffer: ArrayBuffer): Foo {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): Foo {
        var memberCount = reader.readObjectHeader();

        var value = new Foo();
        value.age = reader.readInt32();

        return value;
    }
}



export class MemoryPackWriter {
    // pooled writer
    static singletonWriter: MemoryPackWriter;

    public static getInstance(): MemoryPackWriter {
        this.singletonWriter.clear();
        return this.singletonWriter;
    }

    private buffer: Uint8Array
    private dataView: DataView;
    private utf8Encoder: TextEncoder;
    private length: number

    public constructor(initialCapacity: number = 256) {
        this.buffer = new Uint8Array(initialCapacity);
        this.dataView = new DataView(this.buffer);
        this.utf8Encoder = new TextEncoder();
        this.length = 0;
    }

    private ensureCapacity(count: number) {
        if (this.buffer.length - this.length < count) {
            var nextCapacity = this.buffer.length;
            var to = this.length + count;

            while (nextCapacity < to) {
                nextCapacity = nextCapacity * 2;
            }

            var nextBuffer = new Uint8Array(nextCapacity);
            nextBuffer.set(this.buffer);

            // TODO: set to cached buffer?
            this.buffer = nextBuffer;
            this.dataView = new DataView(this.buffer.buffer);
        }
    }

    public writeObjectHeader(memberCount: number) {
        this.writeInt32(memberCount);
    }

    public writeInt32(value: number): void {
        this.ensureCapacity(4);
        this.dataView.setInt32(this.length, value, true);
        this.length += 4;
    }

    public writeString(value: string): void {
        // TODO: write string header

        this.ensureCapacity((value.length + 1) * 3);
        var result = this.utf8Encoder.encodeInto(value, this.buffer.subarray(length));
        this.length += result.written;
    }

    public clear(): void {
        this.length = 0;
    }

    public getSpan(): Uint8Array {
        return this.buffer.subarray(0, length);
    }

    public toArray(): Uint8Array {
        return this.buffer.slice(0, length);
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
