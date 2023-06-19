using OpenAI_API.Chat;
using static Caisy.Web.Infrastructure.Models.ChatHistory;

namespace Caisy.Web.Features.Shared.Models;

public abstract class ConversationBase
{
    public required IEnumerable<Message> Messages { get; set; } = Array.Empty<Message>();

    public record Message(string Content, MessageRole Role)
    {
        private static Dictionary<MessageRole, ChatMessageRole> _roleMapping { get; set; } = new()
        {
            { MessageRole.User, ChatMessageRole.User },
            { MessageRole.Caisy, ChatMessageRole.System }
        };

        public class MessageProfile : Profile
        {
            public MessageProfile()
            {
                CreateMap<ChatHistoryMessage, Message>()
                    .ReverseMap();

                CreateMap<Message, ChatMessage>()
                    .ForMember(d => d.Role, mo => mo.MapFrom(s => _roleMapping.GetValueOrDefault(s.Role)))
                    .ReverseMap()
                    .ForMember(d => d.Role, mo => mo.MapFrom(s => _roleMapping.Single(v => v.Value == s.Role).Key));
            }
        }
    }

    public enum MessageRole
    {
        User,
        Caisy
    }
}
