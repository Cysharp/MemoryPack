import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";
import { NoMarkByteEnum } from "./NoMarkByteEnum.js"; 
import { NestedObject } from "./NestedObject.js"; 
import { IMogeUnion } from "./IMogeUnion.js"; 

export class ArrayGenericsCheck {
    array1: (NestedObject | null)[] | null;
    array2: (IMogeUnion | null)[] | null;
    list1: NoMarkByteEnum[] | null;

    constructor() {
        this.array1 = null;
        this.array2 = null;
        this.list1 = null;

    }

    static serialize(value: ArrayGenericsCheck | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: ArrayGenericsCheck | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(3);
        writer.writeArray(value.array1, (writer, x) => NestedObject.serializeCore(writer, x));
        writer.writeArray(value.array2, (writer, x) => IMogeUnion.serializeCore(writer, x));
        writer.writeArray(value.list1, (writer, x) => writer.writeUint8(x));

    }

    static serializeArray(value: ArrayGenericsCheck[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: ArrayGenericsCheck[] | null): void {
        writer.writeArray(value, (writer, x) => ArrayGenericsCheck.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): ArrayGenericsCheck | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): ArrayGenericsCheck | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        const value = new ArrayGenericsCheck();
        if (count == 3) {
            value.array1 = reader.readArray(reader => NestedObject.deserializeCore(reader));
            value.array2 = reader.readArray(reader => IMogeUnion.deserializeCore(reader));
            value.list1 = reader.readArray(reader => reader.readUint8());

        }
        else if (count > 3) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.array1 = reader.readArray(reader => NestedObject.deserializeCore(reader)); if (count == 1) return value;
            value.array2 = reader.readArray(reader => IMogeUnion.deserializeCore(reader)); if (count == 2) return value;
            value.list1 = reader.readArray(reader => reader.readUint8()); if (count == 3) return value;

        }
        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): (ArrayGenericsCheck | null)[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): (ArrayGenericsCheck | null)[] | null {
        return reader.readArray(reader => ArrayGenericsCheck.deserializeCore(reader));
    }
}
