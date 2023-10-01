using GoFile_DL.Entities;

namespace GoFile_DL.Helpers
{
	public interface IAPIParser
	{
		Task<Folder> ParseApiResponse(GetContentResponse response, Config config);
	}
}