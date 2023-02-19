using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace FourKnights
{
    public class Program
    {
        public static Program program;

        private DiscordSocketClient _client;
        public static async Task Main(string[] args) {
            program = new Program();
            await program.MainAsync(args);
        }

        public async Task MainAsync(string[] args)
        {
            _client = new DiscordSocketClient();

            _client.Log += Log;

            if (args.Length == 0) return;

            string token = args[0];

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            _client.Ready += Client_Ready;
            _client.SlashCommandExecuted += Handle_Command;

            await Task.Delay(-1);
        }

        private async Task Handle_Command(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case ("echo"):
                    {
                        await command.RespondAsync(command.Data.Options.First().Value.ToString());
                        return;
                    }
                case ("startgame"):
                    {
                        await DiscordChess.StartGame(command);
                        return;
                    }
                case ("cancelgame"):
                    {
                        await DiscordChess.CancelGame(command);
                        return;
                    }
                case ("move"):
                    {
                        await DiscordChess.Move(command);
                        return;
                    }
                case ("resendboard"):
                    {
                        await DiscordChess.ResendBoard(command);
                        return;
                    }
            }
        }

        public Emote GetEmoteStringByName(string name)
        {
            return _client.GetGuild(1075923960031617084)
                    .Emotes
                    .FirstOrDefault(x => x.Name.IndexOf(
                        name, StringComparison.OrdinalIgnoreCase) != -1);
        }

        public async Task Create_Commands()
        {
            SocketGuild guild = _client.GetGuild(1075923960031617084);

            SlashCommandBuilder guildCommand = new();
            guildCommand
                .WithName("echo")
                .WithDescription("Repeat a message back")
                .AddOption("message", ApplicationCommandOptionType.String, "The message to repeat", isRequired: true);

            await guild.CreateApplicationCommandAsync(guildCommand.Build());

            SlashCommandBuilder guildCommand2 = new();
            guildCommand2
                .WithName("startgame")
                .WithDescription("Starts a chess game in the channel");

            await guild.CreateApplicationCommandAsync(guildCommand2.Build());

            SlashCommandBuilder guildCommand3 = new();
            guildCommand3
                .WithName("move")
                .WithDescription("Play a move in the current game")
                .AddOption("move", ApplicationCommandOptionType.String, "The move in algebraic notation", isRequired: true);

            await guild.CreateApplicationCommandAsync(guildCommand3.Build());

            SlashCommandBuilder guildCommand4 = new();
            guildCommand4
                .WithName("cancelgame")
                .WithDescription("Ends the chess game in the channel");

            await guild.CreateApplicationCommandAsync(guildCommand4.Build());

            SlashCommandBuilder guildCommand5 = new();
            guildCommand4
                .WithName("resendboard")
                .WithDescription("Resends the board for the game in the current channel");

            await guild.CreateApplicationCommandAsync(guildCommand4.Build());
        }

        public async Task Client_Ready()
        {
            await Create_Commands();
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}