using function_api_heroes.Entity;
using function_api_heroes.Models;
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
    public static class HeroesApi
    {
        private const string HEROES_ROUTE = "heroes";
        private const string TABLE_NAME = "heroesItems";

        [FunctionName("CreateHero")]
        public static async Task<IActionResult> CreateTodo(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = HEROES_ROUTE)]HttpRequest req,
           [Table(TABLE_NAME, Connection = "AzureWebJobsStorage")] IAsyncCollector<HeroTableEntity> heroTable,
           ILogger log)
        {
            log.LogInformation("Creating a new todo list item");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<HeroCreateModel>(requestBody);

            var todo = new Hero()
            {
                Name = input.Name,
                Description = input.Description
            };

            await heroTable.AddAsync(todo.ToTableEntity());
            return new OkObjectResult(todo);
        }

        [FunctionName("GetHeroes")]
        public static async Task<IActionResult> GetHeroes(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = HEROES_ROUTE)]HttpRequest req,
        [Table(TABLE_NAME, Connection = "AzureWebJobsStorage")] CloudTable heroTable,
        ILogger log)
        {
            log.LogInformation("Getting heroes list items");

            var query = new TableQuery<HeroTableEntity>();
            var segment = await heroTable.ExecuteQuerySegmentedAsync(query, null);

            return new OkObjectResult(segment.Select(Mappings.ToHero));
        }

        [FunctionName("UpdateTodo")]
        public static async Task<IActionResult> UpdateTodo(
           [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "heroes/{id}")]HttpRequest req,
           [Table(TABLE_NAME, Connection = "AzureWebJobsStorage")] CloudTable heroTable,
           ILogger log, Guid id)
        {
            var findOperation = TableOperation.Retrieve<HeroTableEntity>(Mappings.HERO_PARTITION_KEY, id.ToString());
            var findResult = await heroTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new NotFoundResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updated = JsonConvert.DeserializeObject<HeroUpdateModel>(requestBody);
            var heroRow = (HeroTableEntity)findResult.Result;

            heroRow.Name = updated.Name;
            heroRow.Description = updated.Description;

            var replaceOperation = TableOperation.Replace(heroRow);
            await heroTable.ExecuteAsync(replaceOperation);
            return new OkObjectResult(heroRow.ToHero());
        }

        [FunctionName("DeleteTodo")]
        public static async Task<IActionResult> DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "heroes/{id}")]HttpRequest req,
            [Table(TABLE_NAME, Connection = "AzureWebJobsStorage")] CloudTable heroTable,
            ILogger log, Guid id)
        {
            var deleteOperation = TableOperation.Delete(new TableEntity()
            { PartitionKey = Mappings.HERO_PARTITION_KEY, RowKey = id.ToString(), ETag = "*" });
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
