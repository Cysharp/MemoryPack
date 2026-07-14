import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";
export class Vector3 {
    x;
    y;
    z;
    constructor() {
        this.x = 0;
        this.y = 0;
        this.z = 0;
    }
    static serialize(value) {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }
    static serializeCore(writer, value) {
        writer.writeFloat32(value.x);
        writer.writeFloat32(value.y);
        writer.writeFloat32(value.z);
    }
    static serializeArray(value) {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }
    static serializeArrayCore(writer, value) {
        writer.writeArray(value, (writer, x) => Vector3.serializeCore(writer, x));
    }
    static deserialize(buffer) {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }
    static deserializeCore(reader) {
        const value = new Vector3();
        value.x = reader.readFloat32();
        value.y = reader.readFloat32();
        value.z = reader.readFloat32();
        return value;
    }
    static deserializeArray(buffer) {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }
    static deserializeArrayCore(reader) {
        return reader.readArray(reader => Vector3.deserializeCore(reader));
    }
}
//# sourceMappingURL=Vector3.js.map