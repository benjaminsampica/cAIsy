using OpenAI_API.Chat;
using static Caisy.Web.Infrastructure.Models.ChatHistory;

namespace Caisy.Web.Features.Shared;

public abstract class ConversationBase
{
    internal ICollection<Message> Messages { get; set; } = new List<Message>();

    public void AddUserMessage(string message) => Messages.Add(new Message { Content = message, Role = Message.MessageRole.User });
    public void AddCaisyMessage(string message) => Messages.Add(new Message { Content = message, Role = Message.MessageRole.Caisy });
    public void UndoLastConversation()
    {
        Messages.Remove(Messages.LastOrDefault()!);
        Messages.Remove(Messages.LastOrDefault()!);
    }

    public int MessageCount => Messages.Count;

    public class Message
    {
        public required string Content { get; init; }
        public required MessageRole Role { get; init; }

        public class MessageProfile : Profile
        {
            private static Dictionary<MessageRole, ChatMessageRole> RoleMapping { get; set; } = new()
            {
                { MessageRole.User, ChatMessageRole.User },
                { MessageRole.Caisy, ChatMessageRole.Assistant },
                { MessageRole.Hidden, ChatMessageRole.System }
            };

            public MessageProfile()
            {
                CreateMap<ChatHistoryMessage, Message>()
                    .ReverseMap();

                CreateMap<Message, ChatMessage>()
                    .ForMember(d => d.Role, mo => mo.MapFrom(s => RoleMapping.GetValueOrDefault(s.Role)))
                    .ConstructUsing((message, d) =>
                    {
                        return new ChatMessage();
                    });
            }
        }

        public bool IsUserMessage => Role == MessageRole.User;

        public enum MessageRole
        {
            User,
            Caisy,
            Hidden
        }
    }
}
