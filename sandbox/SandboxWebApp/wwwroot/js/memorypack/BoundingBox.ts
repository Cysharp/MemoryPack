import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";
import { Vector3 } from "./Vector3.js";

export class BoundingBox {
    min: Vector3;
    max: Vector3;

    constructor() {
        this.min = new Vector3();
        this.max = new Vector3();

    }

    static serialize(value: BoundingBox): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: BoundingBox): void {
        Vector3.serializeCore(writer, value.min);
        Vector3.serializeCore(writer, value.max);

    }

    static serializeArray(value: BoundingBox[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: BoundingBox[] | null): void {
        writer.writeArray(value, (writer, x) => BoundingBox.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): BoundingBox {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): BoundingBox {
        const value = new BoundingBox();
        value.min = Vector3.deserializeCore(reader);
        value.max = Vector3.deserializeCore(reader);

        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): BoundingBox[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): BoundingBox[] | null {
        return reader.readArray(reader => BoundingBox.deserializeCore(reader));
    }
}
