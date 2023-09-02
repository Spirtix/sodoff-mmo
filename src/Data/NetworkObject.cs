namespace sodoffmmo.Data;
public class NetworkObject {
    Dictionary<string, DataWrapper> fields = new();

    public NetworkObject() {}

    public NetworkObject(NetworkData data) {
        Deserialize(data);
    }

    public NetworkObject(byte[] data) : this(new NetworkData(data)) { }

    public void Add(string label, DataWrapper wrapper) => fields[label] = wrapper;

    public void Add(string label, byte value) => fields[label] = new DataWrapper(NetworkDataType.Byte, value);

    public void Add(string label, bool value) => fields[label] = new DataWrapper(NetworkDataType.Bool, value);

    public void Add(string label, short value) => fields[label] = new DataWrapper(NetworkDataType.Short, value);

    public void Add(string label, int value) => fields[label] = new DataWrapper(NetworkDataType.Int, value);

    public void Add(string label, long value) => fields[label] = new DataWrapper(NetworkDataType.Long, value);

    public void Add(string label, float value) => fields[label] = new DataWrapper(NetworkDataType.Float, value);

    public void Add(string label, double value) => fields[label] = new DataWrapper(NetworkDataType.Double, value);

    public void Add(string label, string value) => fields[label] = new DataWrapper(NetworkDataType.String, value);

    public void Add(string label, byte[] value) => fields[label] = new DataWrapper(NetworkDataType.ByteArray, value);

    public void Add(string label, bool[] value) => fields[label] = new DataWrapper(NetworkDataType.BoolArray, value);

    public void Add(string label, short[] value) => fields[label] = new DataWrapper(NetworkDataType.ShortArray, value);

    public void Add(string label, int[] value) => fields[label] = new DataWrapper(NetworkDataType.IntArray, value);

    public void Add(string label, long[] value) => fields[label] = new DataWrapper(NetworkDataType.LongArray, value);

    public void Add(string label, float[] value) => fields[label] = new DataWrapper(NetworkDataType.FloatArray, value);

    public void Add(string label, double[] value) => fields[label] = new DataWrapper(NetworkDataType.DoubleArray, value);

    public void Add(string label, string[] value) => fields[label] = new DataWrapper(NetworkDataType.StringArray, value);

    public void Add(string label, NetworkArray value) => fields[label] = new DataWrapper(NetworkDataType.NetworkArray, value);

    public void Add(string label, NetworkObject value) => fields[label] = new DataWrapper(NetworkDataType.NetworkObject, value);

    public T Get<T>(string key) {
        if (!fields.ContainsKey(key))
            return default;
        return (T)fields[key].Data;
    }


    public NetworkPacket Serialize() => new NetworkPacket(0x80, SerializeObject(this));

    private byte[] SerializeObject(NetworkObject obj) {
        NetworkData data = new();
        data.WriteValue((byte)18);
        data.WriteValue(Convert.ToInt16(obj.fields.Count));
        foreach (string key in obj.fields.Keys) {
            data.WriteValue(key);
            data.WriteChunk(EncodeObject(obj.fields[key]));
        }
        return data.Data;
    }

    private void Deserialize(NetworkData data) {
        if (data.ReadByte() != 0x12)
            throw new Exception("Invalid object type");

        short count = data.ReadShort();
        for (short i = 0; i < count; i++) {
            string label = data.ReadString();
            DataWrapper obj = DecodeObject(data);
            Add(label, obj);
        }
    }

