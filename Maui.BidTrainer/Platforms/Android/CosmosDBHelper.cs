using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.VisualBasic;

//[assembly: Xamarin.Forms.Dependency(typeof(CosmosDbHelper))]
namespace Maui.BidTrainer
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