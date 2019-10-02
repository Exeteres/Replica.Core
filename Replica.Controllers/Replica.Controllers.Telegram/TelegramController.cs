using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Replica.Core.Controllers;
using Replica.Core.Entity;
using Replica.Core.Entity.Attachments;
using Replica.Core.Exceptions;
using Replica.Core.Extensions;
using Replica.Core.Utils;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using Document = Replica.Core.Entity.Attachments.Document;
using Sticker = Replica.Core.Entity.Attachments.Sticker;
using Voice = Replica.Core.Entity.Attachments.Voice;

namespace Replica.Controllers.Telegram
{
    [Controller("tg")]
    public class TelegramController : ControllerBase<TelegramOptions>
    {
        private TelegramBotClient _bot;
        private readonly AttachmentFactory _factory = new AttachmentFactory("tg");

        private InlineKeyboardMarkup RenderButtons(OutMessage message)
        {
            var buttons = new List<InlineKeyboardButton>();
            foreach (var btn in message.Buttons)
            {
                var button = new InlineKeyboardButton();
                button.Text = btn.Text;

                button.CallbackData = RegisterHandler(btn.Handler);

                buttons.Add(button);
            }

            return message.Buttons.Count() > 3 ?
                new InlineKeyboardMarkup(buttons.Select(x => new InlineKeyboardButton[] { x })) :
                new InlineKeyboardMarkup(buttons);
        }

        public override async Task<string> ResolveSource(Attachment attachment)
        {
            return $"https://api.telegram.org/file/bot{Options.Token}/{(await _bot.GetFileAsync(attachment.FileId)).FilePath}";
        }

        public override async Task<IEnumerable<long>> SendMessage(long chatId, OutMessage message)
        {
            var ids = new List<long>();
            if (message.Buttons != null)
            {
                var result = await _bot.SendTextMessageAsync(chatId, message.Text, replyMarkup: RenderButtons(message), disableWebPagePreview: true);
                ids.Add(result.MessageId);
                return ids;
            }
            var iscode = message.Flags.HasFlag(MessageFlags.Code);
            var res = await _bot.SendTextMessageAsync(chatId, iscode
                ? $"<code>{message.Text.EscapeHTML()}</code>"
                : message.Text, iscode || message.Flags.HasFlag(MessageFlags.AllowHTMl)
                    ? ParseMode.Html
                    : ParseMode.Default, true);
            ids.Add(res.MessageId);
            if (message.Attachments == null)
                return ids;
            foreach (var attachment in message.Attachments)
            {
                if (attachment is WebPage page)
                {
                    res = await _bot.SendTextMessageAsync(chatId, page.Source);
                    ids.Add(res.MessageId);
                    continue;
                }
                res = null;
                var id = GetContentId(attachment);
                var src = id ?? attachment.Source ?? await ResolveController(attachment.Controller).ResolveSource(attachment);
                switch (attachment)
                {
                    case Photo photo:

                        res = await _bot.SendPhotoAsync(chatId, src);
                        if (id == null)
                            SetContentId(attachment, res.Photo.Last().FileId);
                        break;
                    // TODO разобраться с PhotoSize
                    case Sticker sticker:
                        res = await _bot.SendPhotoAsync(chatId, src);
                        if (id == null)
                            SetContentId(attachment, res.Photo.Last().FileId); // лол
                        break;
                    // case Document doc:
                    //     res = await _bot.SendDocumentAsync(chatId, src);
                    //     if (id == null)
                    //         SetContentId(attachment, res.Document.FileId);
                    //     break;
                    case Voice voice:
                        res = await _bot.SendVoiceAsync(chatId, src);
                        if (id == null)
                            SetContentId(attachment, res.Voice.FileId);
                        break;
                }
                if (res != null)
                    ids.Add(res.MessageId);
            }
            return ids;
        }

        public override void Start()
        {
            _bot.StartReceiving();
        }

        public override void Stop()
        {
            _bot.StopReceiving();
        }

        private Attachment ExtractAttachment(Message message)
        {
            switch (message.Type)
            {
                case MessageType.Photo:
                    return _factory.CreatePhoto(message.Photo
                        .Select(x => _factory.CreatePhotoSize(x.FileId, null, x.Width, x.Height, x.FileSize))
                        .ToArray());
                case MessageType.Document:
                    return _factory.CreateDocument(
                        message.Document.FileId,
                        null,
                        message.Document.FileName,
                        message.Document.FileSize);
                case MessageType.Sticker:
                    return _factory.CreateSticker(
                        message.Sticker.FileId,
                        null,
                        message.Sticker.FileSize,
                        message.Sticker.Height,
                        message.Sticker.Width,
                        message.Sticker.Emoji
                    );
                case MessageType.Voice:
                    return _factory.CreateVoice(
                        message.Voice.FileId,
                        null,
                        message.Voice.Duration,
                        message.Voice.FileSize
                    );
            }
            return null;
        }

