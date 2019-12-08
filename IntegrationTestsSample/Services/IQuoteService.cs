using System.Threading.Tasks;

namespace IntegrationTestsSample.Services
{
    public interface IQuoteService
    {
        Task<string> GenerateQuote();
    }
}
