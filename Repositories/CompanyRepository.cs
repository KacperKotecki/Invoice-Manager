using Invoice_Manager.Models;
using Invoice_Manager.Models.Domains;
using System.Threading.Tasks;


namespace Invoice_Manager.Repositories
{
    public class CompanyRepository
    {
        private readonly ApplicationDbContext _context;
        public CompanyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Company> GetCompanyByIdAsync(int companyId)
        {
            return await _context.Companies.FindAsync(companyId);
        }
    }
}