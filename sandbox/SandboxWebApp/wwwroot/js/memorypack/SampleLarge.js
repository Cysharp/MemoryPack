import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";
export class SampleLarge {
    _id;
    author;
    created_at;
    description;
    image;
    keywords;
    language;
    permalink;
    published;
    title;
    updated_at;
    url;
    constructor() {
        this._id = null;
        this.author = null;
        this.created_at = null;
        this.description = null;
        this.image = null;
        this.keywords = null;
        this.language = null;
        this.permalink = null;
        this.published = false;
        this.title = null;
        this.updated_at = null;
        this.url = null;
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
        writer.writeObjectHeader(12);
        writer.writeString(value._id);
        writer.writeString(value.author);
        writer.writeString(value.created_at);
        writer.writeString(value.description);
        writer.writeString(value.image);
        writer.writeArray(value.keywords, (writer, x) => writer.writeString(x));
        writer.writeString(value.language);
        writer.writeString(value.permalink);
        writer.writeBoolean(value.published);
        writer.writeString(value.title);
        writer.writeString(value.updated_at);
        writer.writeString(value.url);
    }
    static deserialize(buffer) {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }
    static deserializeCore(reader) {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }
        const value = new SampleLarge();
        if (count == 12) {
            value._id = reader.readString();
            value.author = reader.readString();
            value.created_at = reader.readString();
            value.description = reader.readString();
            value.image = reader.readString();
            value.keywords = reader.readArray(reader => reader.readString());
            value.language = reader.readString();
            value.permalink = reader.readString();
            value.published = reader.readBoolean();
            value.title = reader.readString();
            value.updated_at = reader.readString();
            value.url = reader.readString();
        }
        else if (count > 12) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0)
                return value;
            value._id = reader.readString();
            if (count == 1)
                return value;
            value.author = reader.readString();
            if (count == 2)
                return value;
            value.created_at = reader.readString();
            if (count == 3)
                return value;
            value.description = reader.readString();
            if (count == 4)
                return value;
            value.image = reader.readString();
            if (count == 5)
                return value;
            value.keywords = reader.readArray(reader => reader.readString());
            if (count == 6)
                return value;
            value.language = reader.readString();
            if (count == 7)
                return value;
            value.permalink = reader.readString();
            if (count == 8)
                return value;
            value.published = reader.readBoolean();
            if (count == 9)
                return value;
            value.title = reader.readString();
            if (count == 10)
                return value;
            value.updated_at = reader.readString();
            if (count == 11)
                return value;
            value.url = reader.readString();
            if (count == 12)
                return value;
        }
        return value;
    }
}
//# sourceMappingURL=SampleLarge.js.map