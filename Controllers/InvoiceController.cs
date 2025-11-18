using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Invoice_Manager.Filters;
using Invoice_Manager.Models;
using Invoice_Manager.Models.Domains;
using Invoice_Manager.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace Invoice_Manager.Controllers
{
    [CheckProfileCompletion]
    [Authorize]
    public class InvoiceController : Controller
    {
        private ApplicationDbContext _context;
        private ApplicationUserManager _userManager;

        public InvoiceController()
        {
            _context = new ApplicationDbContext();
            _userManager = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        }


        public async Task<ActionResult> Index(string searchQuery = null, InvoiceStatus? status = null)
        {
            var userId = User.Identity.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            var companyId = user.CompanyId;

            // --- Obliczanie statystyk ---
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var amountToCollect = await _context.Invoices
                .Where(i => i.CompanyId == companyId && (i.Status == InvoiceStatus.Sent || i.Status == InvoiceStatus.Overdue))
                .SumAsync(i => (decimal?)i.TotalGrossAmount) ?? 0;

            var amountCollectedThisMonth = await _context.Payments
                .Where(p => p.Invoice.CompanyId == companyId && p.PaymentDate >= thirtyDaysAgo)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var overdueCount = await _context.Invoices
                .CountAsync(i => i.CompanyId == companyId && i.Status == InvoiceStatus.Overdue);

            var statsViewModel = new DashboardStatsViewModel
            {
                AmountToCollect = amountToCollect,
                AmountCollectedThisMonth = amountCollectedThisMonth,
                OverdueCount = overdueCount
            };

            // --- Budowanie głównego zapytania o faktury ---
            var query = _context.Invoices
                .Where(i => i.CompanyId == companyId);

            // Filtrowanie
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(i => i.InvoiceNumber.Contains(searchQuery) || i.Client_Name.Contains(searchQuery));
            }

            if (status.HasValue)
            {
                query = query.Where(i => i.Status == status.Value);
            }

            // Projekcja i pobranie danych
            var invoices = await query
                .OrderByDescending(i => i.IssueDate)
                .Take(30) // Paginacja
                .Select(i => new InvoiceCardViewModel
                {
                    InvoiceId = i.InvoiceId,
                    InvoiceNumber = i.InvoiceNumber,
                    ClientName = i.Client_Name,
                    TotalGrossAmount = i.TotalGrossAmount,
                    Currency = i.Currency,
                    Status = i.Status,
                    DueDate = i.DueDate
                }).ToListAsync();

            // --- Złożenie finalnego ViewModelu ---
            var viewModel = new DashboardViewModel
            {
                Invoices = invoices,
                Stats = statsViewModel,
                SearchQuery = searchQuery,
                ActiveFilter = status
            };

            return View(viewModel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
