using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CoronaStatistics.Database;
using CoronaStatistics.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoronaStatistics.Controllers
{
    [ApiController]
    [Route("api/")]
    public class CoronaStatisticsController : ControllerBase
    {
        private readonly ILogger<CoronaStatisticsController> _logger;
        private readonly CoronaStatisticsDbContext _dbContext;

        public CoronaStatisticsController(ILogger<CoronaStatisticsController> logger, CoronaStatisticsDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("states")]
        public IEnumerable<FederalState> GetStates() 
            => _dbContext.FederalStates.Include(s => s.Districts);
        
        [HttpGet]
        [Route("states/{stateId}/cases")]
        public IEnumerable<CovidCases> GetStates([FromRoute] int stateId) 
            => _dbContext.CovidCases
            .Where(c => c.District.State.Id == stateId)
            .Include(c => c.District);
        
        [HttpGet]
        [Route("cases")]
        public IEnumerable<CovidCases> GetCases()
            => _dbContext.CovidCases;

        [HttpPost]
        [Route("importData")]
        public async Task ImportData()
        {
            HttpClient client = new HttpClient();
            if (!_dbContext.FederalStates.Any() && !_dbContext.Districts.Any())
            {
                var csvContent = await client.GetStringAsync("http://www.statistik.at/verzeichnis/reglisten/polbezirke.csv");
                var lines = csvContent.Split("\n").Skip(3).SkipLast(2).ToList();

                // Parse federal states
                var federalStates = lines.Select(l => l.Split(";"))
                    .Select(de => de[1])
                    .Distinct()
                    .Select(de => new FederalState() { Name = de }).ToList();

                // Parse districts
                var districts = lines.Select(l => l.Split(";"))
                    .Distinct()
                    .Select(de => new District()
                    {
                        Name = de[3],
                        State = federalStates.Find(fs => fs.Name == de[1]),
                        Code = de[2]
                    }).ToList();

                foreach (var district in districts)
                {
                    district.State.Districts.Add(district);
                }
                
                await _dbContext.FederalStates.AddRangeAsync(federalStates);
                await _dbContext.Districts.AddRangeAsync(districts);
                await _dbContext.SaveChangesAsync();
            }

            var casesCsvContent = await client.GetStringAsync("https://covid19-dashboard.ages.at/data/CovidFaelle_GKZ.csv");
            var casesLines = casesCsvContent.Split("\n").Skip(1).ToList();

            // Update data if not from this day already
            var covidDate = new DateTime(_dbContext.CovidCases.ToList().Min(cc => cc.Date.Ticks));
            var currentDate = DateTime.Now;
            if (covidDate.Year != currentDate.Year || covidDate.DayOfYear != currentDate.DayOfYear)
            {
                // Clear case table first
                var transaction = await _dbContext.Database.BeginTransactionAsync();
                await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM dbo.CovidCases");
                await transaction.CommitAsync();

                var cases = casesLines.Select(l => l.Split(";"))
                    .Select(col => new CovidCases()
                    {
                        District = _dbContext.Districts.ToList().Find(d => d.Code == col[1]),
                        Population = int.Parse(col[2]),
                        CaseCount = int.Parse(col[3]),
                        Deaths = int.Parse(col[4]),
                        WeekIncidence = int.Parse(col[5]),
                        Date = DateTime.Now
                    }).ToList();

                foreach (var covidCase in cases)
                {
                    var caseDistrict = _dbContext.Districts.FirstOrDefault(d => d.Code == covidCase.District.Code);
                    caseDistrict?.Cases.Add(covidCase);
                }
                
                await _dbContext.CovidCases.AddRangeAsync(cases);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}