        private ChatInfo CreateChat(Chat chat)
            => new ChatInfo(chat.Id, chat.Title ?? chat.Username, 0);

        private InMessage CreateMessage(Message message)
        {
            if (message == null)
                return null;
            var attachment = ExtractAttachment(message);
            if (message.ForwardFrom == null && message.ForwardFromChat == null)
            {
                return new InMessage
                {
                    Id = message.MessageId,
                    Text = message.Text ?? message.Caption,
                    Chat = CreateChat(message.Chat),
                    Sender = CreateUser(message.From),
                    Reply = CreateMessage(message.ReplyToMessage),
                    Attachments = attachment != null ?
                        new[] { attachment } :
                        new Attachment[0],
                    Date = message.Date,
                    Forwarded = new InMessage[0]
                };
            }
            else
            {
                return new InMessage
                {
                    Id = message.MessageId,
                    Chat = CreateChat(message.Chat),
                    Sender = CreateUser(message.From),
                    Reply = CreateMessage(message.ReplyToMessage),
                    Attachments = new Attachment[0],
                    Date = message.Date,
                    Forwarded = new InMessage[]
                    {
                        new InMessage
                        {
                            Id = message.ForwardFromMessageId,
                            Text = message.Text ?? message.Caption,
                            Attachments = attachment != null ?
                                new[] { attachment } :
                                new Attachment[0],
                            Sender = message.ForwardFrom != null
                                ? CreateUser(message.ForwardFrom)
                                : CreateUser(message.ForwardFromChat),
                            Chat = CreateChat(message.ForwardFromChat ?? message.Chat),
                            Date = message.ForwardDate.Value,
                            Forwarded = new InMessage[0]
                        }
                    }
                };
            }
        }

        private UserInfo CreateUser(User user)
        {
            return new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Language = user.LanguageCode,
                IsBot = user.IsBot
            };
        }

        private UserInfo CreateUser(Chat user)
        {
            return new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Title = user.Title
            };
        }

        public override Task EditMessage(long chatId, long id, OutMessage message)
        {
            var ids = new List<long>();
            if (message.Buttons != null)
            {
                return _bot.EditMessageTextAsync(chatId, (int)id, message.Text, replyMarkup: RenderButtons(message));
            }

            return _bot.EditMessageTextAsync(chatId, (int)id, message.Text, ParseMode.Html);

            //var attachment = message.Attachments[0];
            //return _bot.EditMessageCaptionAsync(chatId, (int)id, message.Text, RenderButtons(message));
        }

        public override void Init()
        {
            _bot = new TelegramBotClient(Options.Token);
            _bot.OnMessage += (e, args) =>
            {
                if (args.Message.Text == "/test" || args.Message.Caption == "/test")
                {
                    _ = SendMessage(args.Message.Chat.Id, OutMessage.FromCode(
                        JsonConvert.SerializeObject(args.Message, Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })));
                    return;
                }

                TakeOver(CreateMessage(args.Message));
            };
            _bot.OnCallbackQuery += (e, query)
                => TakeOverButtons(new Core.Entity.InlineQuery
                {
                    User = CreateUser(query.CallbackQuery.From),
                    ChatId = query.CallbackQuery.Message.Chat.Id
                }, query.CallbackQuery.Data);
        }

        public override Task DeleteMessage(long chatId, int id)
        {
            return _bot.DeleteMessageAsync(chatId, id);
        }

        protected override async Task<UserInfo> FetchUserInfo(long userId)
        {
            var user = await _bot.GetChatAsync(userId);
            return new UserInfo
            {
                Id = userId,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Title = user.Title
            };
        }

        protected override async Task<ChatInfo> FetchChatInfo(long chatId)
        {
            var chat = await _bot.GetChatAsync(chatId);
            try
            {
                var admins = await _bot.GetChatAdministratorsAsync(chatId);
                var owner = admins.FirstOrDefault(x => x.Status == ChatMemberStatus.Creator);
                return new ChatInfo(chat.Id, chat.Title ?? chat.Username, owner != null ? owner.User.Id : 0);
            }
            catch (ApiRequestException)
            {
                return new ChatInfo(chat.Id, chat.Title ?? chat.Username, 0);
            }
        }

        protected override void SetupWebhook(SimpleHttpServer server, string endpoint, string path, string secret)
        {
            _bot.SetWebhookAsync(endpoint + "/" + secret);

            server.AddHandler(path + "/" + secret, json =>
            {
                try
                {
                    var update = JsonConvert.DeserializeObject<Update>(json);
                    if (update.Message == null) return "ok";
                    Task.Run(() => TakeOver(CreateMessage(update.Message)));
                    return "ok";
                }
                catch { return "fuck"; }
            });
        }

        protected override void DisposeWebhook()
        {
            _bot.DeleteWebhookAsync();
        }
    }
}
