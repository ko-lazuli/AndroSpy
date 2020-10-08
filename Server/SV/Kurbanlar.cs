using System.Net.Sockets;
namespace Server
{
    public class Kurbanlar
    {
        public Socket soket;
        public string id;
        public Kurbanlar(Socket s, string ident)
        {
            soket = s;
            id = ident;
        }
    }
}
