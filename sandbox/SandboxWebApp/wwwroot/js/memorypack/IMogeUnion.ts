import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";

import { SampleUnion1 } from "./SampleUnion1.js"; 
import { SampleUnion2 } from "./SampleUnion2.js"; 

export abstract class IMogeUnion {
    static serialize(value: IMogeUnion | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: IMogeUnion | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }
        else if (value instanceof SampleUnion1) {
            writer.writeUnionHeader(0);
            SampleUnion1.serializeCore(writer, value);
            return;
        }
        else if (value instanceof SampleUnion2) {
            writer.writeUnionHeader(1);
            SampleUnion2.serializeCore(writer, value);
            return;
        }

        else {
            throw new Error("Concrete type is not in MemoryPackUnion");
        }
    }

    static serializeArray(value: IMogeUnion[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: IMogeUnion[] | null): void {
        writer.writeArray(value, (writer, x) => IMogeUnion.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): IMogeUnion | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): IMogeUnion | null {
        const [ok, tag] = reader.tryReadUnionHeader();
        if (!ok) {
            return null;
        }

        switch (tag) {
            case 0:
                return SampleUnion1.deserializeCore(reader);
            case 1:
                return SampleUnion2.deserializeCore(reader);

            default:
                throw new Error("Tag is not found in this MemoryPackUnion");
        }
    }

    static deserializeArray(buffer: ArrayBuffer): (IMogeUnion | null)[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): (IMogeUnion | null)[] | null {
        return reader.readArray(reader => IMogeUnion.deserializeCore(reader));
    }
}
