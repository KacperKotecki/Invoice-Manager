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
        public async Task<ActionResult> Create()
        {
            var userId = User.Identity.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            var companyId = user.CompanyId;

            // Pobierz dane potrzebne do dropdownów
            var clients = await _context.Clients
                .Where(c => c.CompanyId == companyId && c.IsActive)
                .ToListAsync();

            var products = await _context.Products
                .Where(p => p.CompanyId == companyId)
                .ToListAsync();

            var taxRates = await _context.TaxRates
               .Where(t => t.CompanyId == companyId)
               .ToListAsync();

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

            return View("InvoiceForm", viewModel); // Użyjemy wspólnego widoku dla Create i Edit
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Prefix = "Invoice")] Invoice invoice)
        {
            var userId = User.Identity.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            // ========================================================================
            // 1. INTELIGENTNE CZYSZCZENIE WALIDACJI
            // ========================================================================

            // Pobieramy wszystkie klucze błędów do listy
            var keys = ModelState.Keys.ToList();

            foreach (var key in keys)
            {
                // Jeśli błąd dotyczy pól, które i tak uzupełniamy automatycznie - USUŃ GO.
                // Używamy EndsWith, żeby zadziałało zarówno dla "Currency" jak i "Invoice.Currency"
                if (key.EndsWith("InvoiceNumber") ||
                    key.EndsWith("Currency") ||
                    key.EndsWith("Status") ||
                    key.Contains("Company_") ||  // Wszystkie pola Company_Name, Company_Street itd.
                    key.Contains("Client_") ||   // Wszystkie pola Client_Name, Client_Street itd.
                    key.EndsWith("Company") ||   // Obiekt nawigacyjny
                    key.EndsWith("Client"))      // Obiekt nawigacyjny
                {
                    ModelState.Remove(key);
                }

                // Dodatkowo czyścimy błędy nazw produktów w liście (bo uzupełniamy je z bazy)
                if (key.Contains("InvoiceItems") && key.EndsWith(".Name"))
                {
                    ModelState.Remove(key);
                }
            }

            // ========================================================================

            // 2. Walidacja pozycji (czy jest chociaż jedna?)
            if (invoice.InvoiceItems == null || !invoice.InvoiceItems.Any())
            {
                ModelState.AddModelError("", "Faktura musi mieć co najmniej jedną pozycję.");
            }

            // SPRAWDZENIE FINALNE
            if (!ModelState.IsValid)
            {
                // Jeśli NADAL są błędy (np. brak ClientId albo złe daty), wróć do formularza
                return await ReloadFormWithErrors(invoice, user.CompanyId);
            }

            // ========================================================================
            // 3. UZUPEŁNIANIE DANYCH (BACKEND)
            // ========================================================================

            invoice.CompanyId = user.CompanyId;
            invoice.Status = InvoiceStatus.Draft;

            // Ręczne ustawienie waluty (bo usunęliśmy błąd walidacji, więc musimy nadać wartość!)
            if (string.IsNullOrEmpty(invoice.Currency))
            {
                invoice.Currency = "PLN";
            }

            // Generowanie numeru faktury
            var count = await _context.Invoices.CountAsync(i => i.CompanyId == user.CompanyId) + 1;
            invoice.InvoiceNumber = $"FV/{DateTime.Now.Year}/{DateTime.Now.Month:D2}/{count}";

            // --- SNAPSHOTTING FIRMY ---
            var company = await _context.Companies.FindAsync(user.CompanyId);
            if (company == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest, "Brak profilu firmy");

            invoice.Company_Name = company.CompanyName;
            invoice.Company_TaxId = company.TaxId;
            invoice.Company_Street = company.Street;
            invoice.Company_City = company.City;
            invoice.Company_PostalCode = company.PostalCode;
            invoice.Company_BankAccount = company.BankAccount;

            // --- SNAPSHOTTING KLIENTA ---
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

            // --- MATEMATYKA SERWEROWA ---
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

                // Uzupełnij nazwę z produktu
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
        // Metoda pomocnicza do przeładowania formularza w razie błędu
        private async Task<ActionResult> ReloadFormWithErrors(Invoice invoice, int companyId)
        {
            var viewModel = new InvoiceFormViewModel
            {
                Heading = "Nowa Faktura",
                Invoice = invoice,
                Clients = await _context.Clients.Where(c => c.CompanyId == companyId && c.IsActive).ToListAsync(),
                Products = await _context.Products.Where(p => p.CompanyId == companyId).ToListAsync(),
                TaxRates = await _context.TaxRates.Where(t => t.CompanyId == companyId).ToListAsync()
            };
            return View("InvoiceForm", viewModel);
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
