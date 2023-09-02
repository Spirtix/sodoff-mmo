using System.Text;

namespace sodoffmmo.Data;
public class NetworkData {
    byte[] data;
    int offset = 0;

    public byte[] Data {
        get { return data; }
    }

    public NetworkData() {
        data = new byte[0];
    } 

    public NetworkData(byte[] data) {
        this.data = data;
    }

    public void Seek(int offset) {
        int newOffset = this.offset + offset;
        if (newOffset >= 0 && newOffset < data.Length)
            this.offset = newOffset;
    }

    public byte[] ReverseOrder(byte[] data) {
        if (!BitConverter.IsLittleEndian) return data;
        Array.Reverse(data);
        return data;
    }

    public byte ReadByte() {
        return data[offset++];
    }
    public byte[] ReadChunk(int count) {
        byte[] chunk = new byte[count];
        Buffer.BlockCopy(data, offset, chunk, 0, count);
        offset += count;
        return chunk;
    }

    public short ReadShort() {
        byte[] arr = ReverseOrder(ReadChunk(2));
        return BitConverter.ToInt16(arr);
    }

    public ushort ReadUShort() {
        byte[] arr = ReverseOrder(ReadChunk(2));
        return BitConverter.ToUInt16(arr);
    }

    public bool ReadBool() => data[offset++] == 1;

    public int ReadInt() {
        byte[] arr = ReverseOrder(ReadChunk(4));
        return BitConverter.ToInt32(arr);
    }

    public long ReadLong() {
        byte[] arr = ReverseOrder(ReadChunk(8));
        return BitConverter.ToInt64(arr);
    }

    public float ReadFloat() {
        byte[] arr = ReverseOrder(ReadChunk(4));
        return BitConverter.ToSingle(arr);
    }

    public double ReadDouble() {
        byte[] arr = ReverseOrder(ReadChunk(8));
        return BitConverter.ToDouble(arr);
    }

    public string ReadString() {
        ushort count = ReadUShort();
        string str = Encoding.UTF8.GetString(data, offset, count);
        offset += count;
        return str;
    }

    public void WriteChunk(byte[] chunk, int offset, int count) {
        byte[] newData = new byte[data.Length + count];
        Buffer.BlockCopy(data, 0, newData, 0, data.Length);
        Buffer.BlockCopy(chunk, offset, newData, data.Length, count);
        data = newData;
    }

    public void WriteChunk(byte[] chunk) => WriteChunk(chunk, 0, chunk.Length);

    public void WriteValue(byte b) => WriteChunk(new byte[] { b });

    public void WriteValue(bool b) => WriteChunk(new byte[] { (byte)((!b) ? 0 : 1) });

    public void WriteValue(int i) => WriteChunk(ReverseOrder(BitConverter.GetBytes(i)));

    public void WriteValue(short s) => WriteChunk(ReverseOrder(BitConverter.GetBytes(s)));

    public void WriteValue(ushort us) => WriteChunk(ReverseOrder(BitConverter.GetBytes(us)));

    public void WriteValue(long l) => WriteChunk(ReverseOrder(BitConverter.GetBytes(l)));

    public void WriteValue(float f) => WriteChunk(ReverseOrder(BitConverter.GetBytes(f)));

    public void WriteValue(double d) => WriteChunk(ReverseOrder(BitConverter.GetBytes(d)));

    public void WriteValue(string str) {
        WriteValue(GetUTFStringLength(str));
        WriteChunk(Encoding.UTF8.GetBytes(str));
    }

    private ushort GetUTFStringLength(string str) {
        ushort length = 0;
        foreach (int c in str) {
            if (c > 0 && c < 128)
                ++length;
            else if (c > 2047)
                length += 3;
            else
                length += 2;
        }
        if (length > 32768)
            throw new Exception("String is too long");
        return length;
    }
}
