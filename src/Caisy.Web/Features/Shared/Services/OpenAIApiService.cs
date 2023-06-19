using Caisy.Web.Features.Shared.Models;
using OpenAI_API;
using OpenAI_API.Chat;
using static Caisy.Web.Features.Shared.Models.ConversationBase;

namespace Caisy.Web.Features.Shared.Services;

public interface IOpenAIApiService
{
    Task<Message> GetBestCompletionAsync(ConversationBase conversation);
}

public class OpenAIApiService : IOpenAIApiService
{
    private readonly OpenAIAPI _openAIApi;
    private readonly IMapper _mapper;

    public OpenAIApiService(IUser user, IMapper mapper)
    {
        _openAIApi = new OpenAIAPI(user.ApiKey);
        _mapper = mapper;
    }

    public async Task<Message> GetBestCompletionAsync(ConversationBase conversation)
    {
        var existingMessages = _mapper.Map<List<ChatMessage>>(conversation.Messages);

        var completion = await _openAIApi.Chat.CreateChatCompletionAsync(existingMessages);

        var returnMessage = _mapper.Map<Message>(completion.Choices.Single().Message);
        return returnMessage;
    }
}

public class OpenApiRequest
{
    public string? Prompt { get; set; }
    public string? TestCaseFramework { get; set; }
}

public class OpenApiResponse
{
    public string? Response { get; set; }
}
