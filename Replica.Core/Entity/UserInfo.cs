namespace Replica.Core.Entity
{
    public struct UserInfo
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Language { get; set; }
        public bool IsBot { get; set; }
        public string Title { get; set; }
    }
}
