using System.Text.RegularExpressions;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Replica.Core.Controllers;
using Replica.Core.Entity;
using Replica.Core.Exceptions;
using Replica.Core.Extensions;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;
using Replica.Core.Entity.Attachments;
using System.Net;
using System.Text;
using VkNet.Model.Attachments;
using Photo = Replica.Core.Entity.Attachments.Photo;
using Attachment = Replica.Core.Entity.Attachments.Attachment;
using System.Net.Http;
using Sticker = Replica.Core.Entity.Attachments.Sticker;
using VkNet.Enums.SafetyEnums;
using Document = Replica.Core.Entity.Attachments.Document;
using Replica.Core.Utils;
using VkNet.Utils;
using Replica.Controllers.VK.Utils;

namespace Replica.Controllers.VK
{
    [Controller("vk")]
    public class VkController : ControllerBase<VkOptions>
    {
        private VkApi _api;

        private CancellationTokenSource _cts;

        private readonly AttachmentFactory _factory = new AttachmentFactory("vk");

        public override void Init()
        {
            _api = new VkApi(new SerilogLogger());
            _api.Authorize(new ApiAuthParams { AccessToken = Options.Token });
        }

        public override Task DeleteMessage(long chatId, int id)
        {
            throw new NotImplementedException();
        }

        private class GenericAttachment : MediaAttachment
        {
            public GenericAttachment(string type, long user, long id, string token)
            {
                Alias = type;
                OwnerId = user;
                Id = id;
                AccessKey = token;
            }

            protected override string Alias { get; }
        }

        private static MediaAttachment ParseAttachment(string attachment)
        {
            var match = Regex.Match(attachment, @"(\D*)(\d*)_(.*)_(.*)");
            return new GenericAttachment(match.Groups[1].Value,
                long.Parse(match.Groups[2].Value),
                long.Parse(match.Groups[3].Value),
                match.Groups[4].Value);
        }

        public override async Task<IEnumerable<long>> SendMessage(long chatId, OutMessage message)
        {
            var ids = new List<long>();
            var attachments = new List<MediaAttachment>();
            if (message.Attachments != null)
                foreach (var attachment in message.Attachments)
                {
                    var id = GetContentId(attachment);
                    if (attachment is Sticker sticker && sticker.Controller == "vk")
                    {
                        ids.Add(await _api.Messages.SendAsync(new MessagesSendParams
                        {
                            PeerId = chatId,
                            Message = message.Text,
                            RandomId = (int)DateTime.Now.Ticks
                        }));
                        ids.Add(await _api.Messages.SendAsync(new MessagesSendParams
                        {
                            PeerId = chatId,
                            StickerId = uint.Parse(GetContentId(attachment).Replace("sticker_", "")),
                            RandomId = (int)DateTime.Now.Ticks
                        }));
                        return ids;
                    }
                    var src = attachment.Source ?? await ResolveController(attachment.Controller).ResolveSource(attachment);
                    switch (attachment)
                    {
                        case Photo _:
                        case Sticker _:
                            var a = id != null
                                ? ParseAttachment(id)
                                : await UploadPhoto(chatId, src);
                            if (id == null)
                                SetContentId(attachment, a.ToString());
                            attachments.Add(a);
                            break;
                        case Voice _:
                            a = id != null
                                ? ParseAttachment(id)
                                : await UploadVoice(chatId, src);
                            if (id == null)
                                SetContentId(attachment, a.ToString());
                            attachments.Add(a);
                            break;
                        case Document doc:
                            a = id != null
                                ? ParseAttachment(id)
                                : await UploadDocument(chatId, src, doc.FileName);
                            if (id == null)
                                SetContentId(attachment, a.ToString());
                            attachments.Add(a);
                            break;
                    }
                }
            ids.Add(await _api.Messages.SendAsync(new MessagesSendParams
            {
                PeerId = chatId,
                Message = message.Text,
                Attachments = attachments,
                RandomId = (int)DateTime.Now.Ticks
            }));
            return ids;
        }

