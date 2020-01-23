using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Replica.Core.Controllers;
using Replica.Core.Entity;
using Replica.Core.Utils;
using Attachment = Replica.Core.Entity.Attachments.Attachment;
using DAttachment = Discord.Attachment;
using Replica.Core.Entity.Attachments;

namespace Replica.Controllers.Discord
{
    [Controller("dc")]
    public class DiscordController : ControllerBase<DiscordOptions>
    {
        private DiscordSocketClient _client;
        private readonly AttachmentFactory _factory = new AttachmentFactory("dc");

        public override Task DeleteMessage(long chatId, int id)
        {
            throw new NotImplementedException();
        }

        public override Task EditMessage(long chatId, long id, OutMessage message)
        {
            throw new NotImplementedException();
        }

        private Task Log(LogMessage msg)
        {
            var messageTemplate = "[DC] " + msg.Message;

            if (msg.Exception != null)
            {
                Serilog.Log.Error(msg.Exception, messageTemplate);
                return Task.CompletedTask;
            }

            Serilog.Log.Debug(messageTemplate);
            return Task.CompletedTask;
        }

        private ChatInfo CreateChat(ISocketMessageChannel channel)
            => new ChatInfo((long)channel.Id, channel.Name, 0);

        private UserInfo CreateUser(SocketUser user)
            => new UserInfo
            {
                Id = (long)user.Id,
                Username = user.Username,
                FirstName = user.Username,
                IsBot = user.IsBot
            };

        private Attachment CreateAttachment(DAttachment attachment)
        {
            var ext = Path.GetExtension(attachment.Filename);
            switch (ext)
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                    return _factory.CreatePhoto(new PhotoSize[] { _factory.CreatePhotoSize(
                        attachment.Id.ToString(),
                        attachment.Url,
                        attachment.Width.Value,
                        attachment.Height.Value,
                        attachment.Size)});
                default: return null;
            }
        }

        private InMessage CreateMessage(SocketMessage message)
        {
            return new InMessage
            {
                Id = (int)message.Id,
                Text = message.Content,
                Date = message.Timestamp.DateTime,
                Sender = CreateUser(message.Author),
                Attachments = message.Attachments.Select(CreateAttachment)
                    .Where(x => x != null)
                    .ToArray(),
                Forwarded = new InMessage[0],
                Chat = CreateChat(message.Channel)
            };
        }

        private Task MessageReceived(SocketMessage message)
        {
            if (message.Author.IsBot) return Task.CompletedTask;
            TakeOver(CreateMessage(message));
            return Task.CompletedTask;
        }

        public override async void Init()
        {
            _client = new DiscordSocketClient();
            _client.Log += Log;
            _client.MessageReceived += MessageReceived;

            await _client.LoginAsync(TokenType.Bot, Options.Token);
        }

        public override Task<string> ResolveSource(Attachment attachment)
        {
            throw new NotImplementedException();
        }

        public override async Task<IEnumerable<long>> SendMessage(long chatId, OutMessage message)
        {
            var ids = new List<long>();
            var channel = _client.GetChannel((ulong)chatId) as IMessageChannel;

            IUserMessage result;
            if (message.AuthorName != null)
            {
                var photos = message.Attachments.OfType<Photo>();
                var embed = new EmbedBuilder()
                    .WithAuthor(message.AuthorName, message.AuthorIcon, message.AuthorUrl)
                    .WithDescription(message.Text)
                    .WithFooter(message.Footer);
                IController controller = null;
                if (photos.Count() > 0)
                {
                    controller = ResolveController(photos.First().Controller);
                    var source = await controller.ResolveSource(photos.First());
                    embed.WithImageUrl(source);
                    photos = photos.Skip(1);
                }
                result = await channel.SendMessageAsync(embed: embed.Build());
                ids.Add((long)result.Id);

                foreach (var photo in photos)
                {
                    var source = await controller.ResolveSource(photos.First());
                    embed = new EmbedBuilder().WithImageUrl(source);
                    result = await channel.SendMessageAsync(embed: embed.Build());
                    ids.Add((long)result.Id);
                }

                return ids;
            }

            result = await channel.SendMessageAsync(
               message.Flags.HasFlag(MessageFlags.Code)
               ? $"```{message.Text}```"
               : message.Text);
            ids.Add((long)result.Id);
            return ids;
        }

        public override void Start() => _client.StartAsync();
        public override void Stop() => _client.StopAsync();

        protected override void DisposeWebhook()
        {
            throw new NotImplementedException();
        }

        protected override Task<ChatInfo> FetchChatInfo(long chatId)
        {
            var channel = _client.GetChannel((ulong)chatId) as IMessageChannel;
            if (channel == null) return Task.FromResult(new ChatInfo(chatId, "Not found", 0));
            return Task.FromResult(new ChatInfo(chatId, channel.Name, 0));
        }

        protected override Task<UserInfo> FetchUserInfo(long userId)
        {
            throw new NotImplementedException();
        }

        protected override void SetupWebhook(SimpleHttpServer server, string endpoint, string path, string secret)
        {
            throw new NotImplementedException();
        }
    }
}
