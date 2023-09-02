using sodoffmmo;
using System.Net;

Server server = new(IPAddress.Any, 9933);
await server.Run();
