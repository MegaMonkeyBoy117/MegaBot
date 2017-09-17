using Discord;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;

namespace MegaBotv2
{
    public class Database
    {
        private string table { get; set; }
        private const string server = "localhost";
        private const string database = "userinfo";
        private const string username = "root";
        private const string password = "PeterThomson12";
        private MySqlConnection dbConnection;

        public Database(string table)
        {
            this.table = table;
            MySqlConnectionStringBuilder stringBuilder = new MySqlConnectionStringBuilder();
            stringBuilder.Server = server;
            stringBuilder.UserID = username;
            stringBuilder.Password = password;
            stringBuilder.Database = database;
            stringBuilder.SslMode = MySqlSslMode.None;

            var connectionString = stringBuilder.ToString();

            dbConnection = new MySqlConnection(connectionString);

            dbConnection.Open();
        }

        public MySqlDataReader FireCommand(string query)
        {
            if (dbConnection == null)
            { 
                return null;
            }

            MySqlCommand command = new MySqlCommand(query, dbConnection);

            var mySqlReader = command.ExecuteReader();

            return mySqlReader;
        }

        public void CloseConnection()
        {
            if (dbConnection != null)
            {
                dbConnection.Close();
            }
        }

        public static List<String> CheckExistingUser(IUser user)
        {
            var result = new List<String>();
            var database = new Database("userinfo");

            var str = string.Format("SELECT * FROM users WHERE user_id = '{0}'", user.Id);
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var userId = (string)tableName["user_id"];

                result.Add(userId);
            }

            return result;
        }

        public static List<String> CheckExistingServer(IGuild guild)
        {
            var result = new List<String>();
            var database = new Database("userinfo");

            var id = guild.Id;

            var str = $"SELECT * FROM servers WHERE server_id = '{id}'";
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var server_id = (string)tableName["server_id"];

                result.Add(server_id);
            }