    private byte[] EncodeObject(DataWrapper obj) {
        switch ((NetworkDataType)obj.Type) {
            case NetworkDataType.Null:
                return DataEncoder.EncodeNull();
            case NetworkDataType.Bool:
                return DataEncoder.EncodeBool((bool)obj.Data);
            case NetworkDataType.Byte:
                return DataEncoder.EncodeByte((byte)obj.Data);
            case NetworkDataType.Short:
                return DataEncoder.EncodeShort((short)obj.Data);
            case NetworkDataType.Int:
                return DataEncoder.EncodeInt((int)obj.Data);
            case NetworkDataType.Long:
                return DataEncoder.EncodeLong((long)obj.Data);
            case NetworkDataType.Float:
                return DataEncoder.EncodeFloat((float)obj.Data);
            case NetworkDataType.Double:
                return DataEncoder.EncodeDouble((double)obj.Data);
            case NetworkDataType.String:
                return DataEncoder.EncodeString((string)obj.Data);
            case NetworkDataType.BoolArray:
                return DataEncoder.EncodeBoolArray((bool[])obj.Data);
            case NetworkDataType.ByteArray:
                return DataEncoder.EncodeByteArray((byte[])obj.Data);
            case NetworkDataType.ShortArray:
                return DataEncoder.EncodeShortArray((short[])obj.Data);
            case NetworkDataType.IntArray:
                return DataEncoder.EncodeIntArray((int[])obj.Data);
            case NetworkDataType.LongArray:
                return DataEncoder.EncodeLongArray((long[])obj.Data);
            case NetworkDataType.FloatArray:
                return DataEncoder.EncodeFloatArray((float[])obj.Data);
            case NetworkDataType.DoubleArray:
                return DataEncoder.EncodeDoubleArray((double[])obj.Data);
            case NetworkDataType.StringArray:
                return DataEncoder.EncodeStringArray((string[])obj.Data);
            case NetworkDataType.NetworkArray:
                return EncodeNetworkArray((NetworkArray)obj.Data);
            case NetworkDataType.NetworkObject:
                return SerializeObject((NetworkObject)obj.Data);
            default: throw new Exception("Invalid object");
        }
    }

    private byte[] EncodeNetworkArray(NetworkArray arr) {
        NetworkData data = new();
        data.WriteValue((byte)17);
        data.WriteValue(Convert.ToInt16(arr.Length));
        for (int i = 0; i < arr.Length; i++)
            data.WriteChunk(EncodeObject(arr[i]));
        return data.Data;
    }

    private DataWrapper DecodeObject(NetworkData data) {
        switch ((NetworkDataType)data.ReadByte()) {
            case NetworkDataType.Null:
                return DataDecoder.DecodeNull(data);
            case NetworkDataType.Bool:
                return DataDecoder.DecodeBool(data);
            case NetworkDataType.Byte:
                return DataDecoder.DecodeByte(data);
            case NetworkDataType.Short:
                return DataDecoder.DecodeShort(data);
            case NetworkDataType.Int:
                return DataDecoder.DecodeInt(data);
            case NetworkDataType.Long:
                return DataDecoder.DecodeLong(data);
            case NetworkDataType.Float:
                return DataDecoder.DecodeFloat(data);
            case NetworkDataType.Double:
                return DataDecoder.DecodeDouble(data);
            case NetworkDataType.String:
                return DataDecoder.DecodeString(data);
            case NetworkDataType.BoolArray:
                return DataDecoder.DecodeBoolArray(data);
            case NetworkDataType.ByteArray:
                return DataDecoder.DecodeByteArray(data);
            case NetworkDataType.ShortArray:
                return DataDecoder.DecodeShortArray(data);
            case NetworkDataType.IntArray:
                return DataDecoder.DecodeIntArray(data);
            case NetworkDataType.LongArray:
                return DataDecoder.DecodeLongArray(data);
            case NetworkDataType.FloatArray:
                return DataDecoder.DecodeFloatArray(data);
            case NetworkDataType.DoubleArray:
                return DataDecoder.DecodeDoubleArray(data);
            case NetworkDataType.StringArray:
                return DataDecoder.DecodeStringArray(data);
            case NetworkDataType.NetworkArray:
                return DecodeNetworkArray(data);
            case NetworkDataType.NetworkObject:
                data.Seek(-1);
                return new DataWrapper(NetworkDataType.NetworkObject, new NetworkObject(data));
            default: throw new Exception("Invalid object");
        }
    }

    private DataWrapper DecodeNetworkArray(NetworkData data) {
        NetworkArray array = new NetworkArray();
        short count = data.ReadShort();
        for (short i = 0; i < count; i++) {
            DataWrapper wrapper = DecodeObject(data);
            array.Add(wrapper);
        }
        return new DataWrapper(NetworkDataType.NetworkArray, array);
    }

    public static NetworkObject WrapObject(byte c, short a, NetworkObject obj) {
        NetworkObject wrapper = new();
        wrapper.Add("c", c);
        wrapper.Add("a", a);
        wrapper.Add("p", obj);
        return wrapper;
    }
}
