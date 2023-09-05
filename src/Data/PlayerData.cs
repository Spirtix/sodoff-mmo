using sodoffmmo.Core;

namespace sodoffmmo.Data;
public class PlayerData {
    public double R { get; set; }
    public double R1 { get; set; }
    public double R2 { get; set; }
    public double R3 { get; set; }
    public string Fp { get; set; } = "";
    public double Mx { get; set; } = 6;
    public string Udt { get; set; } = "";
    public double P1 { get; set; }
    public double P2 { get; set; }
    public double P3 { get; set; }

    public string Nt { get; set; } = "1.0";
    public int T { get; set; } = 0;
    public string J { get; set; } = "2";
    public int F { get; set; }
    public int Mbf { get; set; }
    public string Bu { get; set; } = "False";
    public string Uid { get; set; } = "";
    public string Pu { get; set; } = "";
    public string A { get; set; } = "";
    public string Ra { get; set; } = "";
    public string Cu { get; set; } = "-1";
    public string M { get; set; } = "False";
    public string L { get; set; } = "";
    public string UNToken { get; set; } = "";

    public NetworkArray GetNetworkData(int clientID) {
        NetworkArray arr = new();
        arr.Add(clientID);
        arr.Add(Uid);
        arr.Add((short)1);
        arr.Add((short)clientID);

        NetworkArray paramArr = new();
        paramArr.Add(NetworkArray.DoubleParam("R1", R1));
        paramArr.Add(NetworkArray.StringParam("FP", Fp));
        paramArr.Add(NetworkArray.DoubleParam("MX", Mx));
        paramArr.Add(NetworkArray.StringParam("UDT", Udt));
        paramArr.Add(NetworkArray.DoubleParam("P2", P2));
        paramArr.Add(NetworkArray.StringParam("NT", Nt));
        paramArr.Add(NetworkArray.IntParam("t", T));
        paramArr.Add(NetworkArray.StringParam("J", J));
        paramArr.Add(NetworkArray.IntParam("F", F));
        paramArr.Add(NetworkArray.IntParam("MBF", Mbf));
        paramArr.Add(NetworkArray.DoubleParam("R2", R2));
        paramArr.Add(NetworkArray.DoubleParam("R", R));
        paramArr.Add(NetworkArray.StringParam("BU", Bu));
        paramArr.Add(NetworkArray.DoubleParam("P1", P1));
        paramArr.Add(NetworkArray.StringParam("UID", Uid));
        paramArr.Add(NetworkArray.DoubleParam("R3", R3));
        paramArr.Add(NetworkArray.StringParam("PU", Pu));
        paramArr.Add(NetworkArray.StringParam("A", A));
        paramArr.Add(NetworkArray.StringParam("RA", Ra));
        paramArr.Add(NetworkArray.DoubleParam("P3", P3));
        paramArr.Add(NetworkArray.StringParam("CU", Cu));
        paramArr.Add(NetworkArray.StringParam("M", M));
        paramArr.Add(NetworkArray.StringParam("L", L));

        arr.Add(paramArr);
        return arr;
    }
}
