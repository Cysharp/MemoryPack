import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";

export class S {
    myBool: boolean;
    myByte: number;
    mySByte: number;
    myShort: number;

    public constructor() {
        this.myBool = false;
        this.myByte = 0;
        this.mySByte = 0;
        this.myShort = 0;

    }

    static serialize(value: S | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: S | null): void {
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

    static deserialize(buffer: ArrayBuffer): S | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): S | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        var value = new S();
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
}
