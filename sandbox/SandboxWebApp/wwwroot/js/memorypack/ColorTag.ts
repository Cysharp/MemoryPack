import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";

export class ColorTag {
    name: string | null;
    code: number;

    constructor() {
        this.name = null;
        this.code = 0;

    }

    static serialize(value: ColorTag): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: ColorTag): void {
        writer.writeObjectHeader(2);
        writer.writeString(value.name);
        writer.writeInt32(value.code);

    }

    static serializeArray(value: (ColorTag)[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: (ColorTag)[] | null): void {
        writer.writeArray(value, (writer, x) => ColorTag.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): ColorTag {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): ColorTag {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            throw new Error("Cannot deserialize null into struct ColorTag.");
        }

        const value = new ColorTag();
        if (count == 2) {
            value.name = reader.readString();
            value.code = reader.readInt32();

        }
        else if (count > 2) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.name = reader.readString(); if (count == 1) return value;
            value.code = reader.readInt32(); if (count == 2) return value;

        }
        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): (ColorTag)[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): (ColorTag)[] | null {
        return reader.readArray(reader => ColorTag.deserializeCore(reader));
    }
}
