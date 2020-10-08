using System.Net.Sockets;
namespace Server
{
    /// <summary>
    /// Represents all victims in our system
    /// </summary>
    public class Victims
    {
        public Socket socket;
        public string id;
        /// <summary>
        /// Creates a new <see cref="Victims"/> instance
        /// </summary>
        /// <param name="socket">Socket that connects our client to the server</param>
        /// <param name="id">identification of the server</param>
        public Victims(Socket socket, string id)
        {
            this.socket = socket;
            this.id = id;
        }
    }
}
