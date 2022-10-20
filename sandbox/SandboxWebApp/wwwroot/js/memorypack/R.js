import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";
export class R {
    id;
    age;
    firstName;
    lastName;
    dateOfBirth;
    gender;
    emails;
    constructor() {
        this.id = "00000000-0000-0000-0000-000000000000";
        this.age = 0;
        this.firstName = null;
        this.lastName = null;
        this.dateOfBirth = new Date(0);
        this.gender = 0;
        this.emails = null;
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
        writer.writeObjectHeader(7);
        writer.writeGuid(value.id);
        writer.writeInt32(value.age);
        writer.writeString(value.firstName);
        writer.writeString(value.lastName);
        writer.writeDate(value.dateOfBirth);
        writer.writeInt32(value.gender);
        writer.writeArray(value.emails, (writer, x) => writer.writeString(x));
    }
    static serializeArray(value) {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }
    static serializeArrayCore(writer, value) {
        writer.writeArray(value, (writer, x) => R.serializeCore(writer, x));
    }
    static deserialize(buffer) {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }
    static deserializeCore(reader) {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }
        const value = new R();
        if (count == 7) {
            value.id = reader.readGuid();
            value.age = reader.readInt32();
            value.firstName = reader.readString();
            value.lastName = reader.readString();
            value.dateOfBirth = reader.readDate();
            value.gender = reader.readInt32();
            value.emails = reader.readArray(reader => reader.readString());
        }
        else if (count > 7) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0)
                return value;
            value.id = reader.readGuid();
            if (count == 1)
                return value;
            value.age = reader.readInt32();
            if (count == 2)
                return value;
            value.firstName = reader.readString();
            if (count == 3)
                return value;
            value.lastName = reader.readString();
            if (count == 4)
                return value;
            value.dateOfBirth = reader.readDate();
            if (count == 5)
                return value;
            value.gender = reader.readInt32();
            if (count == 6)
                return value;
            value.emails = reader.readArray(reader => reader.readString());
            if (count == 7)
                return value;
        }
        return value;
    }
    static deserializeArray(buffer) {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }
    static deserializeArrayCore(reader) {
        return reader.readArray(reader => R.deserializeCore(reader));
    }
}
//# sourceMappingURL=R.js.map