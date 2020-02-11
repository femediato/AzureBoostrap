using System;

namespace function_api_heroes.Entity
{
    public class Hero
    {
        public Guid Id { get; internal set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedTime { get; internal set; }

        public Hero()
        {
            this.Id = Guid.NewGuid();
            this.CreatedTime = DateTime.Now;
        }
    }
}
