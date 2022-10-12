import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";
import { SampleUnion1 } from "./SampleUnion1.js";
import { SampleUnion2 } from "./SampleUnion2.js";
export class IMogeUnion {
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
        else if (value instanceof SampleUnion1) {
            writer.writeUnionHeader(0);
            SampleUnion1.serializeCore(writer, value);
            return;
        }
        else if (value instanceof SampleUnion2) {
            writer.writeUnionHeader(1);
            SampleUnion2.serializeCore(writer, value);
            return;
        }
        else {
            throw new Error("Concrete type is not in MemoryPackUnion");
        }
    }
    static deserialize(buffer) {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }
    static deserializeCore(reader) {
        const [ok, tag] = reader.tryReadUnionHeader();
        if (!ok) {
            return null;
        }
        switch (tag) {
            case 0:
                return SampleUnion1.deserializeCore(reader);
            case 1:
                return SampleUnion2.deserializeCore(reader);
            default:
                throw new Error("Tag is not found in this MemoryPackUnion");
        }
    }
}
//# sourceMappingURL=IMogeUnion.js.map