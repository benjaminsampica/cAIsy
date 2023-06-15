namespace Caisy.Web.Features.OpenAi
{
    public class OpenAiService
    {

        //TODO: circle back to this
        public UserProfile _userProfile { get; set; }

        public OpenAiService(UserProfile userProfile)
        {
            _userProfile = userProfile;
        }

        
    }
}
