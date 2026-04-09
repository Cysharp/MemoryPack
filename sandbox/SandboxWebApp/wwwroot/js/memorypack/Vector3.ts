import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";

export class Vector3 {
    x: number;
    y: number;
    z: number;

    constructor() {
        this.x = 0;
        this.y = 0;
        this.z = 0;

    }

    static serialize(value: Vector3): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: Vector3): void {
        writer.writeFloat32(value.x);
        writer.writeFloat32(value.y);
        writer.writeFloat32(value.z);

    }

    static serializeArray(value: Vector3[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: Vector3[] | null): void {
        writer.writeArray(value, (writer, x) => Vector3.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): Vector3 {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): Vector3 {
        const value = new Vector3();
        value.x = reader.readFloat32();
        value.y = reader.readFloat32();
        value.z = reader.readFloat32();

        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): Vector3[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): Vector3[] | null {
        return reader.readArray(reader => Vector3.deserializeCore(reader));
    }
}
