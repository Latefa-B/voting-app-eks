using System;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using Npgsql;
using StackExchange.Redis;

namespace Worker {
    public class Program {
        public static int Main(string[] args) {
            try {
                var pgsql = OpenDbConnection("Server=db;Username=postgres;Password=postgres;");
                var redisConn = OpenRedisConnection("redis");
                var redis = redisConn.GetDatabase();
                var definition = new { vote = "", voter_id = "" };
                while (true) {
                    Thread.Sleep(100);
                    if (redisConn == null || !redisConn.IsConnected) {
                        redisConn = OpenRedisConnection("redis");
                        redis = redisConn.GetDatabase();
                    }
                    string json = redis.ListLeftPop("votes");
                    if (json != null) {
                        var vote = JsonConvert.DeserializeAnonymousType(json, definition);
                        Console.WriteLine($"Processing vote for '{vote.vote}' by '{vote.voter_id}'");
                        if (!pgsql.State.Equals(System.Data.ConnectionState.Open)) {
                            pgsql = OpenDbConnection("Server=db;Username=postgres;Password=postgres;");
                        } else {
                            UpdateVote(pgsql, vote.voter_id, vote.vote);
                        }
                    }
                }
            } catch (Exception ex) {
                Console.Error.WriteLine(ex.ToString());
                return 1;
            }
        }

        private static NpgsqlConnection OpenDbConnection(string connectionString) {
            NpgsqlConnection connection;
            while (true) {
                try {
                    connection = new NpgsqlConnection(connectionString);
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"CREATE TABLE IF NOT EXISTS votes
                        (id VARCHAR(255) NOT NULL UNIQUE, vote VARCHAR(255) NOT NULL)";
                    command.ExecuteNonQuery();
                    break;
                } catch (SocketException) {
                    Console.Error.WriteLine("Waiting for db");
                    Thread.Sleep(1000);
                } catch (DbException) {
                    Console.Error.WriteLine("Waiting for db");
                    Thread.Sleep(1000);
                }
            }
            Console.Error.WriteLine("Connected to db");
            return connection;
        }

        private static ConnectionMultiplexer OpenRedisConnection(string hostname) {
            var ipAddress = GetIp(hostname);
            while (true) {
                try {
                    return ConnectionMultiplexer.Connect(ipAddress);
                } catch (RedisConnectionException) {
                    Console.Error.WriteLine("Waiting for redis");
                    Thread.Sleep(1000);
                }
            }
        }

        private static string GetIp(string hostname) {
            return Dns.GetHostEntryAsync(hostname).Result.AddressList
                .First(a => a.AddressFamily == AddressFamily.InterNetwork).ToString();
        }

        private static void UpdateVote(NpgsqlConnection connection, string voterId, string vote) {
            var command = connection.CreateCommand();
            try {
                command.CommandText =
                    "INSERT INTO votes (id, vote) VALUES (@id, @vote) " +
                    "ON CONFLICT (id) DO UPDATE SET vote = EXCLUDED.vote";
                command.Parameters.AddWithValue("@id", voterId);
                command.Parameters.AddWithValue("@vote", vote);
                command.ExecuteNonQuery();
            } catch (DbException exception) {
                Console.Error.WriteLine($"Error: {exception.Message}");
            } finally {
                command.Dispose();
            }
        }
    }
}
