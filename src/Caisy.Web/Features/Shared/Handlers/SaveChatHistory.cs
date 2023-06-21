﻿using static Caisy.Web.Infrastructure.Models.ChatHistory;

namespace Caisy.Web.Features.Shared.Handlers;

public record SaveChatHistoryCommand : IRequest<string>
{
    public string? ExistingChatHistoryId { get; set; }
    public required ConversationBase Conversation { get; set; }
}

public class SaveChatHistory : IRequestHandler<SaveChatHistoryCommand, string>
{
    private readonly IRepository<Infrastructure.Models.ChatHistory> _chatHistoryRepository;
    private readonly IMapper _mapper;

    public SaveChatHistory(IRepository<Infrastructure.Models.ChatHistory> chatHistoryRepository, IMapper mapper)
    {
        _chatHistoryRepository = chatHistoryRepository;
        _mapper = mapper;
    }

    public async Task<string> Handle(SaveChatHistoryCommand command, CancellationToken cancellationToken)
    {
        var chatHistoryMessages = _mapper.Map<List<ChatHistoryMessage>>(command.Conversation.Messages);

        if (command.ExistingChatHistoryId is not null)
        {
            await _chatHistoryRepository.RemoveAsync(command.ExistingChatHistoryId, cancellationToken);
        }

        var chatHistory = new Infrastructure.Models.ChatHistory
        {
            Messages = chatHistoryMessages
        };

        await _chatHistoryRepository.AddAsync(chatHistory, cancellationToken);

        return chatHistory.Id;
    }
}