namespace Maui.BidTrainer.Platforms.IOS;

public class CosmosDbHelper : ICosmosDbHelper
{

    public async Task<IEnumerable<Account>> GetAllAccounts()
    {
        return await Task.FromResult(new List<Account>());
    }

    public Task<Account?> GetAccount(string username)
    {
        throw new NotImplementedException();
    }

    public Task InsertAccount(Account account)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAccount(Account account)
    {
        throw new NotImplementedException();
    }
}