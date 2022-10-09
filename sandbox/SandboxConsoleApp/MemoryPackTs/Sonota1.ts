import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";

export class Sonota1 {
    hokuHoku: number;

    public constructor() {
        this.hokuHoku = 0;

    }

    static serialize(value: Sonota1 | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: Sonota1 | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(1);
        writer.writeInt32(value.hokuHoku);

    }

    static deserialize(buffer: ArrayBuffer): Sonota1 | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): Sonota1 | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        var value = new Sonota1();
        if (count == 1) {
            value.hokuHoku = reader.readInt32();

        }
        else if (count > 1) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.hokuHoku = reader.readInt32(); if (count == 1) return value;

        }
        return value;
    }
}
