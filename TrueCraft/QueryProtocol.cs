using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TrueCraft.Core.Server;
using TrueCraft.World;

namespace TrueCraft
{
    public class QueryProtocol
    {
        private readonly UdpClient _udp;
        private readonly int _port;
        private readonly Timer _timer;
        private readonly Random _rnd;
        private readonly IMultiplayerServer _server;
        private readonly CancellationTokenSource _cToken;

        private readonly byte[] ProtocolVersion = { 0xFE, 0xFD };
        private readonly byte Type_Handshake = 0x09;
        private readonly byte Type_Stat = 0x00;

        private readonly ConcurrentDictionary<IPEndPoint, QueryUser> _userList;

        public QueryProtocol(IMultiplayerServer server)
        {
            _udp = new UdpClient(_port);
            _port = Program.ServerConfiguration?.QueryPort ?? ServerConfiguration.QueryPortDefault;
            _timer = new Timer(ResetUserList, null, Timeout.Infinite, Timeout.Infinite);
            _rnd = new Random();
            _server = server;
            _cToken = new CancellationTokenSource();
            _userList = new ConcurrentDictionary<IPEndPoint, QueryUser>();
        }

        public void Start()
        {
            _timer.Change(0, 30000);
            _udp.BeginReceive(HandleReceive, null);
        }

        private void HandleReceive(IAsyncResult ar)
        {
            if (_cToken.IsCancellationRequested) return;
            
            try
            {
                IPEndPoint clientEP = new IPEndPoint(IPAddress.Any, _port);
                byte[] buffer = _udp.EndReceive(ar, ref clientEP!);

                DoReverseEndian(buffer);

                if (CheckVersion(buffer))
                {
                    if (buffer[2] == Type_Handshake)
                        HandleHandshake(buffer, clientEP);
                    else if (buffer[2] == Type_Stat)
                    {
                        if (buffer.Length == 11)
                            HandleBasicStat(buffer, clientEP);
                        else if (buffer.Length == 15)
                            HandleFullStat(buffer, clientEP);
                    }
                }
            }
            catch
            {
                // TODO:
                //  1. What more specific Exception type could be caught?
                //  2. When is such an Exception thrown?
                //  3. Why is it safe to ignore?
            }
            if (_cToken.IsCancellationRequested) return;

            _udp.BeginReceive(HandleReceive, null);
        }

        private void HandleHandshake(byte[] buffer, IPEndPoint clientEP)
        {
            using (var ms = new MemoryStream(buffer))
            {
                using (var stream = new BinaryReader(ms))
                {
                    int sessionId = GetSessionId(stream);

                    var user = new QueryUser { SessionId = sessionId, ChallengeToken = _rnd.Next() };

                    if (_userList.ContainsKey(clientEP))
                    {
                        QueryUser u;
                        while (!_userList.TryRemove(clientEP, out u))
                            Thread.Sleep(1);
                    }

                    _userList[clientEP] = user;

                    using (var response = new MemoryStream())
                    {
                        using (var writer = new BinaryWriter(response))
                        {
                            WriteHead(Type_Handshake, user, writer);
                            WriteStringToStream(user.ChallengeToken.ToString(), response);
                            SendResponse(response.ToArray(), clientEP);
                        }
                    }
                }

            }
        }

        private void HandleBasicStat(byte[] buffer, IPEndPoint clientEP)
        {
            using (var ms = new MemoryStream(buffer))
            {
                using (var stream = new BinaryReader(ms))
                {
                    int sessionId = GetSessionId(stream);
                    int token = GetToken(stream);

                    var user = GetUser(clientEP);
                    if (user.ChallengeToken != token || user.SessionId != sessionId) throw new Exception("Invalid credentials");

                    var stats = GetStats();
                    using (var response = new MemoryStream())
                    {
                        using (var writer = new BinaryWriter(response))
                        {
                            WriteHead(Type_Stat, user, writer);
                            WriteStringToStream(stats["hostname"], response);
                            WriteStringToStream(stats["gametype"], response);
                            WriteStringToStream(stats["numplayers"], response);
                            WriteStringToStream(stats["maxplayers"], response);
                            byte[] hostport = BitConverter.GetBytes(ushort.Parse(stats["hostport"]));
                            Array.Reverse(hostport);//The specification needs little endian short
                            writer.Write(hostport);
                            WriteStringToStream(stats["hostip"], response);

                            SendResponse(response.ToArray(), clientEP);
                        }
                    }
                }
            }
        }

