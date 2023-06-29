using OpenAI_API;
using OpenAI_API.Chat;

namespace Caisy.Web.Features.Shared.Services;

public interface IOpenAIApiService
{
    Task<string> GetBestCompletionAsync(ConversationBase conversation, CancellationToken cancellationToken = default);
}

public class OpenAIApiService : IOpenAIApiService
{
    private readonly OpenAIAPI _openAIApi;
    private readonly IMapper _mapper;

    public OpenAIApiService(IIdentityProvider identityProvider, IMapper mapper)
    {
        _openAIApi = new OpenAIAPI(identityProvider.User!.ApiKey);
        _mapper = mapper;
    }

    public async Task<string> GetBestCompletionAsync(ConversationBase conversation, CancellationToken cancellationToken = default)
    {
        var existingMessages = _mapper.Map<List<ChatMessage>>(conversation.Messages);

        var completion = await _openAIApi.Chat.CreateChatCompletionAsync(existingMessages);

        var response = completion.Choices.Single().Message.Content;

        return response;
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
