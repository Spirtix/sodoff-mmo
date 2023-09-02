namespace sodoffmmo.Data;
public class DataWrapper {

    public int Type { get; private set; }
    public object Data { get; private set; }

    public DataWrapper(NetworkDataType type, object data) {
        this.Type = (int)type;
        this.Data = data;
    }
}
