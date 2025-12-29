using Invoice_Manager.Models;
using Invoice_Manager.Models.Domains;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Invoice_Manager.Repositories
{
    
    public class ProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetProductsForCompanyAsync(int companyId)
        {
            return await _context.Products
                .Where(p => p.CompanyId == companyId)
                .ToListAsync();
        }
    }
}