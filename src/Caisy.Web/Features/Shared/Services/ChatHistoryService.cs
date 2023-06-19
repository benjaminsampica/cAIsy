using Caisy.Web.Features.Shared.Models;
using static Caisy.Web.Infrastructure.Models.ChatHistory;

namespace Caisy.Web.Features.Shared.Services;

public interface IChatHistoryService
{
    Task<string> SaveAsync(ConversationBase conversation, string? existingChatHistoryId = null, CancellationToken cancellationToken = default);
}

public class ChatHistoryService : IChatHistoryService
{
    private readonly IRepository<Infrastructure.Models.ChatHistory> _chatHistoryRepository;
    private readonly IMapper _mapper;

    public ChatHistoryService(IRepository<Infrastructure.Models.ChatHistory> chatHistoryRepository, IMapper mapper)
    {
        _chatHistoryRepository = chatHistoryRepository;
        _mapper = mapper;
    }

    public async Task<string> SaveAsync(ConversationBase conversation, string? existingChatHistoryId = null, CancellationToken cancellationToken = default)
    {
        var chatHistoryMessages = _mapper.Map<List<ChatHistoryMessage>>(conversation.Messages);

        if (existingChatHistoryId is not null)
        {
            await _chatHistoryRepository.RemoveAsync(existingChatHistoryId, cancellationToken);
        }

        var chatHistory = new Infrastructure.Models.ChatHistory
        {
            Messages = chatHistoryMessages
        };

        await _chatHistoryRepository.AddAsync(chatHistory, cancellationToken);

        return chatHistory.Id;
    }
}