            return result;
        }

        public static string EnterUser(IUser user)
        {
            if(user.IsBot == true)
            {
                return null;
            }

            var database = new Database("userinfo");

            var str = $"INSERT INTO users (user_id, username, tokens, level, xp, messages, blacklist) VALUES ('{user.Id}', '{user.Username}', '100', '1', '1', '1', '0')";
            var table = database.FireCommand(str);

            database.CloseConnection();

            return null;
        }

        public static string EnterServer(IGuild guild)
        {
            var database = new Database("userinfo");

            var str = $"INSERT INTO servers (server_id) VALUES ('{guild.Id}')";
            var table = database.FireCommand(str);

            database.CloseConnection();

            return null;
        }

        public static void ChangeTokens(IUser user, uint tokens)
        {
            var database = new Database("userinfo");

            try
            {
                var strings = string.Format("UPDATE users SET tokens = tokens + '{1}' WHERE user_id = {0}", user.Id, tokens);
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch(Exception)
            {
                database.CloseConnection();
                return;
            }
        }

        public static List<userinfo> GetUserStatus(IUser user)
        {
            
            var result = new List<userinfo>();

            var database = new Database("userinfo");

            var str = $"SELECT * FROM users WHERE user_id = '{user.Id.ToString()}'";
            var users = database.FireCommand(str);

            while (users.Read())
            {
              
                var user_id = (string)users["user_id"];
                var username = (string)users["username"];
                var currentTokens = (uint)users["tokens"];
                var level = (int)users["level"];
                var xp = (int)users["xp"];
                var messages = (int)users["messages"];
                var blacklist = (int)users["blacklist"];

                
            result.Add(new userinfo
            {
                 user_id = user_id,
                 username = username,
                 tokens = currentTokens,
                 level = level,
                 xp = xp,
                 messages = messages,
                 blacklist = blacklist

            });
                
            }
            database.CloseConnection();

            return result;
        }

        public static List<userinfo> GetServerStatus(IGuild guild)
        {
            var result = new List<userinfo>();

            var database = new Database("userinfo");

            var check = Database.CheckExistingServer(guild);

            if (check.Count() <= 0)
            {
                Database.EnterServer(guild);
            }

            var str = $"SELECT * FROM servers WHERE server_id = '{guild.Id}'";
            var server = database.FireCommand(str);

            while (server.Read())
            {

                var server_id = (string)server["server_id"];
                var levelupnot = (int)server["levelupnot"];
                var joinnot = (int)server["joinnot"];
                var leavenot = (int)server["leavenot"];

                result.Add(new userinfo
                {
                    server_id = server_id,
                    levelupnot = levelupnot,
                    joinnot = joinnot,
                    leavenot = leavenot

                });
            }
            database.CloseConnection();

            return result;
        }

        public static List<userinfo> GetChannelStatus(IChannel channel)
        {
            var result = new List<userinfo>();

            var database = new Database("userinfo");

            var str = $"SELECT * FROM channels WHERE server_id = '{channel.Id}'";
            var channels = database.FireCommand(str);

            while (channels.Read())
            {

                var channel_id = (string)channels["channel_id"];
                var server_id = (string)channels["server_id"];
                var channel_name = (string)channels["channel_name"];


                result.Add(new userinfo
                {
                    channel_id = channel_id,
                    server_id = server_id,
                    channel_name = channel_name

                });
            }
            database.CloseConnection();

            return result;
        }

        public static int returnXP(SocketMessage msg)
        {
            Random rand = new Random();
            var msgCount = msg.Content.Length;
            var xp = rand.Next(msgCount / 5);

            return xp;
        }

        public static int calculateNextLevel(int currentLevel)
        {
            var calc = Math.Pow(currentLevel + 1, 4);
            var calc2 = Convert.ToInt32(calc);
            return calc2;
        }

        public static void addXP(IUser user, int xp)
        {
            var database = new Database("userinfo");
            var users = Database.GetUserStatus(user);
            int newxp = users.FirstOrDefault().xp += xp;
            try
            {
                var strings = $"UPDATE users SET xp = '{newxp}' WHERE user_id = '{user.Id.ToString()}'"; 
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch(Exception)
            {
                database.CloseConnection();
                return;
            }
        }

        public static void levelUp(IUser user, int xp)
        {
            var database = new Database("userinfo");
            var users = Database.GetUserStatus(user);
            int newlevel = users.FirstOrDefault().level += 1;
            int newxp = users.FirstOrDefault().xp += xp;
            uint newtokens = users.FirstOrDefault().tokens += 100;
            int checkblacklist = users.FirstOrDefault().blacklist;            

            try
            {
                var strings = $"UPDATE users SET level = '{newlevel}', xp = '{newxp}', tokens = '{newtokens}' WHERE user_id = '{user.Id.ToString()}'";
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch(Exception)
            {
                database.CloseConnection();
                return;
            }
        }        

        public static void blacklist(IUser user)
        {
            var database = new Database("userinfo");
            var users = Database.GetUserStatus(user);
            try
            {
                var strings = $"UPDATE users SET blacklist = '1' WHERE user_id = '{user.Id.ToString()}'";
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch (Exception)
            {
                database.CloseConnection();
                return;
            }
        }

        public static void whitelist(IUser user)
        {
            var database = new Database("userinfo");
            var users = Database.GetUserStatus(user);
            try
            {
                var strings = $"UPDATE users SET blacklist = '0' WHERE user_id = '{user.Id.ToString()}'";
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch (Exception)
            {
                database.CloseConnection();
                return;
            }
        }

        public static void tgjoin(IGuild guild)
        {
            var database = new Database("userinfo");
            var servers = Database.GetServerStatus(guild);
            var nowtg = servers.FirstOrDefault().joinnot;

            var newtg = 1;
            
            if(nowtg == 1)
            {
                newtg = 0;
            }
            else
            {
                newtg = 1;
            }

            try
            {
                var strings = $"UPDATE servers SET joinnot = '{newtg}' WHERE server_id = '{guild.Id.ToString()}'";
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch (Exception)
            {
                database.CloseConnection();
                return;
            }
        }

        public static void tgleave(IGuild guild)
        {
            var database = new Database("userinfo");
            var servers = Database.GetServerStatus(guild);
            var nowtg = servers.FirstOrDefault().leavenot;

            var newtg = 1;

            if (nowtg == 1)
            {
                newtg = 0;
            }
            else
            {
                newtg = 1;
            }

            try
            {
                var strings = $"UPDATE servers SET leavenot = '{newtg}' WHERE server_id = '{guild.Id.ToString()}'";
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch (Exception)
            {
                database.CloseConnection();
                return;
            }
        }

        public static void tglevel(IGuild guild)
        {
            var database = new Database("userinfo");
            var servers = Database.GetServerStatus(guild);
            var nowtg = servers.FirstOrDefault().levelupnot;

            var newtg = 1;

            if (nowtg == 1)
            {
                newtg = 0;
            }
            else
            {
                newtg = 1;
            }

            try
            {
                var strings = $"UPDATE servers SET levelupnot = '{newtg}' WHERE server_id = '{guild.Id.ToString()}'";
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch (Exception)
            {
                database.CloseConnection();
                return;
            }
        }

        public static void SetSteamID(IUser user, string steamID)
        {
            var database = new Database("userinfo");

            try
            {
                var strings = $"UPDATE users SET steamid = '{steamID}' WHERE user_id = '{user.Id}'";
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch(Exception)
            {
                database.CloseConnection();
                Console.WriteLine("Couldn't perform the mySQL action: Set steam ID");
                return;
            }
        }

        public static List<userinfo> GetSteamUserStatus(IUser user)
        {
            var result = new List<userinfo>();

            var database = new Database("userinfo");

            var str = string.Format("SELECT * FROM users WHERE user_id = '{0}'", user.Id);
            var users = database.FireCommand(str);

            while (users.Read())
            {
                var steamid = (string)users["steamid"];


                result.Add(new userinfo
                {
                    steamid = steamid
                });
            }
            database.CloseConnection();

            return result;
        }
                   
        }
    }

    

