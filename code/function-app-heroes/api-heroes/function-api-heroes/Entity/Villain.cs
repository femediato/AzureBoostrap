using System;

namespace function_api_heroes.Entity
{
    public class Villain
    {
        public Guid Id { get; internal set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedTime { get; internal set; }

        public Villain()
        {
            this.Id = Guid.NewGuid();
        }
    }
}