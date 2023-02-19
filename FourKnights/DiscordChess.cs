using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChessCat;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace FourKnights
{
    public class DiscordChess
    {
        public static Dictionary<ISocketMessageChannel, ChessCat.Game> gameByChannel = new();
        public static Dictionary<ChessCat.Game, RestUserMessage> messageByGame = new();

        public static Dictionary<string, string> chessEmojiByName = new()
        {
            {"brilliant", (Program.program.GetEmoteStringByName("brilliant")).ToString()},
            {"blunder", (Program.program.GetEmoteStringByName("blunder")).ToString()},
            {"wkw", (Program.program.GetEmoteStringByName("wkw")).ToString()},
            {"wkg", (Program.program.GetEmoteStringByName("wkg")).ToString()},
            {"wqw", (Program.program.GetEmoteStringByName("wqw")).ToString()},
            {"wqg", (Program.program.GetEmoteStringByName("wqg")).ToString()},
            {"wrw", (Program.program.GetEmoteStringByName("wrw")).ToString()},
            {"wrg", (Program.program.GetEmoteStringByName("wrg")).ToString()},
            {"wnw", (Program.program.GetEmoteStringByName("wnw")).ToString()},
            {"wng", (Program.program.GetEmoteStringByName("wng")).ToString()},
            {"wbw", (Program.program.GetEmoteStringByName("wbw")).ToString()},
            {"wbg", (Program.program.GetEmoteStringByName("wbg")).ToString()},
            {"wpw", (Program.program.GetEmoteStringByName("wpw")).ToString()},
            {"wpg", (Program.program.GetEmoteStringByName("wpg")).ToString()},
            {"bkw", (Program.program.GetEmoteStringByName("bkw")).ToString()},
            {"bkg", (Program.program.GetEmoteStringByName("bkg")).ToString()},
            {"bqw", (Program.program.GetEmoteStringByName("bqw")).ToString()},
            {"bqg", (Program.program.GetEmoteStringByName("bqg")).ToString()},
            {"brw", (Program.program.GetEmoteStringByName("brw")).ToString()},
            {"brg", (Program.program.GetEmoteStringByName("brg")).ToString()},
            {"bnw", (Program.program.GetEmoteStringByName("bnw")).ToString()},
            {"bng", (Program.program.GetEmoteStringByName("bng")).ToString()},
            {"bbw", (Program.program.GetEmoteStringByName("bbw")).ToString()},
            {"bbg", (Program.program.GetEmoteStringByName("bbg")).ToString()},
            {"bpw", (Program.program.GetEmoteStringByName("bpw")).ToString()},
            {"bpg", (Program.program.GetEmoteStringByName("bpg")).ToString()},
            {"esw", (Program.program.GetEmoteStringByName("esw")).ToString()},
            {"esg", (Program.program.GetEmoteStringByName("esg")).ToString()},
        };

        public static Dictionary<Piece, string> emojiNameByPiece = new()
        {
            {Piece.WKing, "wk" },
            {Piece.WQueen, "wq" },
            {Piece.WRook, "wr" },
            {Piece.WBishop, "wb" },
            {Piece.WKnight, "wn" },
            {Piece.WPawn, "wp" },
            {Piece.FakeWPawn, "es" },
            {Piece.BKing, "bk" },
            {Piece.BQueen, "bq" },
            {Piece.BRook, "br" },
            {Piece.BBishop, "bb" },
            {Piece.BKnight, "bn" },
            {Piece.BPawn, "bp" },
            {Piece.FakeBPawn, "es" },
            {default, "es" }
        };

        public static async Task<string> GetGameAsEmoji(ChessCat.Game game)
        {
            string making = "";

            bool even = true;
            byte counter = 0;
            for (short y = 7; y >= 0; y--)
            {
                for (short x = 0; x <= 7; x++)
                {
                    Piece p = game.GetPiece((byte)x, (byte)y);
                    making += chessEmojiByName[emojiNameByPiece[p] + (even ? 'g' : 'w')];
                    even = !even;
                    counter++;
                    if (counter == 8)
                    {
                        making += '\n';
                        counter = 0;
                        even = !even;
                    }
                }
            }

            making += '\n';
            making += (game.ToPlay == ChessColor.White) ? "White to play" : "Black to play";
            return making;
        }

        public static async Task StartGame(SocketSlashCommand command)
        {
            ISocketMessageChannel channel = command.Channel;
            if (gameByChannel.ContainsKey(channel))
            {
                await command.RespondAsync("This channel already has an ongoing game!", ephemeral: true);
                return;
            }
            ChessCat.Game game;
            game = new();
            gameByChannel[channel] = game;
            await command.RespondAsync("Starting game...");
            messageByGame[game] = await channel.SendMessageAsync(await GetGameAsEmoji(game));
        }

        public static async Task ResendBoard(SocketSlashCommand command)
        {
            ISocketMessageChannel channel = command.Channel;
            if (!gameByChannel.ContainsKey(channel))
            {
                await command.RespondAsync("No game in this channel", ephemeral: true);
                return;
            }
            ChessCat.Game game = gameByChannel[channel];
            messageByGame[game] = await channel.SendMessageAsync(await GetGameAsEmoji(game));
            await command.RespondAsync("Board resent", ephemeral: true);
        }

        public static async Task CancelGame(SocketSlashCommand command)
        {
            ISocketMessageChannel channel = command.Channel;
            if (!gameByChannel.ContainsKey(channel))
            {
                await command.RespondAsync("No game in this channel", ephemeral: true);
                return;
            }
            ChessCat.Game game = gameByChannel[channel];
            await messageByGame[game].ModifyAsync(async message =>
            {
                message.Content = await GetGameAsEmoji(game) + "\nGame Cancelled!";
            });
            messageByGame.Remove(game);
            gameByChannel.Remove(channel);
            await command.RespondAsync("Game cancelled");
        }

        public static async Task Move(SocketSlashCommand command)
        {
            string? move = command.Data.Options.First().Value.ToString();
            if (move == null) return;

            ISocketMessageChannel channel = command.Channel;
            if (!gameByChannel.ContainsKey(channel))
            {
                await command.RespondAsync("No game in this channel! Start one with /startgame", ephemeral: true);
                return;
            }
            ChessCat.Game game = gameByChannel[channel];

            (MoveResult, string) result = game.interpretMove(move);

            await messageByGame[game].ModifyAsync(async message =>
            {
                message.Content = await GetGameAsEmoji(game);
            });

            await command.RespondAsync(result.Item2, ephemeral: true);
            await Task.Delay(1000);
            await command.DeleteOriginalResponseAsync();

            return;
        }
    }
}
