using function_api_heroes.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace function_api_heroes
{
    public static class VillainsApi
    {
        private const string VILLAINS_ROUTE = "villains";
        private const string VILLAINS_TABLE_NAME = "villainItems";

        [FunctionName("CreateVillain")]
        public static async Task<IActionResult> CreateVillain(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = VILLAINS_ROUTE)]HttpRequest req,
           [Table(VILLAINS_TABLE_NAME, Connection = "AzureWebJobsStorage")] IAsyncCollector<VillainTableEntity> villainTable,
           ILogger log)
        {
            log.LogInformation("Creating a new Villain");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<VillainCreateModel>(requestBody);

            var villain = new Villain()
            {
                Name = input.Name,
                Description = input.Description
            };
            await villainTable.AddAsync(villain.ToTableEntity());
            return new OkObjectResult(villain);
        }

        [FunctionName("GetVillains")]
        public static async Task<IActionResult> GetVillains(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = VILLAINS_ROUTE)]HttpRequest req,
        [Table(VILLAINS_TABLE_NAME, Connection = "AzureWebJobsStorage")] CloudTable villainTable,
        ILogger log)
        {
            log.LogInformation("Getting villains list items");
            var query = new TableQuery<VillainTableEntity>();
            var segment = await villainTable.ExecuteQuerySegmentedAsync(query, null);

            return new OkObjectResult(segment.Select(Mappings.ToVillain));
        }

        [FunctionName("UpdateVillain")]
        public static async Task<IActionResult> UpdateVillain(
           [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "villains/{id}")]HttpRequest req,
           [Table(VILLAINS_TABLE_NAME, Connection = "AzureWebJobsStorage")] CloudTable villainTable,
           ILogger log, Guid id)
        {
            var findOperation = TableOperation.Retrieve<VillainTableEntity>(Mappings.VILLAIN_PARTITION_KEY, id.ToString());
            var findResult = await villainTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new NotFoundResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updated = JsonConvert.DeserializeObject<VillainUpdateModel>(requestBody);
            var villainRow = (VillainTableEntity)findResult.Result;

            villainRow.Name = updated.Name;
            villainRow.Description = updated.Description;

            var replaceOperation = TableOperation.Replace(villainRow);
            await villainTable.ExecuteAsync(replaceOperation);
            return new OkObjectResult(villainRow.ToVillain());
        }

        [FunctionName("DeleteVillain")]
        public static async Task<IActionResult> DeleteVillain(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "villains/{id}")]HttpRequest req,
            [Table(VILLAINS_TABLE_NAME, Connection = "AzureWebJobsStorage")] CloudTable heroTable,
            ILogger log, Guid id)
        {
            var deleteOperation = TableOperation.Delete(new TableEntity()
            { PartitionKey = Mappings.VILLAIN_PARTITION_KEY, RowKey = id.ToString(), ETag = "*" });
            try
            {
                var deleteResult = await heroTable.ExecuteAsync(deleteOperation);
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == 404)
            {
                return new NotFoundResult();
            }
            return new OkResult();
        }
    }
}
