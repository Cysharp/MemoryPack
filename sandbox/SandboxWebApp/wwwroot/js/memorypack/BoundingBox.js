import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";
import { Vector3 } from "./Vector3.js";
export class BoundingBox {
    min;
    max;
    constructor() {
        this.min = new Vector3();
        this.max = new Vector3();
    }
    static serialize(value) {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }
    static serializeCore(writer, value) {
        Vector3.serializeCore(writer, value.min);
        Vector3.serializeCore(writer, value.max);
    }
    static serializeArray(value) {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }
    static serializeArrayCore(writer, value) {
        writer.writeArray(value, (writer, x) => BoundingBox.serializeCore(writer, x));
    }
    static deserialize(buffer) {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }
    static deserializeCore(reader) {
        const value = new BoundingBox();
        value.min = Vector3.deserializeCore(reader);
        value.max = Vector3.deserializeCore(reader);
        return value;
    }
    static deserializeArray(buffer) {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }
    static deserializeArrayCore(reader) {
        return reader.readArray(reader => BoundingBox.deserializeCore(reader));
    }
}
//# sourceMappingURL=BoundingBox.js.map