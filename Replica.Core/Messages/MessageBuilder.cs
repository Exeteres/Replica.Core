using System;
using System.Collections.Generic;
using System.Linq;
using Replica.Core.Contexts;
using Replica.Core.Entity;
using Replica.Core.Entity.Attachments;

namespace Replica.Core.Messages
{
    public class MessageBuilder
    {
        private OutMessage _message = new OutMessage();
        private readonly Lazy<List<Button>> _buttons = new Lazy<List<Button>>();
        private readonly Lazy<List<Attachment>> _attachments = new Lazy<List<Attachment>>();
        private readonly Lazy<List<string>> _photos = new Lazy<List<string>>();

        public MessageBuilder Set(MessageFlags flags)
        {
            _message.Flags |= flags;
            return this;
        }

        public MessageBuilder AddText(string text)
        {
            _message.Text += text + "\n";
            return this;
        }

        public MessageBuilder Replicate(InMessage message)
        {
            _message.Text = message.Text;
            _message.Attachments = message.Attachments;
            return this;
        }

        public MessageBuilder SetText(string text)
        {
            _message.Text = text;
            return this;
        }

        public MessageBuilder AddButton(string text, ButtonHandler handler)
        {
            _buttons.Value.Add(new Button(text, handler));
            return this;
        }

        public MessageBuilder AddAttachment(Attachment attachment)
        {
            _attachments.Value.Add(attachment);
            return this;
        }

        public MessageBuilder AddPhoto(string url)
        {
            _photos.Value.Add(url);
            return this;
        }

        public OutMessage Build()
        {
            if (_attachments.IsValueCreated)
                _message.Attachments = _attachments.Value.ToArray();
            return _message;
        }

        public OutMessage Build(Context context)
        {
            if (_photos.IsValueCreated)
            {
                foreach (var photo in _photos.Value)
                {
                    _attachments.Value.Add(new Photo(context.Controller.Name, new PhotoSize[] { new PhotoSize(context.Controller.Name, photo, photo, 0, 0, 0) }));
                }
            }
            var message = Build();
            if (_buttons.IsValueCreated)
                message.Buttons = _buttons.Value.ToArray();

            return message;
        }
    }
}