        private void HandleFullStat(byte[] buffer, IPEndPoint clientEP)
        {
            using (var stream = new MemoryStream(buffer))
            {
                using (var reader = new BinaryReader(stream))
                {
                    int sessionId = GetSessionId(reader);
                    int token = GetToken(reader);

                    var user = GetUser(clientEP);
                    if (user.ChallengeToken != token || user.SessionId != sessionId) throw new Exception("Invalid credentials");

                    var stats = GetStats();
                    using (var response = new MemoryStream())
                    {
                        using (var writer = new BinaryWriter(response))
                        {
                            WriteHead(Type_Stat, user, writer);
                            WriteStringToStream("SPLITNUM\0\0", response);
                            foreach (var pair in stats)
                            {
                                WriteStringToStream(pair.Key, response);
                                WriteStringToStream(pair.Value, response);
                            }
                            writer.Write((byte)0x00);
                            writer.Write((byte)0x01);
                            WriteStringToStream("player_\0", response);
                            var players = GetPlayers();
                            foreach (string player in players)
                                WriteStringToStream(player, response);
                            writer.Write((byte)0x00);

                            SendResponse(response.ToArray(), clientEP);
                        }
                    }
                }
            }
        }

        private bool CheckVersion(byte[] ver)
        {
            return ver[0] == ProtocolVersion[0] && ver[1] == ProtocolVersion[1];
        }
        private int GetSessionId(BinaryReader stream)
        {
            stream.BaseStream.Position = 3;
            return stream.ReadInt32();
        }
        private int GetToken(BinaryReader stream)
        {
            stream.BaseStream.Position = 7;
            return stream.ReadInt32();
        }

        private void WriteHead(byte type, QueryUser user, BinaryWriter stream)
        {
            stream.Write(type);
            stream.Write(user.SessionId);
        }

        private void SendResponse(byte[] res, IPEndPoint destination)
        {
            _udp.Send(res, res.Length, destination);
        }
        private QueryUser GetUser(IPEndPoint ipe)
        {
            if (!_userList.ContainsKey(ipe))
                throw new Exception("Undefined user");

            return _userList[ipe];
        }
        private Dictionary<string, string> GetStats()
        {
            var stats = new Dictionary<string, string>
            {
                // TODO: why is a key called hostname storing the Message Of The Day???
                {"hostname", Program.ServerConfiguration?.MOTD ?? String.Empty},
                {"gametype", "SMP"},
                {"game_id", "TRUECRAFT"},
                {"version", "1.0"},
                {"plugins", "TrueCraft"},
                {"map", ((IWorld?)_server.World)?.Name ?? String.Empty },
                {"numplayers", _server.Clients.Count.ToString()},
                {"maxplayers", "64"},
                {"hostport", (Program.ServerConfiguration?.ServerPort ?? ServerConfiguration.ServerPortDefault).ToString()},
                {"hostip", Program.ServerConfiguration?.ServerAddress ?? "?" }
            };
            return stats;
        }

        private List<string> GetPlayers()
        {
            var names = new List<string>();
            lock (Program.Server!.ClientLock)
                foreach (var client in _server.Clients)
                    names.Add(client.Username ?? "n/a");
            return names;
        }

        private void DoReverseEndian(byte[] buffer)
        {
            if (buffer.Length >= 7)
            {
                Swap(ref buffer[3], ref buffer[6]);
                Swap(ref buffer[4], ref buffer[5]);
            }
            if (buffer.Length >= 11)
            {
                Swap(ref buffer[7], ref buffer[10]);
                Swap(ref buffer[8], ref buffer[9]);
            }
        }
        private void Swap(ref byte a, ref byte b)
        {
            byte c = a;
            a = b;
            b = c;
        }

        public void Stop()
        {
            _timer.Dispose();
            _cToken.Cancel();
            _udp.Close();
        }

        private void ResetUserList(object? state)
        {
            _userList.Clear();
        }

        struct QueryUser
        {
            public int SessionId;
            public int ChallengeToken;
        }

        private byte[] String0ToBytes(string s)
        { return Encoding.UTF8.GetBytes(s + "\0"); }
        private void WriteToStream(byte[] bytes, Stream stream)
        { stream.Write(bytes, 0, bytes.Length); }
        private void WriteStringToStream(string s, Stream stream)
        { WriteToStream(String0ToBytes(s), stream); }
    }
}
