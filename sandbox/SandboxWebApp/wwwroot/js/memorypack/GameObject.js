import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";
import { Vector3 } from "./Vector3.js";
import { BoundingBox } from "./BoundingBox.js";
import { ColorTag } from "./ColorTag.js";
export class GameObject {
    name;
    position;
    bounds;
    tag;
    constructor() {
        this.name = null;
        this.position = new Vector3();
        this.bounds = new BoundingBox();
        this.tag = new ColorTag();
    }
    static serialize(value) {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }
    static serializeCore(writer, value) {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }
        writer.writeObjectHeader(4);
        writer.writeString(value.name);
        Vector3.serializeCore(writer, value.position);
        BoundingBox.serializeCore(writer, value.bounds);
        ColorTag.serializeCore(writer, value.tag);
    }
    static serializeArray(value) {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }
    static serializeArrayCore(writer, value) {
        writer.writeArray(value, (writer, x) => GameObject.serializeCore(writer, x));
    }
    static deserialize(buffer) {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }
    static deserializeCore(reader) {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }
        const value = new GameObject();
        if (count == 4) {
            value.name = reader.readString();
            value.position = Vector3.deserializeCore(reader);
            value.bounds = BoundingBox.deserializeCore(reader);
            value.tag = ColorTag.deserializeCore(reader);
        }
        else if (count > 4) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0)
                return value;
            value.name = reader.readString();
            if (count == 1)
                return value;
            value.position = Vector3.deserializeCore(reader);
            if (count == 2)
                return value;
            value.bounds = BoundingBox.deserializeCore(reader);
            if (count == 3)
                return value;
            value.tag = ColorTag.deserializeCore(reader);
            if (count == 4)
                return value;
        }
        return value;
    }
    static deserializeArray(buffer) {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }
    static deserializeArrayCore(reader) {
        return reader.readArray(reader => GameObject.deserializeCore(reader));
    }
}
//# sourceMappingURL=GameObject.js.map