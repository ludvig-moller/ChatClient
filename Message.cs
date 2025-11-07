
namespace ChatClient
{
    public class Message
    {
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public required string Username { get; set; }
        public required string Text { get; set; }
    }
}
