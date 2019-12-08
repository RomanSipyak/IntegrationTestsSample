using System.Threading.Tasks;

namespace IntegrationTestsSample.Services
{
    public interface IGithubClient
  {
    Task<GithubUser> GetUserAsync(string userName);
  }
}
