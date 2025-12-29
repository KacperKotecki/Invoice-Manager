using Invoice_Manager.Models;
using Invoice_Manager.Models.Domains;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Invoice_Manager.Repositories
{
    public class TaxRateRepository
    {
        private readonly ApplicationDbContext _context;

        public TaxRateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaxRate>> GetTaxRatesByCountryAsync(string countryCode)
        {
            var rates = await _context.TaxRates
                .Where(t => t.Country == countryCode && t.IsActive)
                .ToListAsync();

            if (!rates.Any())
            {
                rates = await _context.TaxRates
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.Rate)
                    .ToListAsync();
            }

            return rates;
        }
    }
}