namespace sodoffmmo.Data;
internal static class DataEncoder {
    internal static byte[] EncodeNull() => new byte[] { 0 };

    internal static byte[] EncodeByte(byte value) {
        NetworkData data = new();
        data.WriteValue((byte)NetworkDataType.Byte);
        data.WriteValue(value);
        return data.Data;
    }

    internal static byte[] EncodeBool(bool value) {
        NetworkData data = new();
        data.WriteValue((byte)NetworkDataType.Bool);
        data.WriteValue(value);
        return data.Data;
    }

    internal static byte[] EncodeShort(short value) {
        NetworkData data = new();
        data.WriteValue((byte)NetworkDataType.Short);
        data.WriteValue(value);
        return data.Data;
    }

    internal static byte[] EncodeInt(int value) {
        NetworkData data = new();
        data.WriteValue((byte)NetworkDataType.Int);
        data.WriteValue(value);
        return data.Data;
    }

    internal static byte[] EncodeLong(long value) {
        NetworkData data = new();
        data.WriteValue((byte)NetworkDataType.Long);
        data.WriteValue(value);
        return data.Data;
    }

    internal static byte[] EncodeFloat(float value) {
        NetworkData data = new();
        data.WriteValue((byte)NetworkDataType.Float);
        data.WriteValue(value);
        return data.Data;
    }

    internal static byte[] EncodeDouble(double value) {
        NetworkData data = new();
        data.WriteValue((byte)NetworkDataType.Double);
        data.WriteValue(value);
        return data.Data;
    }

    internal static byte[] EncodeString(string value) {
        NetworkData data = new();
        data.WriteValue((byte)NetworkDataType.String);
        data.WriteValue(value);
        return data.Data;
    }

    internal static byte[] EncodeBoolArray(bool[] value) {
        NetworkData data = new();
        data.WriteValue((byte)NetworkDataType.BoolArray);
        data.WriteValue(Convert.ToInt16(value.Length));
        for (int i = 0; i < value.Length; i++)
            data.WriteValue(value[i]);
        return data.Data;
    }

    internal static byte[] EncodeByteArray(byte[] value) {
        NetworkData data = new();
        data.WriteValue((byte)NetworkDataType.ByteArray);
        data.WriteValue(Convert.ToInt16(value.Length));
        for (int i = 0; i < value.Length; i++)
            data.WriteValue(value[i]);
        return data.Data;
    }

    internal static byte[] EncodeShortArray(short[] value) {
        NetworkData data = new();
        data.WriteValue((byte)NetworkDataType.ShortArray);
        data.WriteValue(Convert.ToInt16(value.Length));
        for (int i = 0; i < value.Length; i++)
            data.WriteValue(value[i]);
        return data.Data;
    }

    internal static byte[] EncodeIntArray(int[] value) {
        NetworkData data = new();
        data.WriteValue((byte)NetworkDataType.IntArray);
        data.WriteValue(Convert.ToInt16(value.Length));
        for (int i = 0; i < value.Length; i++)
            data.WriteValue(value[i]);
        return data.Data;
    }

    internal static byte[] EncodeLongArray(long[] value) {
        NetworkData data = new();
        data.WriteValue((byte)NetworkDataType.LongArray);
        data.WriteValue(Convert.ToInt16(value.Length));
        for (int i = 0; i < value.Length; i++)
            data.WriteValue(value[i]);
        return data.Data;
    }

    internal static byte[] EncodeFloatArray(float[] value) {
        NetworkData data = new();
        data.WriteValue((byte)NetworkDataType.FloatArray);
        data.WriteValue(Convert.ToInt16(value.Length));
        for (int i = 0; i < value.Length; i++)
            data.WriteValue(value[i]);
        return data.Data;
    }

    internal static byte[] EncodeDoubleArray(double[] value) {
        NetworkData data = new();
        data.WriteValue((byte)NetworkDataType.DoubleArray);
        data.WriteValue(Convert.ToInt16(value.Length));
        for (int i = 0; i < value.Length; i++)
            data.WriteValue(value[i]);
        return data.Data;
    }

    internal static byte[] EncodeStringArray(string[] value) {
        NetworkData data = new();
        data.WriteValue((byte)NetworkDataType.StringArray);
        data.WriteValue(Convert.ToInt16(value.Length));
        for (int i = 0; i < value.Length; i++)
            data.WriteValue(value[i]);
        return data.Data;
    }
}
