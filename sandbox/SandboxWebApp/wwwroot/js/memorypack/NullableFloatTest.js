import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";
export class NullableFloatTest {
    nullableFloat;
    nullableDouble;
    constructor() {
        this.nullableFloat = null;
        this.nullableDouble = null;
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
        writer.writeNullableFloat32(value.nullableFloat);
        writer.writeNullableFloat64(value.nullableDouble);
    }
    static serializeArray(value) {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }
    static serializeArrayCore(writer, value) {
        writer.writeArray(value, (writer, x) => NullableFloatTest.serializeCore(writer, x));
    }
    static deserialize(buffer) {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }
    static deserializeCore(reader) {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }
        const value = new NullableFloatTest();
        if (count == 2) {
            value.nullableFloat = reader.readNullableFloat32();
            value.nullableDouble = reader.readNullableFloat64();
        }
        else if (count > 2) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0)
                return value;
            value.nullableFloat = reader.readNullableFloat32();
            if (count == 1)
                return value;
            value.nullableDouble = reader.readNullableFloat64();
            if (count == 2)
                return value;
        }
        return value;
    }
    static deserializeArray(buffer) {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }
    static deserializeArrayCore(reader) {
        return reader.readArray(reader => NullableFloatTest.deserializeCore(reader));
    }
}
//# sourceMappingURL=NullableFloatTest.js.map