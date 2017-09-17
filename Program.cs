using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using System.Windows.Forms;

namespace MegaBotv2
{
    public class Program
    {
        //Declaring variables.
        private CommandService commands;
        private DiscordSocketClient client;
        private IServiceProvider _provider;
        

        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {

            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
            });


            client.Log += Log;                      

            //Setting CommandService to commands
            commands = new CommandService();

            //Initialize command handling.
            await ConfigureAsync();

            //Connect bot to Discord.
            await client.LoginAsync(TokenType.Bot, Token.token);
            await client.StartAsync();
            await client.SetGameAsync("[]help");

            client.UserJoined += AnnounceJoinedUser;
            client.UserLeft += AnnounceLeftUser;

            //Restarts the bot
            await Task.Delay(900000);
            Application.Restart();
            
            
        }


        //Configuring Async
        public async Task ConfigureAsync()
        {
            client.MessageReceived += HandleCommand;
            client.MessageReceived += addMsg;
            client.MessageReceived += giveXP;            
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        //Command handler, returning errors, setting prefix
        public async Task HandleCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;

            if (message == null)
            {
                return;
            }
            int argPos = 1;

            if (!(message.HasStringPrefix("[]", ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos)))
            {
                return;
            }

            if (message.Author.IsBot == true)
            {
                return;
            }

            var user = message.Author;
            var users = Database.GetUserStatus(user);
            var isblacklist = users.FirstOrDefault().blacklist;

            if (isblacklist == 1)
            {
                return;
            }

                
            var context = new CommandContext(client, message);

            var channel = (messageParam.Channel as SocketChannel);            

            var result = await commands.ExecuteAsync(context, argPos, _provider);

            if (!result.IsSuccess)
            {
                //if (result.ErrorReason = CommandError.UnknownCommand)
               // {
                 //   return;
               // }
                await context.Channel.SendMessageAsync("Sorry! " + result.ErrorReason + " Try using []help to see available commands");
            }
        }


        private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
        {
            var message = await before.GetOrDownloadAsync();
            Console.WriteLine($"{message} -> {after}");
        }

        private Task Log(LogMessage message)
        {
            var warningColour = Console.ForegroundColor;

            switch (message.Severity)
            {
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
            }

            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message}");

            return Task.CompletedTask;
        }

        public async Task AnnounceJoinedUser(SocketGuildUser user)
        {
            var id = user.Guild.Id;
            var guildName = client.GetGuild(id);
            ulong ucheck = 1;
            var serverid = user.Guild.Id;

            var result = Database.CheckExistingUser(user);

            if (result.Count() <= 0)
            {
                Database.EnterUser(user);
            }


            if (id == ucheck)
            {
                var channel = user.Guild.DefaultChannel;
                var embed = new EmbedBuilder()
                {
                    Color = new Color(0, 255, 0)
                    
                }               
                .AddField(y =>
                {
                    y.Name = ("Name: ");
                    y.Value = (user);
                    y.IsInline = true;
                })
                .AddField(y =>
                {
                    y.Name = ("Is a bot: ");
                    y.Value = (user.IsBot);
                    y.IsInline = true;
                });

                embed.Title = ("User Joined: ");
                embed.ImageUrl = user.GetAvatarUrl();

                var guild = user.Guild;
                var server = Database.GetServerStatus(guild);
                var announce = server.FirstOrDefault().leavenot;

                if (announce == 0)
                {
                    await channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                }

                var role = user.Guild.Roles.Where(has => has.Name.ToUpper() == "Beginner".ToUpper());
                await user.AddRolesAsync(role);
            }
            else
            {
                var channel = user.Guild.DefaultChannel;
                var embed = new EmbedBuilder()
                {
                    Color = new Color(0, 255, 0)

                }
                .AddField(y =>
                {
                    y.Name = ("Name: ");
                    y.Value = (user);
                    y.IsInline = true;
                })
                .AddField(y =>
                {
                    y.Name = ("Is a bot: ");
                    y.Value = (user.IsBot);
                    y.IsInline = true;
                });

                embed.Title = ("User Joined ");
                embed.ImageUrl = user.GetAvatarUrl();

                var guild = user.Guild;
                var server = Database.GetServerStatus(guild);
                var announce = server.FirstOrDefault().leavenot;

                if (announce == 0)
                {
                    await channel.SendMessageAsync("", false, embed.Build());
                }
                else { }

                var role = user.Guild.Roles.Where(has => has.Name.ToUpper() == "Beginner".ToUpper());
                await user.AddRolesAsync(role);
            }
        }

        public async Task AnnounceLeftUser(SocketGuildUser user)
        {

            var channel = user.Guild.DefaultChannel;
            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 255, 0)

            }
            .AddField(y =>
            {
                y.Name = ("Name: ");
                y.Value = (user);
                y.IsInline = true;
            })
            .AddField(y =>
            {
                y.Name = ("Is a bot: ");
                y.Value = (user.IsBot);
                y.IsInline = true;
            });

            embed.Title = ("User Left ");
            embed.ImageUrl = user.GetAvatarUrl();

            var guild = user.Guild;
            var server = Database.GetServerStatus(guild);
            var announce = server.FirstOrDefault().leavenot;

            if(announce == 0)
            {
                await channel.SendMessageAsync("", false, embed.Build());
            }
            else { }
            

            var role = user.Guild.Roles.Where(has => has.Name.ToUpper() == "Beginner".ToUpper());
            await user.AddRolesAsync(role); ;
        }

        private async Task giveXP(SocketMessage msg) /*Creates the method*/
        {
            var user = msg.Author; /*Sets variable user to msg.author*/
            var result = Database.CheckExistingUser(user); /*We check if the database contains the user*/
            var users = Database.GetUserStatus(user);

            var guild = (msg.Channel as SocketGuildChannel)?.Guild;
            var server = Database.GetServerStatus(guild);
            var announce = server.FirstOrDefault().levelupnot;



            if (result.Count <= 0 && user.IsBot != true) /*Checks if result contains anyone and checks if the user is not a bot*/
            {
                Database.EnterUser(user);  /*Enters the user*/
            }

            if(users.FirstOrDefault().blacklist == 1)
            {
                return;
            }

            var userData = Database.GetUserStatus(user).FirstOrDefault(); /*Gets the users Data from the database*/
            var xp = Database.returnXP(msg); /*sets variable xp to the number returnXP returns we also pass msg through it*/
            var xpToLevelUp = Database.calculateNextLevel(userData.level); /*sets xpToLevelUp to the number calculateNextLevel returns we pass through the users level*/

            if (userData.xp >= xpToLevelUp) /*Checks if the users xp is greater or equal to the xp required to level up*/
            {
                Database.levelUp(user, xp);  /*Levels up the user using the levelUp method we created */

                var tableName = Database.GetUserStatus(user);

                var embed = new EmbedBuilder()
                {
                    Color = new Color(00, 255, 00),
                };

                embed.Title = "Level Up!";
                embed.Description = user.Mention + " has just leveled up to level " + tableName.FirstOrDefault().level;

                if(announce == 0)
                {
                    await msg.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {

                }
                
                
            }
            else if (user.IsBot != true)/*else if the user is not a bot then its just gonna add xp*/
            {
                Database.addXP(user, xp); /*adds the xp to the user*/
            }
        }

        private async Task addMsg(SocketMessage msg)
        {
            var user = msg.Author;
            var database = new Database("userinfo");
            var users = Database.GetUserStatus(user);
            int newmsg = users.FirstOrDefault().messages += 1;
            try
            {
                var strings = $"UPDATE users SET messages = '{newmsg}' WHERE user_id = '{user.Id.ToString()}'";
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

        private async Task restart()
        {
            var restartline = Console.ReadLine();
            if (restartline == "restart")
            {
                Application.Restart();
            }
            else if (restartline == "end")
            {
                await client.LogoutAsync();
                await client.StopAsync();
                Application.Exit();
            }
            else
            {
                await restart();
            }
        }
    }
}