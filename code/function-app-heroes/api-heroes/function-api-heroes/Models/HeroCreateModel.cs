using Newtonsoft.Json;

namespace function_api_heroes.Models
{
    public class HeroCreateModel
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }
}
