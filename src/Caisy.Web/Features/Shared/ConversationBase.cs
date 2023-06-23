using OpenAI_API.Chat;
using static Caisy.Web.Infrastructure.Models.ChatHistory;

namespace Caisy.Web.Features.Shared;

public abstract class ConversationBase
{
    public ICollection<Message> Messages { get; set; } = new List<Message>();

    public class Message
    {
        public required string Content { get; init; }
        public required MessageRole Role { get; init; }

        public class MessageProfile : Profile
        {
            public static Dictionary<MessageRole, ChatMessageRole> RoleMapping { get; set; } = new()
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
                    })
                    .ReverseMap()
                    .ForMember(d => d.Role, mo => mo.MapFrom(s => RoleMapping.Single(v => v.Value == s.Role).Key));
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
