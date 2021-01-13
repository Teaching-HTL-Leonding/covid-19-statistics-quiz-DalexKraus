using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoronaStatistics.Model
{
    public class District
    {
        public int Id { get; set; }

        [JsonIgnore]
        public FederalState State { get; set; }

        [MaxLength(20)]
        public string Code { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }
        
        [JsonIgnore]
        public List<CovidCases> Cases { get; set; }
    }
}