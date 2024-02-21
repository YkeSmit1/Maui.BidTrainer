using Microsoft.Azure.Cosmos;

namespace Maui.BidTrainer.Platforms.Windows
{
    public class CosmosDbHelper : ICosmosDbHelper
    {
        private static readonly CosmosClient Client = new CosmosClient(Constants.EndpointUri, Constants.PrimaryKey, new CosmosClientOptions { ApplicationRegion = Regions.WestEurope, });
        private static readonly Container Container = Client.GetContainer(Constants.DatabaseName, Constants.CollectionName);

        public async Task<IEnumerable<Account>> GetAllAccounts()
        {
            var queryDefinition = new QueryDefinition("select * from c");
            using var query = Container.GetItemQueryIterator<Account>(queryDefinition);
            return await query.ReadNextAsync();
        }

        public async Task<Account?> GetAccount(string username)
        {
            var queryDefinition = new QueryDefinition($"select * from c where c.username = '{username}'");
            using var query = Container.GetItemQueryIterator<Account>(queryDefinition);
                var account = await query.ReadNextAsync();
            return account.FirstOrDefault();
        }

        public async Task InsertAccount(Account account)
        {
            await Container.CreateItemAsync(account);
        }

        public async Task UpdateAccount(Account account)
        {
            await Container.UpsertItemAsync(account);
        }
    }
}