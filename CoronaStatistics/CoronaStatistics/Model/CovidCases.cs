using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoronaStatistics.Model
{
    public class CovidCases
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        [JsonIgnore]
        public District District { get; set; }

        [Required]
        public int Population { get; set; }

        [Required]
        public int CaseCount { get; set; }

        [Required]
        public int Deaths { get; set; }

        [Required]
        public int WeekIncidence { get; set; }
    }
}