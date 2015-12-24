using System;
using System.Collections.Generic;
using System.Linq;

namespace ShaneSpace.GameSite.WebApi.Hubs
{
    public class HubGroupManager<T>
    {
        private readonly Dictionary<T, HashSet<string>> _connections = new Dictionary<T, HashSet<string>>();

        public int Count => _connections.Count;

        public void JoinGroup(T key, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(key, out connections))
                {
                    connections = new HashSet<string>();
                    _connections.Add(key, connections);
                }

                lock (connections)
                {
                    connections.Add(connectionId);
                }
            }
        }

        public IEnumerable<string> GetConnections(T key)
        {
            HashSet<string> connections;
            if (_connections.TryGetValue(key, out connections))
            {
                return connections;
            }

            return Enumerable.Empty<string>();
        }

        public Dictionary<T, HashSet<string>> GetConnections()
        {
            return _connections;
        }

        public void LeaveGroup(T key, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(key, out connections))
                {
                    return;
                }

                lock (connections)
                {
                    connections.Remove(connectionId);

                    if (connections.Count == 0)
                    {
                        _connections.Remove(key);
                    }
                }
            }
        }

        internal void LeaveAllGroups(string connectionId)
        {
            var groups = new List<T>();
            lock (_connections)
            {
                foreach(var connection in _connections)
                {
                    if (connection.Value.Contains(connectionId))
                    {
                        groups.Add(connection.Key);
                    }
                }
            }
            foreach (var group in groups)
            {
                LeaveGroup(group, connectionId);
            }
        }
    }
}