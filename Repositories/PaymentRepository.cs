using Invoice_Manager.Models;
using Invoice_Manager.Models.Domains;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Invoice_Manager.Repositories
{
    public class PaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Invoice> GetInvoiceWithPaymentsAsync(int invoiceId, int companyId)
        {
            return await _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId && i.CompanyId == companyId);
        }

        public void AddPayment(Payment payment)
        {
            _context.Payments.Add(payment);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}