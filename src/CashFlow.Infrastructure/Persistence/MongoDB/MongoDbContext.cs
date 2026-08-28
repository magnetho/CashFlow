using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CashFlow.Infrastructure.Persistence.MongoDB;

internal sealed class MongoDbContext
{
    public MongoDbContext(IOptions<MongoDbOptions> options)
    {
        Client = new MongoClient(options.Value.ConnectionString);
        Database = Client.GetDatabase(options.Value.DatabaseName);
    }

    public IMongoClient Client { get; }

    public IMongoDatabase Database { get; }

    public IMongoCollection<DailyBalanceDocument> DailyBalances =>
        Database.GetCollection<DailyBalanceDocument>("daily_balances");

    public IMongoCollection<InboxMessageDocument> InboxMessages =>
        Database.GetCollection<InboxMessageDocument>("inbox_messages");
}
