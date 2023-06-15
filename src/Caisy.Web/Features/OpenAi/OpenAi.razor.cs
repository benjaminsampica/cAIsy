using OpenAI_API;
using OpenAI_API.Chat;

namespace Caisy.Web.Features.OpenAi
{
    public partial class OpenAi
    {
        [Inject] public IRepository<UserProfile> ProfileRepository { get; set; } = null!;
        [Inject] public ISnackbar Snackbar { get; set; } = null!;
        private OpenAIAPI OpenAiApi { get; set; }
        private OpenApiRequest _request = new();
        private OpenApiResponse _response = new();
        private Conversation _conversation;
        private readonly CancellationTokenSource _cts = new();

        protected override async Task OnInitializedAsync()
        {
            var profile = (await ProfileRepository.GetAllAsync(_cts.Token)).FirstOrDefault();

            if (profile != null) 
            {
                OpenAiApi = new OpenAIAPI(profile.ApiKey);
            }
            else
            {
                Snackbar.Add("No profile found.", Severity.Error);
            }       

            _conversation = OpenAiApi.Chat.CreateConversation();
        }

        private async Task OnValidSubmitAsync()
        {

            /// give instruction as System
            /// This is probably where we will massage the prompt
            //chat.AppendSystemMessage("You are a teacher who helps children understand if things are animals or not.  If the user tells you an animal, you say \"yes\".  If the user tells you something that is not an animal, you say \"no\".  You only ever respond with \"yes\" or \"no\".  You do not say anything else.");

            _conversation.AppendUserInput(_request.Prompt);
            _response.Response = await _conversation.GetResponseFromChatbotAsync();       
        }
    }

    public class OpenApiRequest
    {
        public string? Prompt { get; set; }
    }

    public class OpenApiResponse
    {
        public string? Response { get; set; }
    }

}
