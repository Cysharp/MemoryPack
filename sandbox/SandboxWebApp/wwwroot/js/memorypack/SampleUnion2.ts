import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";
import { IMogeUnion } from "./IMogeUnion.js"; 

export class SampleUnion2 implements IMogeUnion {
    myProperty: string | null;

    constructor() {
        this.myProperty = null;

    }

    static serialize(value: SampleUnion2 | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: SampleUnion2 | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(1);
        writer.writeString(value.myProperty);

    }

    static deserialize(buffer: ArrayBuffer): SampleUnion2 | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): SampleUnion2 | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        const value = new SampleUnion2();
        if (count == 1) {
            value.myProperty = reader.readString();

        }
        else if (count > 1) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.myProperty = reader.readString(); if (count == 1) return value;

        }
        return value;
    }
}
