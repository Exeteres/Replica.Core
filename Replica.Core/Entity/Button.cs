namespace Replica.Core.Entity
{
    public delegate void ButtonHandler(InlineQuery query = null);

    public struct Button
    {
        public Button(string text, ButtonHandler handler)
        {
            Text = text;
            Handler = handler;
        }

        public string Text { get; }
        public ButtonHandler Handler { get; }
    }
}
