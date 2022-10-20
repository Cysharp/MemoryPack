import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";

export class NestedObject {
    myProperty: number;
    myProperty2: string | null;

    constructor() {
        this.myProperty = 0;
        this.myProperty2 = null;

    }

    static serialize(value: NestedObject | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: NestedObject | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(2);
        writer.writeInt32(value.myProperty);
        writer.writeString(value.myProperty2);

    }

    static serializeArray(value: (NestedObject | null)[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: (NestedObject | null)[] | null): void {
        writer.writeArray(value, (writer, x) => NestedObject.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): NestedObject | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): NestedObject | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        const value = new NestedObject();
        if (count == 2) {
            value.myProperty = reader.readInt32();
            value.myProperty2 = reader.readString();

        }
        else if (count > 2) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.myProperty = reader.readInt32(); if (count == 1) return value;
            value.myProperty2 = reader.readString(); if (count == 2) return value;

        }
        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): (NestedObject | null)[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): (NestedObject | null)[] | null {
        return reader.readArray(reader => NestedObject.deserializeCore(reader));
    }
}
