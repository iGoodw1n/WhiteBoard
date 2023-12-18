using ApiBoard.Helpers;
using ApiBoard.Services;

namespace ApiBoard.Extensions
{
    public static class StorageExtensions
    {
        public static async Task<IServiceCollection> AddStorageCosmos(this IServiceCollection services)
        {
            var cosmosDbConnection = await DbCreator.EnsureDbCreatedAsync(StringStorage.EndpointUri, StringStorage.PrimaryKey);
            var storage = new BoardCloudStorageService(cosmosDbConnection);
            services.AddSingleton(storage);
            return services;
        }
    }
}
