using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoronaStatistics.Model
{
    public class FederalState
    {
        public int Id { get; set; }
        
        [MaxLength(50)]
        public string Name { get; set; }
        
        public List<District> Districts { get; set; }
    }
}