        protected override async Task<UserInfo> FetchUserInfo(long id)
        {
            if (id > 0)
            {
                var users = await _api.Users.GetAsync(new long[] { id }, ProfileFields.ScreenName | ProfileFields.Language | ProfileFields.Photo100);
                if (users.Count == 0)
                    throw new NotFoundException("User not found");
                var user = users[0];
                return new UserInfo
                {
                    Id = id,
                    Username = user.ScreenName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Language = user.Language.ToString(),
                    AvatarUrl = user.Photo100.ToString(),
                    IsBot = false
                };
            }
            var groups = await _api.Groups.GetByIdAsync(new string[0], id.ToString(), GroupsFields.Status);
            if (groups.Count == 0)
                throw new NotFoundException("Group not found");
            var group = groups[0];
            return new UserInfo
            {
                Id = id,
                Username = group.ScreenName,
                FirstName = group.Name,
                IsBot = true
            };
        }

        protected async override Task<ChatInfo> FetchChatInfo(long chatId)
        {
            /* fix */
            if (chatId < 2000000000)// fix
            /* fix */
            {// fix
                /* fix */
                var user = await GetUserInfo(chatId);// fix
                /* fix */
                return new ChatInfo(chatId, user.Username, user.Id);// fix
            }// fix/* fix *//* fix */
            /* fix */
            try// fix/* fix */
            /* fix *//* fix *//* fix */
            {// fix/* fix */
                /* fix *//* fix *//* fix */
                /* fix *//* fix */
                var chats = await _api.Messages.GetConversationsByIdAsync(new long[] { chatId }, new string[0]); // TODO fix
                if (chats.Items.Count() == 0) // fix
                    /* fix */                                  /* fix */            /* fix */    /* fix */
                    return new ChatInfo(chatId, "Unable to get name", 0); //fix
                /* fix */               /* /* fi                 /* fix */                                        /* fix */
                var chat = chats.Items.First().ChatSettings; //  fix
                /* fix */
                return new ChatInfo(chatId, chat.Title, chat.OwnerId); // fix/* fix */
            } // fix
            catch { return new ChatInfo(chatId, "Error while getting ChatInfo", 0); } // fix
        }

        private async Task<InMessage> CreateMessage(Message message)
        {
            if (message == null)
                return null;
            var attachments = new List<Attachment>();
            foreach (var attachment in message.Attachments)
            {
                switch (attachment.Instance)
                {
                    case VkNet.Model.Attachments.Photo photo:
                        attachments.Add(_factory.CreatePhoto(photo.Sizes
                            .Select(x => _factory.CreatePhotoSize(photo.ToString(), x.Url.ToString(), (int)x.Width, (int)x.Height, 0))
                            .ToArray()));
                        break;
                    case VkNet.Model.Attachments.Sticker sticker:
                        var size = sticker.Images.ElementAt(sticker.Images.Count() - 3); //лол
                        attachments.Add(_factory.CreateSticker(sticker.ToString(), size.Url.ToString(), size.Width, size.Height, 0, "-"));
                        break;
                    case AudioMessage audio:
                        attachments.Add(_factory.CreateVoice(audio.ToString(), audio.LinkOgg.ToString(), (int)audio.Duration, 0));
                        break;
                    case VkNet.Model.Attachments.Document doc:
                        attachments.Add(_factory.CreateDocument(doc.ToString(), doc.Uri, doc.Title, (int)doc.Size));
                        break;
                    case Wall post:
                        attachments.Add(_factory.CreateWebPage(post.ToString(), "https://vk.com/" + post));
                        break;
                }
            }
            return new InMessage
            {
                Text = message.Text,
                Chat = await FetchChatInfo(message.ChatId ?? message.PeerId.Value),
                Sender = await GetUserInfo(message.FromId.Value),
                Attachments = attachments.ToArray(),
                Forwarded = (await message.ForwardedMessages
                    .SelectAsync(async x => await CreateMessage(x)))
                    .ToArray(),
                Reply = await CreateMessage(message.ReplyMessage),
                Date = message.Date.Value
            };
        }

        private async Task<string> ReplicateFileAsync(string source, string dest, string type, string filename = null)
        {
            using (var wc = new WebClient())
            using (var hc = new HttpClient())
            using (var form = new MultipartFormDataContent())
            {
                var data = wc.DownloadData(source);
                form.Add(new ByteArrayContent(data), type, filename == null ? type == "photo" ? "replica.jpg" : "replica" : filename);
                var resp = await hc.PostAsync(dest, form);
                resp.EnsureSuccessStatusCode();
                return Encoding.ASCII.GetString(await resp.Content.ReadAsByteArrayAsync());
            }
        }

        private async Task<MediaAttachment> UploadPhoto(long chatId, string url)
        {
            var server = await _api.Photo.GetMessagesUploadServerAsync(chatId);
            var file = await ReplicateFileAsync(url, server.UploadUrl, "photo");
            return (await _api.Photo.SaveMessagesPhotoAsync(file))[0];
        }

