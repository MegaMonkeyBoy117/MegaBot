using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Diagnostics;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Audio;
using System.Net.Http;
using System.Data;
using Newtonsoft.Json;

namespace MegaBotv2
{
    public class BasicCommands : ModuleBase
    {
        private CommandService _service;
        private DiscordSocketClient client;
        private IServiceProvider _provider;
        private SocketCommandContext context;
        Random rnd = new Random();

        //Embeding
        EmbedBuilder MyEmbedBuilder = new EmbedBuilder();

        public BasicCommands(CommandService service)
        {
            _service = service;
        }

        [Command("help")]
        [Summary("Shows a list of all available commands per module, for specifics, do []help (command)!")]
        public async Task HelpAsync()
        {
            var dmChannel = await Context.User.GetOrCreateDMChannelAsync(); /* A channel is created so that the commands will be privately sent to the user, and not flood the chat. */

            var builder = new EmbedBuilder()
            {
                Color = new Color(0, 255, 0),
                Description = "These are the commands you can use, for specifics do []help (command)!"
            };

            foreach (var module in _service.Modules) /* we are now going to loop through the modules taken from the service we initiated earlier ! */
            {
                string description = null;
                foreach (var cmd in module.Commands) /* and now we loop through all the commands per module aswell, oh my! */
                {
                    var result = await cmd.CheckPreconditionsAsync(Context); /* gotta check if they pass */
                    if (result.IsSuccess)
                        description += $"{cmd.Aliases.First()}\n"; /* if they DO pass, we ADD that command's first alias (aka it's actual name) to the description tag of this embed */
                }

                if (!string.IsNullOrWhiteSpace(description)) /* if the module wasn't empty, we go and add a field where we drop all the data into! */
                {
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }           

            try
            {
                await ReplyAsync(Context.User.Mention + " Check your DM's!");
                await dmChannel.SendMessageAsync("", false, builder.Build()); /* then we send it to the user. */
            }
            catch(Exception)
            {
                await ReplyAsync("", false, builder.Build()); /* then we send it to the user. */
            }
        }

        [Command("help")]
        [Summary("Shows what a specific command does and what parameters it takes.")]
        public async Task HelpAsync(string command)
        {
            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
            var result = _service.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                return;
            }

            var builder = new EmbedBuilder()
            {
                Color = new Color(0, 225, 0),
                Description = $"Here are some commands like **{command}**"
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;

                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" + $"Remarks: {cmd.Summary}";
                    x.IsInline = true;
                });
            }
            await dmChannel.SendMessageAsync("", false, builder.Build());
        }

        [Command("rps")]
        [Summary("Play a game of Rock Paper Scissors against the bot!")]
        public async Task rps(string playerSelection)
        {
            int botNumber = rnd.Next(2);
            string botSelection;
            bool correctHand;
            playerSelection = playerSelection.ToLower();
            EmbedBuilder embedBuilder;
            embedBuilder = new EmbedBuilder();
            embedBuilder.WithColor(new Color(0, 225, 0));

            if (playerSelection == "rock")
            {
                correctHand = true;
            }
            else if (playerSelection == "scissors")
            {
                correctHand = true;
            }
            else if (playerSelection == "paper")
            {
                correctHand = true;
            }
            else
            {
                correctHand = false;
            }

            if (correctHand == false)
            {
                await ReplyAsync("Invalid hand, please try again");
                return;
            }

            if (botNumber == 0)
            {
                botSelection = "Scissors";
                embedBuilder.Description = ("The bot chose: " + botSelection);
            }
            else if (botNumber == 1)
            {
                botSelection = "Rock";
                embedBuilder.Description = ("The bot chose: " + botSelection);
            }
            else
            {
                botSelection = "Paper";
                embedBuilder.Description = ("The bot chose: " + botSelection);
            }

            botSelection = botSelection.ToLower();

            if (botSelection == playerSelection)
            {
                embedBuilder.Title = ("Draw!");
            }
            else if (botSelection == "scissors" && playerSelection == "rock")
            {
                embedBuilder.Title = ("You win!");
            }
            else if (botSelection == "rock" && playerSelection == "paper")
            {
                embedBuilder.Title = ("You win!");
            }
            else if (botSelection == "paper" && playerSelection == "scissor")
            {
                embedBuilder.Title = ("You win!");
            }
            else
            {
                embedBuilder.Title = ("You lose!");
            }

            await ReplyAsync("", false, embedBuilder.Build());
        }

        [Command("roll")]
        [Summary("Roll a number up to whatever you enter!")]
        public async Task roll(int maxRoll)
        {
            if (maxRoll < 1)
            {
                await ReplyAsync("Sorry, value cant be lower than 1");
                return;
            }
            int roll = rnd.Next(1, maxRoll + 1);
            EmbedBuilder embedBuilder;
            embedBuilder = new EmbedBuilder();
            embedBuilder.WithColor(new Color(0, 225, 0));
            embedBuilder.Title = ("Rolling... :game_die: ");
            embedBuilder.Description = (Context.User.Mention + " rolled: " + roll);
            await ReplyAsync("", false, embedBuilder.Build());
        }

        [Command("uptime")]
        [Summary("Get the bot uptime")]
        public async Task uptime()
        {
            using (var process = Process.GetCurrentProcess())
            {
                /*this is required for up time*/
                var embed = new EmbedBuilder();
                var application = await Context.Client.GetApplicationInfoAsync(); /*for lib version*/
                embed.WithColor(new Color(0, 255, 0)) /*Hexacode colours*/
                .AddField(y =>  /* add new field, rinse and repeat*/
                {
                    y.Name = "Uptime:";
                    var time = DateTime.Now - process.StartTime; /* Subtracts current time and start time to get Uptime*/
                    var sb = new StringBuilder();
                    if (time.Days > 0)
                    {
                        sb.Append($"{time.Days}d ");
                    }
                    if (time.Hours > 0)
                    {
                        sb.Append($"{time.Hours}h ");
                    }
                    if (time.Minutes > 0)
                    {
                        sb.Append($"{time.Minutes}m ");
                    }
                    sb.Append($"{time.Seconds}s ");
                    y.Value = sb.ToString();
                    y.IsInline = true;
                });

                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("botinfo")]
        [Summary("Shows All Bot Info.")]
        public async Task Info()
        {
            using (var process = Process.GetCurrentProcess())
            {
                /*this is required for up time*/
                var embed = new EmbedBuilder();
                var application = await Context.Client.GetApplicationInfoAsync(); /*for lib version*/
                embed.ImageUrl = application.IconUrl; /*pulls bot Avatar. Not needed can be removed*/
                embed.WithColor(new Color(0, 255, 0)) /*Hexacode colours*/
                .AddField(y =>
                {
                    y.Name = "Author:";  /*Field name here*/
                    y.Value = application.Owner.Username; application.Owner.Id.ToString(); /*Code here. If INT convert to string*/
                    y.IsInline = true;
                })
                .AddField(y =>  /* add new field, rinse and repeat*/
                {
                    y.Name = "Uptime:";
                    var time = DateTime.Now - process.StartTime; /* Subtracts current time and start time to get Uptime*/
                    var sb = new StringBuilder();
                    if (time.Days > 0)
                    {
                        sb.Append($"{time.Days}d ");
                    }
                    if (time.Hours > 0)
                    {
                        sb.Append($"{time.Hours}h ");
                    }
                    if (time.Minutes > 0)
                    {
                        sb.Append($"{time.Minutes}m ");
                    }
                    sb.Append($"{time.Seconds}s ");
                    y.Value = sb.ToString();
                    y.IsInline = true;
                })
                .AddField(y =>
                {
                    y.Name = "Discord.net version:"; /*pulls discord lib version*/
                    y.Value = DiscordConfig.Version;
                    y.IsInline = true;
                }).AddField(y =>
                {
                    y.Name = "Server Amount:";
                    y.Value = (Context.Client as DiscordSocketClient).Guilds.Count.ToString(); /*Numbers of servers the bot is in*/
                    y.IsInline = true;
                })
                .AddField(y =>
                {
                    y.Name = "Available RAM:"; /*pulls ram usage of modules/heaps*/
                    y.Value = System.Environment.WorkingSet /4096  + " Mb";
                    y.IsInline = true;
                })
                .AddField(y =>
                {
                    y.Name = "Number Of Users:";
                    y.Value = (Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Users.Count).ToString(); /*Counts users*/
                    y.IsInline = true;
                })
                .AddField(y =>
                {
                    y.Name = "Channels:";
                    y.Value = (Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count).ToString();
                    y.IsInline = true;
                })
                .AddField(y =>
                {
                    y.Name = "Commands: ";
                    int numcommands = 0;
                    foreach (var module in _service.Modules)
                    {
                        foreach (var cmd in module.Commands) /* and now we loop through all the commands per module aswell, oh my! */
                        {
                            numcommands += 1;
                        }
                    }
                    y.Value = numcommands;
                })
                .AddField(y =>
                {
                    y.Name = "Operating System: ";
                    var os = System.Environment.OSVersion;
                    y.Value = (os);
                    y.IsInline = true;
                })
                .AddField(y =>
                {
                    y.Name = "Processor Count: ";
                    var processors = System.Environment.ProcessorCount;
                    y.Value = (processors);
                    y.IsInline = true;
                });
                await ReplyAsync("", false, embed.Build());

            }
        }

        [Command("userinfo")]
        [Summary("Gives information about a user.")]
        public async Task UserInfo([Remainder]IUser user = null)
        {
            if(user == null)
            {
                user = Context.User;
            }
            var application = await Context.Client.GetApplicationInfoAsync(); // Get client info
            var thumbnailurl = user.GetAvatarUrl();
            var date = $"{user.CreatedAt.Day}/{user.CreatedAt.Month}/{user.CreatedAt.Year}/";
            var auth = new EmbedAuthorBuilder()
            {
                Name = user.Username,
                IconUrl = thumbnailurl,
            };

            var embed = new EmbedBuilder()
            {
                Color = new Color(00, 255, 00),
                Author = auth
            };

            var us = user as SocketGuildUser;

            var D = us.Username;

            var A = us.Discriminator;
            var T = us.Id;
            var S = date;
            var C = us.Status;
            var CC = us.JoinedAt;
            var O = us.Game;
            embed.Title = $"**{us.Username}** Information";
            embed.Description = $"Username: **{D}**\nDiscriminator: **{A}**\nUser ID: **{T}**\nCreated at: **{S}**\nCurrent Status: **{C}**\nJoined server at: **{CC}**\nPlaying: **{O}**";

            await ReplyAsync("", false, embed.Build());
        }

        [Command("serverinfo")]
        [Summary("Returns info about the server.")]
        public async Task GuildInfo()
        {
            EmbedBuilder embedBuilder;
            embedBuilder = new EmbedBuilder();
            embedBuilder.WithColor(new Color(0, 225, 0));

            var gld = Context.Guild as SocketGuild;
            var client = Context.Client as DiscordSocketClient;


            if (!string.IsNullOrWhiteSpace(gld.IconUrl))
                embedBuilder.ThumbnailUrl = gld.IconUrl;
            var O = gld.Owner.Username;

            var V = gld.VoiceRegionId;
            var C = gld.CreatedAt;
            var N = gld.DefaultMessageNotifications;
            var VL = gld.VerificationLevel;
            var XD = gld.Roles.Count;
            var X = gld.MemberCount;
            var Z = client.ConnectionState;

            embedBuilder.Title = $"{gld.Name} Server Information";
            embedBuilder.Description = $"Server Owner: **{O}\n**Voice Region: **{V}\n**Created At: **{C}\n**Message Notification: **{N}\n**Verification: **{VL}\n**Role Count: **{XD}\n **Members: **{X}\n **Conntection state: **{Z}\n\n**";
            await ReplyAsync("", false, embedBuilder.Build());
        }

        [Command("dm")]
        [Summary("DM's the owner (Please only use for bugs or suggestions)")]
        public async Task Dm([Remainder] string dm)
        {
            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 225, 0)
            };
            var user = Context.User;
            var application = await Context.Client.GetApplicationInfoAsync();
            var ownerdm = await application.Owner.GetOrCreateDMChannelAsync();
            var senderdm = await Context.User.GetOrCreateDMChannelAsync();
            embed.Description = $"`{Context.User.Username}` **from** `{Context.Guild.Name}` ** has sent you a message!**\n\n{dm}";
            await ownerdm.SendMessageAsync("", false, embed.Build());
            var embed2 = new EmbedBuilder()
            {
                Color = new Color(0, 225, 0)
            };
            embed2.Title = ("Message sent to Dev!");
            embed2.Description = ("Your message has been sent, the dev will respond shortly :)");
            await ownerdm.SendMessageAsync("", false, embed2.Build());

        }

        [Command("poll")]
        [Summary("Creates a poll about whatever you enter")]
        public async Task poll(string poll)
        {
            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 225, 0)
            };

            embed.Title = ("A poll has started: ");
            embed.Description = (context.User.Mention + " wants to vote on: " + poll);
            var message = await ReplyAsync("", false, embed.Build());
            //await message.AddReactionAsync(IEmote );
        }

        [Command("ping")]
        [Summary("Returns a pong.")]
        public async Task say()
        {
            var embed = new EmbedBuilder()
            {
                Color = new Color(00, 255, 00),
            };

            client = new DiscordSocketClient();
            var ping = client.Latency;
            
            embed.Title = ("Pong!");
            embed.Description = ("Latency: `" + ping + "`");
            await ReplyAsync("", false, embed.Build());
        }
    }
   
    public class MusicCommands : ModuleBase
    {
        private CommandService _service;
        private IServiceProvider _provider;
        Random rnd = new Random();

        private Process CreateStream(string url)
        {
            Process currentsong = new Process();

            currentsong.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C youtube-dl.exe -o - {url} | ffmpeg -i pipe:0 -ac 1 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };

            currentsong.Start();
            return currentsong;
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task play(string url)
        {
            IVoiceChannel channel = (Context.User as IVoiceState).VoiceChannel;
            IAudioClient client = await channel.ConnectAsync();

            var output = CreateStream(url).StandardOutput.BaseStream;
            var stream = client.CreatePCMStream(AudioApplication.Music, 128 * 1024);
            output.CopyToAsync(stream);
            stream.FlushAsync().ConfigureAwait(false);
        }

        [Command("join")]
        [Summary("Makes the bot join the channel")]
        public async Task join()
        {
            IVoiceChannel channel = (Context.User as IVoiceState).VoiceChannel;
            IAudioClient client = await channel.ConnectAsync();
        }

        [Command("leave")]
        [Summary("Makes the bot leave the channel")]
        public async Task leave()
        {
            IVoiceChannel channel = (Context.User as IVoiceState).VoiceChannel;
            IAudioClient client = await channel.ConnectAsync();
        }        
    }   

    public class StatsCommands : ModuleBase
    {
        private CommandService _service;
        private DiscordSocketClient client;
        private IServiceProvider _provider;

        public async Task GetUserDataAsync(string steamIDtoQuery)
        {
            Console.WriteLine("Querying API");
            try
            {
                var vc = new HttpClient();
                string req = await vc.GetStringAsync("http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=192923F4C4BA80829FEE5D285FC9997E&steamids=" + steamIDtoQuery);
                RootObject resp;
                resp = JsonConvert.DeserializeObject<RootObject>(req);
                Console.WriteLine(resp);
                var test = resp.response.players[0].steamid;
                Console.WriteLine(test);
                var userStatus = " ";
                var gameName = " ";
                if (resp.response.players[0].personastate == 1)
                {
                    userStatus = "Online.";
                }
                else
                {
                    userStatus = "Offline.";
                }
                if (resp.response.players[0].gameextrainfo != null)
                {
                    gameName = resp.response.players[0].gameextrainfo;
                }
                else
                {
                    gameName = "Not ingame.";
                }
                var lastlogoff = DateTimeOffset.FromUnixTimeSeconds(resp.response.players[0].lastlogoff).LocalDateTime;
                var embed = new EmbedBuilder()               
                .AddField(fb => fb.WithName(Format.Bold(("Steam Details of : "))).WithValue($"{resp.response.players[0].personaname}"))
                .AddField(fb => fb.WithName(Format.Bold(("Profile URL: "))).WithValue($"{resp.response.players[0].profileurl}"))
                .AddField(fb => fb.WithName(Format.Bold(("User Status: "))).WithValue(userStatus))
                .AddField(fb => fb.WithName(Format.Bold(("In game: "))).WithValue(gameName))
                .AddField(fb => fb.WithName(Format.Bold(("Last Log off : "))).WithValue(lastlogoff));
                embed.Color = new Color(0, 255, 0);
                embed.WithThumbnailUrl(resp.response.players[0].avatarmedium);
                await Context.Channel.SendMessageAsync("", false, embed.Build());

            }
            catch (Exception e)
            {
                await Context.Channel.SendMessageAsync(e.ToString());
            }
        }

        [Command("status")]
        [Summary("Posts information about a users status on the bot.")]
        public async Task Say([Remainder]IUser user = null)
        {
            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 255, 0)
            };

            if (user == null)
            {
                user = Context.User;
            }

            if(user.IsBot == true)
            {
                await ReplyAsync("Bot's do not have statuses, sorry!");
                return;
            }

            var result = Database.CheckExistingUser(user);

            if (result.Count() <= 0)
            {
                Database.EnterUser(user);
            }
            
            var tableName = Database.GetUserStatus(user);

            var isblacklist = tableName.FirstOrDefault().blacklist;
            var textblacklist = "Isn't";
            if(isblacklist == 1)
            {
                textblacklist = "Is";
            }
            else
            {
            }

            embed.Title = ("Your status is as follows: ");
            embed.Description = (user.Mention + ", \n\n :shamrock: " + "Level: " + tableName.FirstOrDefault().level + "\n" +
                            ":shamrock: " + tableName.FirstOrDefault().xp + " xp!\n" +
                            ":shamrock: " + tableName.FirstOrDefault().tokens + " tokens to spend!\n" +
                            ":shamrock: " + textblacklist + " blacklisted!\n" +
                            ":shamrock: " + tableName.FirstOrDefault().messages + " messages sent!" );
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("grant")]
        [Summary("Grant a user tokens (Admins only)")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Award(SocketGuildUser user, [Remainder] uint tokens)
        {
            if(user.IsBot == true)
            {
                await ReplyAsync("Bot's do not have tokens, sorry!");
                return;
            }

            var result = Database.CheckExistingUser(user);

            if (result.Count() <= 0)
            {
                Database.EnterUser(user);
            }

            var tableName = Database.GetUserStatus(user);

            Database.ChangeTokens(user, tokens);
            await ReplyAsync(user.Mention + " was awarded " + tokens + " tokens by " + Context.User.Mention);
            uint usertokens = tableName.FirstOrDefault().tokens;
            await ReplyAsync(user.Mention + " now has " + usertokens + " tokens!");
        }

        [Command("progress")]
        [Summary("See your progress to your next level.")]
        public async Task Progress(IUser user = null)
        {
            if (user == null)
            {
                user = Context.User;
            }

            if (user.IsBot == true)
            {
                await ReplyAsync("Bot's do not have xp, sorry!");
                return;
            }

            var result = Database.CheckExistingUser(user);

            if (result.Count() <= 0)
            {
                Database.EnterUser(user);
            }

            var users = Database.GetUserStatus(user);
            var userlevel = users.FirstOrDefault().level;
            var userxp = users.FirstOrDefault().xp;
            var progress = Database.calculateNextLevel(userlevel) - userxp;

            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 255, 0)
            };

            embed.Title = ("Progress towards next level:");
            embed.Description = (user.Mention + " you need " + progress + " xp to level up!");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("steamid")]
        [Summary("Set your Steam ID")]
        public async Task SetSteamID([Remainder] string steamid)
        {
            var user = Context.User;
            Console.WriteLine("Sending mySQL query...");
            Database.SetSteamID(user, steamid);
            Console.WriteLine("SteamID saved!");
            await ReplyAsync(user.Mention + ", your steamID was successfully set as: " + steamid);
        }

        [Command("steam")]
        [Remarks("Get information on a user using their stored steamID!")]
        public async Task SteamCommand([Remainder] IUser user = null)
        {
            if (user == null)
            {
                user = Context.User;
            }

            var resultTable = Database.GetSteamUserStatus(user);
            var steamID = resultTable[0].steamid;
            await GetUserDataAsync(steamID);
        }
    }

    public class FightCommands : ModuleBase
    {
        private CommandService _service;
        private DiscordSocketClient client;
        private IServiceProvider _provider;

        static string player1;
        static string player2;
        static int player1mana = 3;
        static int player2mana = 3;
        static string whosTurn;
        static string whoWaits;
        static string placeholder;
        static int health1 = 100;
        static int health2 = 100;
        static string SwitchCaseString = "nofight";

        [Command("fight")]
        [Summary("Starts a fight with the mentions user.")]
        public async Task Fight(SocketGuildUser user)
        {

            if (Context.User.Mention != user.Mention && SwitchCaseString == "nofight" && user.IsBot != true)
            {

            }
            else
            {
                await ReplyAsync(Context.User.Mention + "Sorry, but there is already a fight going on, or you're trying to fight yourself, or the bot");
            }

            SwitchCaseString = "fight_p1";
            player1 = Context.User.Mention;
            player2 = user.Mention;

            string[] whoStarts = new string[]
            {
                Context.User.Mention,
                user.Mention
            };

            Random rand = new Random();

            int randomIndex = rand.Next(whoStarts.Length);
            string text = whoStarts[randomIndex];

            whosTurn = text;
            if (text == Context.User.Mention)
            {
                whoWaits = user.Mention;
            }
            else
            {
                whoWaits = Context.User.Mention;
            }

            await ReplyAsync("ITS TIME!");
            await Task.Delay(200);
            await ReplyAsync("TO");
            await Task.Delay(200);
            var message = await ReplyAsync("D-");
            await Task.Delay(200);
            await message.ModifyAsync(msg => msg.Content = "D-D-");
            await Task.Delay(200);
            await message.ModifyAsync(msg => msg.Content = "D-D-D-");
            await Task.Delay(200);
            await message.ModifyAsync(msg => msg.Content = "D-D-D-DUEL");
            await Task.Delay(200);
            await ReplyAsync("Fight started between " + Context.User.Mention + " and " + user.Mention + "!\n \n" + player1 + " you got " + health1 + " health!\n" + player2 + " you got " + health2 + " health!\n \n" + text + " it is your turn!");
        }

        [Command("giveup")]
        [Summary("Stops the fight and gives up.")]
        public async Task GiveUp()
        {
            if (SwitchCaseString == "fight_p1")
            {
                await ReplyAsync("The fight has stopped.");
                SwitchCaseString = "nofight";
                health1 = 100;
                health2 = 100;
            }
            else
            {
                await ReplyAsync("There is no fight to stop");
            }
        }

        [Command("slash")]
        [Summary("Slashes your foe with a sword. Good accuracy and medium damage.")]
        public async Task Slash()
        {
            if (SwitchCaseString == "fight_p1")
            {
                if (whosTurn == Context.User.Mention)
                {
                    Random rand = new Random();

                    int randomIndex = rand.Next(1, 5);

                    if (randomIndex != 1)
                    {
                        Random rand2 = new Random();

                        int damage = rand2.Next(7, 15);
                        int crit = rand2.Next(1, 11);

                        if (crit == 1)
                        {
                            damage = damage * 2;
                            await ReplyAsync(Context.User.Mention + " You Crit! x2 damage!");
                        }

                        await ReplyAsync(Context.User.Mention + " You hit and did " + damage + " damage!");

                        if (Context.User.Mention != player1)
                        {
                            health1 = health1 - damage;
                            if (health1 > 0)
                            {
                                placeholder = whosTurn;
                                whosTurn = whoWaits;
                                whoWaits = placeholder;

                                await ReplyAsync(player1 + " has " + health1 + " health left and " + player1mana + " mana!\n" + player2 + " got " + health2 + " health left and " + player2mana + " mana!\n\n" + whosTurn + " your turn!");
                            }
                            else
                            {
                                await ReplyAsync(player1 + " died. " + player2 + " won!");
                                SwitchCaseString = "nofight";
                                health1 = 100;
                                health2 = 100;
                            }
                        }
                        else if (Context.User.Mention == player1)
                        {
                            health2 = health2 - damage;
                            if (health2 > 0)
                            {
                                placeholder = whosTurn;
                                whosTurn = whoWaits;
                                whoWaits = placeholder;

                                await ReplyAsync(player1 + " has " + health1 + " health left and " + player1mana + " mana!\n" + player2 + " got " + health2 + " health left and " + player2mana + " mana!\n\n" + whosTurn + " your turn!");
                            }
                            else
                            {
                                await ReplyAsync(player2 + " died. " + player1 + " won!");
                                SwitchCaseString = "nofight";
                                health1 = 100;
                                health2 = 100;
                            }
                        }
                        else
                        {
                            await ReplyAsync("Sorry it seems like something went wrong. Please type !giveup");
                        }

                    }
                    else
                    {
                        await ReplyAsync(Context.User.Mention + " Sorry, you missed");

                        placeholder = whosTurn;
                        whosTurn = whoWaits;
                        whoWaits = placeholder;

                        await ReplyAsync(whosTurn + " It's your turn!");
                    }
                }
                else
                {
                    await ReplyAsync(Context.User.Mention + " it is not your turn!");
                }
            }
            else
            {
                await ReplyAsync("There is no fight at the moment");
            }
        }

        [Command("pound")]
        [Summary("A heavy attack with a sword, 50% chance to miss though.")]
        public async Task Pound()
        {
            if (SwitchCaseString == "fight_p1")
            {
                if (whosTurn == Context.User.Mention)
                {
                    Random rand = new Random();

                    int randomIndex = rand.Next(1, 3);

                    if (randomIndex != 1)
                    {
                        Random rand2 = new Random();

                        int damage = rand2.Next(20, 31);
                        int crit = rand2.Next(1, 11);

                        if (crit == 1)
                        {
                            damage = damage * 2;
                            await ReplyAsync(Context.User.Mention + " You Crit! x2 damage!");
                        }

                        await ReplyAsync(Context.User.Mention + " You hit and did " + damage + " damage!");

                        if (Context.User.Mention != player1)
                        {
                            health1 = health1 - damage;
                            if (health1 > 0)
                            {
                                placeholder = whosTurn;
                                whosTurn = whoWaits;
                                whoWaits = placeholder;

                                await ReplyAsync(player1 + " has " + health1 + " health left and " + player1mana + " mana!\n" + player2 + " got " + health2 + " health left and " + player2mana + " mana!\n\n" + whosTurn + " your turn!");
                            }
                            else
                            {
                                await ReplyAsync(player1 + " died. " + player2 + " won!");
                                SwitchCaseString = "nofight";
                                health1 = 100;
                                health2 = 100;
                            }
                        }
                        else if (Context.User.Mention == player1)
                        {
                            health2 = health2 - damage;
                            if (health2 > 0)
                            {
                                placeholder = whosTurn;
                                whosTurn = whoWaits;
                                whoWaits = placeholder;

                                await ReplyAsync(player1 + " has " + health1 + " health left and " + player1mana + " mana!\n" + player2 + " got " + health2 + " health left and " + player2mana + " mana!\n\n" + whosTurn + " your turn!");
                            }
                            else
                            {
                                await ReplyAsync(player2 + " died. " + player1 + " won!");
                                SwitchCaseString = "nofight";
                                health1 = 100;
                                health2 = 100;
                            }
                        }
                        else
                        {
                            await ReplyAsync("Sorry it seems like something went wrong. Please type !giveup");
                        }

                    }
                    else
                    {
                        await ReplyAsync(Context.User.Mention + " Sorry, you missed");

                        placeholder = whosTurn;
                        whosTurn = whoWaits;
                        whoWaits = placeholder;

                        await ReplyAsync(whosTurn + " It's your turn!");
                    }
                }
                else
                {
                    await ReplyAsync(Context.User.Mention + " it is not your turn!");
                }
            }
            else
            {
                await ReplyAsync("There is no fight at the moment");
            }
        }

        [Command("fireball")]
        [Summary("Shoot a fireball, high damage and accuracy but limited uses.")]
        public async Task Fireball()
        {
            if (SwitchCaseString == "fight_p1")
            {
                if (whosTurn == Context.User.Mention)
                {
                    Random rand = new Random();

                    int randomIndex = rand.Next(1, 5);

                    if (randomIndex != 1)
                    {
                        Random rand2 = new Random();

                        int damage = rand2.Next(15, 21);
                        int crit = rand2.Next(1, 11);

                        if (Context.User.Mention == player1)
                        {
                            if(player1mana <= 0)
                            {
                                await ReplyAsync(Context.User.Mention + " Sorry you have no mana!");
                                return;
                            }
                            else
                            {
                                player1mana -= 1;
                            }
                        }
                        else if(Context.User.Mention == player2)
                        {
                            if (player2mana <= 0)
                            { 
                                await ReplyAsync(Context.User.Mention + " Sorry you have no mana!");
                                return;
                            }
                            else
                            {
                                player2mana -= 1;
                            }
                        }

                        if (crit == 1)
                        {
                            damage = damage * 2;
                            await ReplyAsync(Context.User.Mention + " You Crit! x2 damage!");
                        }

                        await ReplyAsync(Context.User.Mention + " You hit and did " + damage + " damage!");

                        if (Context.User.Mention != player1)
                        {
                            health1 = health1 - damage;
                            if (health1 > 0)
                            {
                                placeholder = whosTurn;
                                whosTurn = whoWaits;
                                whoWaits = placeholder;

                                await ReplyAsync(player1 + " has " + health1 + " health left and " + player1mana + " mana!\n" + player2 + " got " + health2 + " health left and " + player2mana + " mana!\n\n" + whosTurn + " your turn!");
                            }
                            else
                            {
                                await ReplyAsync(player1 + " died. " + player2 + " won!");
                                SwitchCaseString = "nofight";
                                health1 = 100;
                                health2 = 100;
                            }
                        }
                        else if (Context.User.Mention == player1)
                        {
                            health2 = health2 - damage;

                            if (health2 > 0)
                            {
                                placeholder = whosTurn;
                                whosTurn = whoWaits;
                                whoWaits = placeholder;

                                await ReplyAsync(player1 + " has " + health1 + " health left and " + player1mana + " mana!\n" + player2 + " got " + health2 + " health left and " + player2mana + "mana!\n\n" + whosTurn + " your turn!");
                            }
                            else
                            {
                                await ReplyAsync(player2 + " died. " + player1 + " won!");
                                SwitchCaseString = "nofight";
                                health1 = 100;
                                health2 = 100;
                            }
                        }
                        else
                        {
                            await ReplyAsync("Sorry it seems like something went wrong. Please type !giveup");
                        }

                    }
                    else
                    {
                        await ReplyAsync(Context.User.Mention + " Sorry, you missed");

                        if (Context.User.Mention == player1)
                        {
                            if (player1mana > 0)
                            { 
                                player1mana -= 1;
                            }
                        }
                        else if (Context.User.Mention == player2)
                        {
                            if (player2mana >= 0)
                            { 
                                player2mana -= 1;
                            }
                        }

                        placeholder = whosTurn;
                        whosTurn = whoWaits;
                        whoWaits = placeholder;

                        await ReplyAsync(whosTurn + " It's your turn!");
                    }
                }
                else
                {
                    await ReplyAsync(Context.User.Mention + " it is not your turn!");
                }
            }
            else
            {
                await ReplyAsync("There is no fight at the moment");
            }
        }
    }

    public class ModCommands : ModuleBase
    {
        private CommandService _service;
        private DiscordSocketClient client;
        private IServiceProvider _provider;
        Random rnd = new Random();

        EmbedBuilder MyEmbedBuilder = new EmbedBuilder();

        public ModCommands(CommandService service)           /* Create a constructor for the commandservice dependency */
        {
            _service = service;
        }

        [Command("clear")]
        [Summary("Deletes the specified amount of messages in the channel.")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task ClearMessage([Remainder] int x = 0)
        {
            if (x <= 100)
            {
                var messagesToDelete = await Context.Channel.GetMessagesAsync(x + 1).Flatten();
                await Context.Channel.DeleteMessagesAsync(messagesToDelete);
                if (x == 1)
                {
                    await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted 1 message`");

                }
                else
                {
                    await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted {x} messages`");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Username}, you cannot delete more than 100 messages");
            }
        }

        [Command("ban")]
        [Summary("Ban a user.")]
        [RequireUserPermission(GuildPermission.BanMembers)] ///Needed User Permissions ///
        [RequireBotPermission(GuildPermission.BanMembers)] ///Needed Bot Permissions ///
        public async Task BanAsync(SocketGuildUser user = null, [Remainder] string reason = null)
        {
            if (user == null) throw new ArgumentException("You must mention a user");
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("You must provide a reason");

            var guild = Context.Guild as SocketGuild;
            var embed = new EmbedBuilder();
            embed.WithColor(new Color(0x00ff00));
            embed.Title = $"{user.Username} was banned";
            embed.Description = $"Username: {user.Username}\n Guild Name: {user.Guild.Name}\n Banned by: {Context.User.Mention}!\n Reason: {reason}";

            await guild.AddBanAsync(user);
            await Context.Channel.SendMessageAsync("", false, embed.Build());
            var dmChannel = await user.GetOrCreateDMChannelAsync();

            await dmChannel.SendMessageAsync("", false, embed.Build());
        }

        [Command("kick")]
        [Summary("Kick a user.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickAsync(SocketGuildUser user = null, [Remainder] string reason = null)
        {
            if (user == null) throw new ArgumentException("You must mention a user");
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("You must provide a reason");

            var guild = Context.Guild as SocketGuild;
            var embed = new EmbedBuilder();
            embed.WithColor(new Color(0x00ff00));
            embed.Title = $"**{user.Username}** has been kicked from {user.Guild.Name}";
            embed.Description = $"Username: {user.Username}\n Guild Name: {user.Guild.Name}\n Kicked by: {Context.User.Mention}!\n Reason: {reason}";

            await user.KickAsync(); ///kicks selected user///
            await Context.Channel.SendMessageAsync("", false, embed.Build()); ///sends embed///
            var dmChannel = await user.GetOrCreateDMChannelAsync();

            await dmChannel.SendMessageAsync("", false, embed.Build());
        }

        [Command("blacklist")]
        [Summary("Blacklist a user from the bot")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Blacklist(SocketGuildUser user)
        {
            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 255, 0)
            };

            var result = Database.CheckExistingUser(user);

            if (result.Count() <= 0)
            {
                Database.EnterUser(user);
            }

            var users = Database.GetUserStatus(user);

            var isblacklist = users.FirstOrDefault().blacklist;

            if(isblacklist == 1)
            {
                embed.Title = ("Blacklisting...");
                embed.Description = ("Sorry, user already blacklisted");
                return;
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                embed.Title = ("Blacklisting...");
                embed.Description = (user + " has been blacklisted from using the bot.");
                Database.blacklist(user);
                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("whitelist")]
        [Summary("Whitelist a user for the bot")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Whitelist(SocketGuildUser user)
        {
            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 255, 0)
            };

            var result = Database.CheckExistingUser(user);

            if (result.Count() <= 0)
            {
                Database.EnterUser(user);
            }

            var users = Database.GetUserStatus(user);

            var isblacklist = users.FirstOrDefault().blacklist;

            if (isblacklist == 0)
            {
                embed.Title = ("Whitelisting...");
                embed.Description = ("Sorry, user isn't blacklisted");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }
            else
            {
                embed.Title = ("Whitelisting...");
                embed.Description = (user + " has been whitelisted from using the bot.");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                Database.whitelist(user);
            }
        }

        [Command("tgjoin")]
        [Summary("Toggle the join server message")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Tgjoin()
        {
            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 255, 0)
            };

            var guild = Context.Guild;
            var servers = Database.GetServerStatus(guild);
            var currenttg = servers.FirstOrDefault().joinnot;

            var result = Database.CheckExistingServer(guild);

            if (result.Count() <= 0)
            {
                Database.EnterServer(guild);
            }

            if (currenttg == 0)
            {
                Database.tgjoin(guild);
                embed.Title = ("Toggling join message: ");
                embed.Description = ("Disabled joining message");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                Database.tgjoin(guild);
                embed.Title = ("Toggling join message: ");
                embed.Description = ("Enabled joining message");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }

        [Command("tgleave")]
        [Summary("Toggle the leave server message")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Tgjleave()
        {
            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 255, 0)
            };

            var guild = (Context.Guild as SocketGuild);
            var servers = Database.GetServerStatus(guild);
            var currenttg = servers.FirstOrDefault().leavenot;

            var result = Database.CheckExistingServer(guild);

            if (result.Count() <= 0)
            {
                Database.EnterServer(guild);
            }

            if (currenttg == 0)
            {
                Database.tgleave(guild);
                embed.Title = ("Toggling leave message: ");
                embed.Description = ("Disabled leaving message");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                Database.tgleave(guild);
                embed.Title = ("Toggling leave message: ");
                embed.Description = ("Enabled leaving message");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }

        [Command("tglevelmsg")]
        [Summary("Toggle the level up server message")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Tglevel()
        {
            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 255, 0)
            };

            var guild = (Context.Guild as SocketGuild);
            var servers = Database.GetServerStatus(guild);
            var currenttg = servers.FirstOrDefault().levelupnot;

            var result = Database.CheckExistingServer(guild);

            if (result.Count() <= 0)
            {
                Database.EnterServer(guild);
            }

            if (currenttg == 0)
            {
                Database.tglevel(guild);
                embed.Title = ("Toggling level up message: ");
                embed.Description = ("Disabled level up message");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                Database.tglevel(guild);
                embed.Title = ("Toggling join message: ");
                embed.Description = ("Enabled level up message");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }
    }
}
