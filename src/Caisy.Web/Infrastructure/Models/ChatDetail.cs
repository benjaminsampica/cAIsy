using OpenAI_API.Chat;

namespace Caisy.Web.Infrastructure.Models;

public class ChatDetail
{
    public ChatDetail()
    {

    }
    public ChatDetail(List<ChatMessage> chatMessages, string source, string destination, int? id = null)
    {
        this.Source = source;
        this.Destination = destination;
        this.ChatTime = DateTime.Now;
        if(id == null)
        {
            this.Id = new Random().Next(1, 9999);
        }
        else
        {
            this.Id = id.Value;
        }
        
        
        this.Messages = new List<MessageContent>();

        if(chatMessages != null && chatMessages.Count > 0)
        {
            foreach(var chatMessage in chatMessages)
            {
                this.Messages.Add(new MessageContent
                {
                    Role = chatMessage.Role,
                    Content = chatMessage.Content
                });
            }
        }
    }

    public int? Id { get;set;}
    public string? Source { get; set; }
    public string? Destination { get; set; }
    public DateTime ChatTime { get;set;}
    public string MessageForGrid
    {
        get
        {
            var messgae = string.Empty;
            if (Messages.Count > 1)
            {
                messgae = Messages[1].Content.Substring(0, Math.Min(Messages[1].Content.Length, 100)) + " ...";
            }
            return messgae;
        }
    }
    public List<MessageContent>? Messages {get; set;}
}

public class MessageContent
{
    public string? Role { get; set; }
    public string? Content { get; set; }
}

