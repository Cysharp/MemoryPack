import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";
import { IMogeUnion } from "./IMogeUnion.js"; 

export class SampleUnion1 implements IMogeUnion {
    myProperty: number | null;

    constructor() {
        this.myProperty = null;

    }

    static serialize(value: SampleUnion1 | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: SampleUnion1 | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(1);
        writer.writeNullableInt32(value.myProperty);

    }

    static serializeArray(value: (SampleUnion1 | null)[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: (SampleUnion1 | null)[] | null): void {
        writer.writeArray(value, (writer, x) => SampleUnion1.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): SampleUnion1 | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): SampleUnion1 | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        const value = new SampleUnion1();
        if (count == 1) {
            value.myProperty = reader.readNullableInt32();

        }
        else if (count > 1) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.myProperty = reader.readNullableInt32(); if (count == 1) return value;

        }
        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): (SampleUnion1 | null)[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): (SampleUnion1 | null)[] | null {
        return reader.readArray(reader => SampleUnion1.deserializeCore(reader));
    }
}
