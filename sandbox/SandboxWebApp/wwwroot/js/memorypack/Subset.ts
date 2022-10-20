import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";

export class Subset {
    myBool: boolean;
    myByte: number;
    mySByte: number;
    myShort: number;

    constructor() {
        this.myBool = false;
        this.myByte = 0;
        this.mySByte = 0;
        this.myShort = 0;

    }

    static serialize(value: Subset | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: Subset | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(4);
        writer.writeBoolean(value.myBool);
        writer.writeUint8(value.myByte);
        writer.writeInt8(value.mySByte);
        writer.writeInt16(value.myShort);

    }

    static serializeArray(value: (Subset | null)[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: (Subset | null)[] | null): void {
        writer.writeArray(value, (writer, x) => Subset.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): Subset | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): Subset | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        const value = new Subset();
        if (count == 4) {
            value.myBool = reader.readBoolean();
            value.myByte = reader.readUint8();
            value.mySByte = reader.readInt8();
            value.myShort = reader.readInt16();

        }
        else if (count > 4) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.myBool = reader.readBoolean(); if (count == 1) return value;
            value.myByte = reader.readUint8(); if (count == 2) return value;
            value.mySByte = reader.readInt8(); if (count == 3) return value;
            value.myShort = reader.readInt16(); if (count == 4) return value;

        }
        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): (Subset | null)[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): (Subset | null)[] | null {
        return reader.readArray(reader => Subset.deserializeCore(reader));
    }
}
