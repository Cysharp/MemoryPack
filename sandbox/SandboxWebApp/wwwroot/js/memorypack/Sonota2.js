import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";
export class Sonota2 {
    myProperty2;
    myProperty1;
    constructor() {
        this.myProperty2 = 0;
        this.myProperty1 = 0;
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
        writer.writeObjectHeader(2);
        writer.writeInt32(value.myProperty2);
        writer.writeInt32(value.myProperty1);
    }
    static deserialize(buffer) {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }
    static deserializeCore(reader) {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }
        var value = new Sonota2();
        if (count == 2) {
            value.myProperty2 = reader.readInt32();
            value.myProperty1 = reader.readInt32();
        }
        else if (count > 2) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0)
                return value;
            value.myProperty2 = reader.readInt32();
            if (count == 1)
                return value;
            value.myProperty1 = reader.readInt32();
            if (count == 2)
                return value;
        }
        return value;
    }
}
//# sourceMappingURL=Sonota2.js.map