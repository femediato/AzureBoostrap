using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace function_api_heroes.Entity
{
    public class HeroTableEntity : TableEntity
    {
        public DateTime CreatedTime { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
