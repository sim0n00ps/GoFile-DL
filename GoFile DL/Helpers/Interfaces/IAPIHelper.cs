using GoFile_DL.Entities;

namespace GoFile_DL.Helpers
{
	public interface IAPIHelper
	{
		Task<string?> CreateAccount();
		Task<string?> GetSiteToken();
		Task<GetContentResponse?> GetContent(string contentId, Config config);
		Task<GetContentResponse?> GetContentWithPassword(string contentId, string password, Config config);
	}
}