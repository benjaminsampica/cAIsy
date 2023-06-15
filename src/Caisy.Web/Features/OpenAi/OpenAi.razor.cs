using OpenAI_API;

namespace Caisy.Web.Features.OpenAi
{
    public partial class OpenAi
    {
        [Inject] public IRepository<UserProfile> ProfileRepository { get; set; } = null!;
        [Inject] public ISnackbar Snackbar { get; set; } = null!;
        private OpenAIAPI OpenAiApi { get; set; }
        private OpenApiRequest _request = new();
        private OpenApiResponse? _response;
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
        }

        private async Task OnValidSubmitAsync()
        {
            var result = await OpenAiApi.Completions.GetCompletion(_request.Prompt);
            _response = new OpenApiResponse()
            {
                Response = result
            };
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
