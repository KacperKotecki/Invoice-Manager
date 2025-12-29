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
    public class ClientRepository
    {
        private readonly ApplicationDbContext _context;
        public ClientRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Client>> GetActiveClientsAsync(int companyId)
        {
            return await _context.Clients
                .Where(c => c.CompanyId == companyId && c.IsActive)
                .ToListAsync();
        }

        public async Task<Client> GetClientByIdAsync(int clientId)
        {
            return await _context.Clients.FindAsync(clientId);
        }
    }
}