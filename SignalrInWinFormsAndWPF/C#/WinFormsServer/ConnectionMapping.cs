using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRChat
{
    public class Connection 
    {
        public string ServerId { get; set; }
        public string ClientId { get; set; }
    }

    public class ConnectionMapping : IConnectionMapping
    {
        private static Dictionary<string, string> Connections = new Dictionary<string, string>();

        public ConnectionMapping()
        {

        }

        public void AddAdmin(string Key)
        {
            foreach (KeyValuePair<string, string> b in Connections) // or foreach(book b in books.Values)
            {
                if (b.Key == Key)
                    return;
            }

            Connections.Add(Key, Guid.Empty.ToString());
        }

        public void AddClient(string connectionid)
        {
            string key = string.Empty;
            foreach (KeyValuePair<string, string> b in Connections) // or foreach(book b in books.Values)
            {
                if (b.Value == Guid.Empty.ToString())
                {
                    key = b.Key;
                    break;
                }
                    
            }

            Connections[key] = connectionid;
        }

        public string SearchClient(string connectiondId)
        {
            foreach (KeyValuePair<string, string> b in Connections) // or foreach(book b in books.Values)
            {
                if (b.Value == connectiondId)
                {
                    return b.Key;
                }

            }

            return string.Empty;
        }

        public string SearchAdmin(string connectiondId)
        {
            foreach (KeyValuePair<string, string> b in Connections) // or foreach(book b in books.Values)
            {
                if (b.Key == connectiondId)
                {
                    return b.Value;
                }

            }

            return string.Empty;
        }

    }
}
