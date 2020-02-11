using function_api_heroes.Entity;
using System;

namespace function_api_heroes
{
    public static class Mappings
    {
        public const string HERO_PARTITION_KEY = "HEROES";
        public const string VILLAIN_PARTITION_KEY = "VILLAINS";

        public static HeroTableEntity ToTableEntity(this Hero todo)
        {
            return new HeroTableEntity()
            {
                PartitionKey = HERO_PARTITION_KEY,
                RowKey = todo.Id.ToString(),
                CreatedTime = DateTime.Now,
                Name = todo.Name,
                Description = todo.Description
            };
        }

        public static Hero ToHero(this HeroTableEntity todo)
        {
            return new Hero()
            {
                Id = new Guid(todo.RowKey),
                CreatedTime = todo.CreatedTime,
                Name = todo.Name,
                Description = todo.Description
            };
        }

        public static VillainTableEntity ToTableEntity(this Villain item)
        {
            return new VillainTableEntity()
            {
                PartitionKey = VILLAIN_PARTITION_KEY,
                RowKey = item.Id.ToString(),
                CreatedTime = DateTime.Now,
                Name = item.Name,
                Description = item.Description
            };
        }

        public static Villain ToVillain(this VillainTableEntity entity)
        {
            return new Villain()
            {
                Id = new Guid(entity.RowKey),
                CreatedTime = entity.CreatedTime,
                Name = entity.Name,
                Description = entity.Description
            };
        }

    }
}