        private async Task<MediaAttachment> UploadVoice(long chatId, string url)
        {
            var server = await _api.Docs.GetMessagesUploadServerAsync(chatId, DocMessageType.AudioMessage);
            var file = await ReplicateFileAsync(url, server.UploadUrl, "file");
#pragma warning disable
            return (await _api.Docs.SaveAsync(file, "replica"))[0].Instance;
#pragma warning restore
        }

        private async Task<MediaAttachment> UploadDocument(long chatId, string url, string filename)
        {
            var server = await _api.Docs.GetMessagesUploadServerAsync(chatId, DocMessageType.Doc);
            switch (filename.Split('.').Last())
            {
                case "apk":
                case "zip":
                case "rar":
                case "exe":
                    filename += "1";
                    break;
            }
            var file = await ReplicateFileAsync(url, server.UploadUrl, "file", filename);
#pragma warning disable
            return (await _api.Docs.SaveAsync(file, filename))[0].Instance;
#pragma warning restore
        }

        public override void Start()
        {
            // TODO fix
            // FlurlHttpException
            _cts = new CancellationTokenSource();
            Task.Run(async () =>
                {
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            var s = _api.Groups.GetLongPollServer(Options.PollingGroup);
                            var response = _api.Groups.GetBotsLongPollHistory(new BotsLongPollHistoryParams()
                            {
                                Server = s.Server,
                                Ts = s.Ts,
                                Key = s.Key,
                                Wait = 25
                            });
                            s.Ts = response.Ts;
                            if (response.Updates == null)
                                continue;

                            foreach (var update in response.Updates)
                            {
                                if (update.Message == null || update.Message.FromId < 0)
                                    continue;
                                if (update.Message.Text == "/test")
                                {
                                    await SendMessage(update.Message.PeerId.Value,
                                        OutMessage.FromText(JsonConvert.SerializeObject(update.Message, Newtonsoft.Json.Formatting.None,
                                            new JsonSerializerSettings
                                            {
                                                NullValueHandling = NullValueHandling.Ignore
                                            })));
                                    continue;
                                }
                                TakeOver(await CreateMessage(update.Message));
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }).ContinueWith(x => throw x.Exception, TaskContinuationOptions.OnlyOnFaulted);
        }

        public override void Stop()
        {
            _cts?.Cancel();
        }

        public override Task EditMessage(long chatId, long id, OutMessage message)
        {
            throw new NotImplementedException();
        }

        public override Task<string> ResolveSource(Attachment attachment)
        {
            return Task.FromResult(attachment.Source);
        }

        private long _serverId;

        protected override void SetupWebhook(SimpleHttpServer server, string endpoint, string path, string secret)
        {
            string confirmation = null;
            server.AddHandler(path, json =>
            {
                var update = JsonConvert.DeserializeObject<CallbackUpdate>(json);
                if (update.Secret != secret)
                    return "fuck";
                switch (update.Type)
                {
                    case "confirmation":
                        return confirmation;
                    case "message_new":
                        var msg = Message.FromJson(new VkResponse(update.Object));
                        Task.Run(async () => TakeOver(await CreateMessage(msg)));
                        return "ok";
                }
                return "ok";
            });
            server.OnStart += async () =>
            {
                confirmation = await _api.Groups.GetCallbackConfirmationCodeAsync(Options.PollingGroup);
                var servers = await _api.Groups.GetCallbackServersAsync(Options.PollingGroup);
                var serverId = servers.FirstOrDefault(x => x.Url == endpoint)?.Id;

                if (serverId.HasValue)
                    await _api.Groups.EditCallbackServerAsync(Options.PollingGroup, (ulong)serverId.Value, endpoint, "Replica", secret);
                else
                    serverId = await _api.Groups.AddCallbackServerAsync(Options.PollingGroup, endpoint, "Replica", secret);
                _serverId = serverId.Value;
                await _api.Groups.SetCallbackSettingsAsync(new CallbackServerParams
                {
                    GroupId = Options.PollingGroup,
                    ServerId = _serverId,
                    CallbackSettings = new CallbackSettings { MessageNew = true }
                });
            };
        }

        protected override void DisposeWebhook()
        {
            _api.Groups.DeleteCallbackServerAsync(Options.PollingGroup, (ulong)_serverId);
        }
    }
}
