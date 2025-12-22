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
using Rotativa;

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

            var query = _context.Invoices
                .Include(i => i.Payments) 
                .Where(i => i.CompanyId == companyId);


            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(i => i.InvoiceNumber.Contains(searchQuery) || i.Client_Name.Contains(searchQuery));
            }

            if (status.HasValue)
            {
                query = query.Where(i => i.Status == status.Value);
            }

            var invoices = await query
                .OrderByDescending(i => i.IssueDate)
                .Take(30)
                .Select(i => new InvoiceCardViewModel
                {
                    InvoiceId = i.InvoiceId,
                    InvoiceNumber = i.InvoiceNumber,
                    ClientName = i.Client_Name,
                    TotalGrossAmount = i.TotalGrossAmount,
                    Currency = i.Currency,
                    Status = i.Status,
                    DueDate = i.DueDate,
                    Payments = i.Payments
                })
                .ToListAsync();


            var viewModel = new DashboardViewModel
            {
                Invoices = invoices,
                Stats = statsViewModel,
                SearchQuery = searchQuery,
                ActiveFilter = status
            };

            return View(viewModel);
        }
        public async Task<ActionResult> Create()
        {
            var userId = User.Identity.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            var companyId = user.CompanyId;

            // Pobieramy firmę, aby uzyskać kod kraju (np. "PL")
            var company = await _context.Companies.FindAsync(companyId);

            var clients = await _context.Clients
                .Where(c => c.CompanyId == companyId && c.IsActive)
                .ToListAsync();

            var products = await _context.Products
                .Where(p => p.CompanyId == companyId)
                .ToListAsync();

            // ZMIANA: Pobieranie stawek VAT na podstawie kraju firmy
            var taxRates = await GetTaxRatesForCountry(company.Country);

            var viewModel = new InvoiceFormViewModel
            {
                Heading = "Nowa Faktura",
                Invoice = new Invoice
                {
                    IssueDate = DateTime.Now,
                    SaleDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(14), // Domyślny termin płatności
                    Currency = "PLN",
                    PaymentMethod = "Przelew"
                },
                Clients = clients,
                Products = products,
                TaxRates = taxRates
            };

            return View("InvoiceForm", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Prefix = "Invoice")] Invoice invoice)
        {
            var userId = User.Identity.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            var keys = ModelState.Keys.ToList();

            foreach (var key in keys)
            {
                if (key.EndsWith("InvoiceNumber") ||
                    key.EndsWith("Currency") ||
                    key.EndsWith("Status") ||
                    key.Contains("Company_") ||
                    key.Contains("Client_") ||
                    key.EndsWith("Company") ||
                    key.EndsWith("Client"))
                {
                    ModelState.Remove(key);
                }


                if (key.Contains("InvoiceItems") && key.EndsWith(".Name"))
                {
                    ModelState.Remove(key);
                }
            }


            if (invoice.InvoiceItems == null || !invoice.InvoiceItems.Any())
            {
                ModelState.AddModelError("", "Faktura musi mieć co najmniej jedną pozycję.");
            }

            if (!ModelState.IsValid)
            {

                return await ReloadFormWithErrors(invoice, user.CompanyId);
            }



            invoice.CompanyId = user.CompanyId;
            invoice.Status = InvoiceStatus.Draft;


            if (string.IsNullOrEmpty(invoice.Currency))
            {
                invoice.Currency = "PLN";
            }


            var count = await _context.Invoices.CountAsync(i => i.CompanyId == user.CompanyId) + 1;
            invoice.InvoiceNumber = $"FV/{DateTime.Now.Year}/{DateTime.Now.Month:D2}/{count}";


            var company = await _context.Companies.FindAsync(user.CompanyId);
            if (company == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest, "Brak profilu firmy");

            invoice.Company_Name = company.CompanyName;
            invoice.Company_TaxId = company.TaxId;
            invoice.Company_Street = company.Street;
            invoice.Company_City = company.City;
            invoice.Company_PostalCode = company.PostalCode;
            invoice.Company_BankName = company.BankName;
            invoice.Company_BankAccount = company.BankAccount;
            


            var client = await _context.Clients.FindAsync(invoice.ClientId);
            if (client == null || client.CompanyId != user.CompanyId)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }

            invoice.Client_Name = client.ClientName;
            invoice.Client_TaxId = client.TaxId;
            invoice.Client_Street = client.Street;
            invoice.Client_City = client.City;
            invoice.Client_PostalCode = client.PostalCode;


            decimal grandTotalNet = 0;
            decimal grandTotalTax = 0;
            decimal grandTotalGross = 0;

            foreach (var item in invoice.InvoiceItems)
            {
                item.TotalNetAmount = item.Quantity * item.UnitPriceNet;
                item.TotalTaxAmount = item.TotalNetAmount * (item.TaxRateValue / 100m);
                item.TotalGrossAmount = item.TotalNetAmount + item.TotalTaxAmount;

                grandTotalNet += item.TotalNetAmount;
                grandTotalTax += item.TotalTaxAmount;
                grandTotalGross += item.TotalGrossAmount;


                if (string.IsNullOrEmpty(item.Name) && item.ProductId.HasValue)
                {
                    var prod = await _context.Products.FindAsync(item.ProductId);
                    if (prod != null) item.Name = prod.Name;
                }
            }

            invoice.TotalNetAmount = grandTotalNet;
            invoice.TotalTaxAmount = grandTotalTax;
            invoice.TotalGrossAmount = grandTotalGross;

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(int id)
        {
            var userId = User.Identity.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            var companyId = user.CompanyId;


            var invoice = await _context.Invoices
               .Include(i => i.InvoiceItems)
               .FirstOrDefaultAsync(i => i.InvoiceId == id && i.CompanyId == companyId);


            if (invoice == null)
            {
                return HttpNotFound();
            }

            var company = await _context.Companies.FindAsync(companyId);

            var clients = await _context.Clients
                .Where(c => c.CompanyId == companyId && c.IsActive)
                .ToListAsync();

            var products = await _context.Products
                .Where(p => p.CompanyId == companyId)
                .ToListAsync();

            var taxRates = await GetTaxRatesForCountry(company.Country);

            var viewModel = new InvoiceFormViewModel
            {
                Heading = $"Edycja faktury {invoice.InvoiceNumber}",

                Invoice = invoice,

                Clients = clients,
                Products = products,
                TaxRates = taxRates
            };

            return View("InvoiceForm", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Prefix = "Invoice")] Invoice invoice)
        {
            var userId = User.Identity.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);


            var keys = ModelState.Keys.Where(k =>
                k.Contains("Company_") ||
                k.Contains("Client_") ||
                k.Contains("InvoiceNumber") ||
                k.Contains("Currency") ||
                k.Contains("Status")).ToList();

            foreach (var key in keys) ModelState.Remove(key);


            foreach (var key in ModelState.Keys.Where(k => k.Contains("InvoiceItems") && k.EndsWith(".Name")).ToList())
            {
                ModelState.Remove(key);
            }


            if (invoice.InvoiceItems == null || !invoice.InvoiceItems.Any())
            {
                ModelState.AddModelError("", "Faktura musi mieć co najmniej jedną pozycję.");
            }

            if (!ModelState.IsValid)
            {
                return await ReloadFormWithErrors(invoice, user.CompanyId);
            }

            var invoiceInDb = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoice.InvoiceId && i.CompanyId == user.CompanyId);

            if (invoiceInDb == null) return HttpNotFound();

            if (invoiceInDb.Status == InvoiceStatus.Paid)
            {
                // blokada edycji opłaconej faktury
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest, "Nie można edytować opłaconej faktury.");
            }

            invoiceInDb.IssueDate = invoice.IssueDate;
            invoiceInDb.SaleDate = invoice.SaleDate;
            invoiceInDb.DueDate = invoice.DueDate;
            invoiceInDb.PaymentMethod = invoice.PaymentMethod;
            invoiceInDb.Notes = invoice.Notes;

            var client = await _context.Clients.FindAsync(invoice.ClientId);
            if (client != null && client.CompanyId == user.CompanyId)
            {
                invoiceInDb.ClientId = invoice.ClientId;
                invoiceInDb.Client_Name = client.ClientName;
                invoiceInDb.Client_TaxId = client.TaxId;
                invoiceInDb.Client_Street = client.Street;
                invoiceInDb.Client_City = client.City;
                invoiceInDb.Client_PostalCode = client.PostalCode;
            }

            _context.InvoiceItems.RemoveRange(invoiceInDb.InvoiceItems);


            invoiceInDb.InvoiceItems = invoice.InvoiceItems;

            decimal grandTotalNet = 0;
            decimal grandTotalTax = 0;
            decimal grandTotalGross = 0;

            foreach (var item in invoiceInDb.InvoiceItems)
            {
                item.TotalNetAmount = item.Quantity * item.UnitPriceNet;
                item.TotalTaxAmount = item.TotalNetAmount * (item.TaxRateValue / 100m);
                item.TotalGrossAmount = item.TotalNetAmount + item.TotalTaxAmount;

                grandTotalNet += item.TotalNetAmount;
                grandTotalTax += item.TotalTaxAmount;
                grandTotalGross += item.TotalGrossAmount;


                if (string.IsNullOrEmpty(item.Name) && item.ProductId.HasValue)
                {
                    var prod = await _context.Products.FindAsync(item.ProductId);
                    if (prod != null) item.Name = prod.Name;
                }
            }


            invoiceInDb.TotalNetAmount = grandTotalNet;
            invoiceInDb.TotalTaxAmount = grandTotalTax;
            invoiceInDb.TotalGrossAmount = grandTotalGross;

                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            var userId = User.Identity.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);


            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == id && i.CompanyId == user.CompanyId);

            if (invoice == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);

            }
            invoice.Status = InvoiceStatus.Cancelled;


            _context.Invoices.Remove(invoice);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> DownloadPdf(int id)
        {
            var userId = User.Identity.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            var invoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.InvoiceId == id && i.CompanyId == user.CompanyId);

            if (invoice == null) return HttpNotFound();

            return new ViewAsPdf("InvoicePdf", invoice)
            {
                FileName = $"Faktura_{invoice.InvoiceNumber.Replace("/", "_")}.pdf"
            };
        }

        private async Task<ActionResult> ReloadFormWithErrors(Invoice invoice, int companyId)
        {
           
            var company = await _context.Companies.FindAsync(companyId);

            var viewModel = new InvoiceFormViewModel
            {
                Heading = "Nowa Faktura",
                Invoice = invoice,
                Clients = await _context.Clients.Where(c => c.CompanyId == companyId && c.IsActive).ToListAsync(),
                Products = await _context.Products.Where(p => p.CompanyId == companyId).ToListAsync(),
                // ZMIANA: Pobieranie stawek VAT na podstawie kraju firmy
                TaxRates = await GetTaxRatesForCountry(company.Country)
            };
            return View("InvoiceForm", viewModel);
        }


        private async Task<List<TaxRate>> GetTaxRatesForCountry(string countryCode)
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
