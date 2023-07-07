using static Caisy.Web.Infrastructure.Models.ChatHistory;

namespace Caisy.Web.Features.Shared.Handlers;

public interface IArchivingState
{
    long? ChatHistoryId { get; set; }
    ConversationBase Conversation { get; set; }
    abstract ChatHistoryType GetType();
}

public record SaveChatHistoryCommand<TState> : INotification
    where TState : class, IArchivingState
{
}

public class SaveChatHistoryCommandHandler<TState> : INotificationHandler<SaveChatHistoryCommand<TState>>
    where TState : class, IArchivingState
{
    private readonly IRepository<Infrastructure.Models.ChatHistory> _chatHistoryRepository;
    private readonly IMapper _mapper;
    private readonly TState _state;

    public SaveChatHistoryCommandHandler(IRepository<Infrastructure.Models.ChatHistory> chatHistoryRepository, IMapper mapper, TState state)
    {
        _chatHistoryRepository = chatHistoryRepository;
        _mapper = mapper;
        _state = state;
    }

    public async Task Handle(SaveChatHistoryCommand<TState> command, CancellationToken cancellationToken)
    {
        var chatHistoryMessages = _mapper.Map<List<ChatHistoryMessage>>(_state.Conversation.Messages);

        if (_state.ChatHistoryId is not null)
        {
            await _chatHistoryRepository.RemoveAsync(_state.ChatHistoryId.Value, cancellationToken);
        }

        var chatHistory = new Infrastructure.Models.ChatHistory
        {
            Messages = chatHistoryMessages,
            Type = _state.GetType()
        };

        await _chatHistoryRepository.AddAsync(chatHistory, cancellationToken);

        _state.ChatHistoryId = chatHistory.Id;
    }
}
