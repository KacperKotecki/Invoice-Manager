using Invoice_Manager.Filters;
using Invoice_Manager.Models;
using Invoice_Manager.Models.Domains;
using Invoice_Manager.Models.ViewModels;
using Invoice_Manager.Repositories;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.Cookies;
using Rotativa;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.Entity;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Protocols;

namespace Invoice_Manager.Controllers
{
    [CheckProfileCompletion]    
    [Authorize]
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ApplicationUserManager _userManager;
        private readonly InvoiceRepository _invoiceRepository;
        private readonly ClientRepository _clientRepository;
        private readonly CompanyRepository _companyRepository;
        private readonly ProductRepository _productRepository;
        private readonly TaxRateRepository _taxRateRepository;
        public InvoiceController(
            ApplicationDbContext context,
            ApplicationUserManager userManager,
            InvoiceRepository invoiceRepository,
            ClientRepository clientRepository,
            CompanyRepository companyRepository,
            ProductRepository productRepository,
            TaxRateRepository taxRateRepository)
        {
            _context = context;
            _userManager = userManager;
            _invoiceRepository = invoiceRepository;
            _clientRepository = clientRepository;
            _companyRepository = companyRepository;
            _productRepository = productRepository;
            _taxRateRepository = taxRateRepository;
        }
        public async Task<ActionResult> Index(string searchQuery = null, InvoiceStatus? status = null)
        {
            var companyId = await GetCurrentCompanyIdAsync();
            var invoices = _invoiceRepository.GetInvoicesWithFilters(companyId, searchQuery, status);
            var viewModel = await PrepareDashboardViewModel(invoices, companyId, searchQuery, status);
            
            return View(viewModel);
        }
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            var viewModel = await PrepareInvoiceFormViewModel(); 
            return View("Manage", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Prefix = "Invoice")] Invoice invoice)
        {
            RemoveSnapshotValidationErrors();

            if (!ModelState.IsValid)
            {
                var vm = await PrepareInvoiceFormViewModel(invoice);
                return View("Manage", vm);
            }

            var companyId = await GetCurrentCompanyIdAsync();
            var company = await _companyRepository.GetCompanyByIdAsync(companyId);
            var client = await _clientRepository.GetClientByIdAsync(invoice.ClientId);

            _invoiceRepository.UpdateInvoiceSnapshots(invoice, company, client);

            await _invoiceRepository.CalculateInvoiceTotals(invoice);

            invoice.Status = InvoiceStatus.Draft;

            var count = await _invoiceRepository.GetNextInvoiceNumberAsync(companyId);
            invoice.InvoiceNumber = $"FV/{DateTime.Now.Year}/{DateTime.Now.Month:D2}/{count}";

            try
            {
                _invoiceRepository.Add(invoice);

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Wystąpił błąd podczas zapisywania faktury. Spróbuj ponownie." + ex);
                var vm = await PrepareInvoiceFormViewModel(invoice);
                return View("Manage", vm);
            }
        }
        [HttpGet]
        public async Task<ActionResult> Edit(int invoiceId)
        {
            var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);
            var companyId = await GetCurrentCompanyIdAsync();

            if (invoice == null || invoice.CompanyId != companyId) return HttpNotFound();
            if (invoice.Status == InvoiceStatus.Paid) return RedirectToAction("Index");

            var viewModel = await PrepareInvoiceFormViewModel(invoice);
            return View("Manage", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Prefix = "Invoice")] Invoice invoice)
        {
            RemoveSnapshotValidationErrors();

            if (!ModelState.IsValid)
            {
                var vm = await PrepareInvoiceFormViewModel(invoice);
                return View("Manage", vm);
            }

            var companyId = await GetCurrentCompanyIdAsync();
            var company = await _companyRepository.GetCompanyByIdAsync(companyId);
            var client = await _clientRepository.GetClientByIdAsync(invoice.ClientId);

            _invoiceRepository.UpdateInvoiceSnapshots(invoice, company, client);

            await _invoiceRepository.CalculateInvoiceTotals(invoice);

            try
            {
                await _invoiceRepository.UpdateInvoiceAsync(invoice, company, client);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Błąd edycji: " + ex.Message);
                return View("Manage", await PrepareInvoiceFormViewModel(invoice));
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int invoiceId)
        {
            var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);

            if (invoice == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);

            invoice.Status = InvoiceStatus.Cancelled;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        private async Task<int> GetCurrentCompanyIdAsync()
        {
            var userId = User.Identity.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            return user.CompanyId;
        }
        private async Task<DashboardViewModel> PrepareDashboardViewModel(List<Invoice> invoices, int companyId, string searchQuery, InvoiceStatus? status)
        {

            var stats = new DashboardStatsViewModel
            {
                AmountToCollect = await _invoiceRepository.AmountToCollect(companyId),
                AmountCollectedThisMonth = await _invoiceRepository.AmountCollectedThisMonth(companyId),
                OverdueCount = await _invoiceRepository.OverdueInvoicesCount(companyId)
            };

            var temp = PrepareInvoiceCardViewModel(invoices);


            return new DashboardViewModel
            {
                Invoices = PrepareInvoiceCardViewModel(invoices),
                Stats = stats,
                SearchQuery = searchQuery,
                ActiveFilter = status
            };

        }


        private List<InvoiceCardViewModel> PrepareInvoiceCardViewModel(List<Invoice> invoices)
        {
            var ListinvoiceCardViewModels = new List<InvoiceCardViewModel>();

            foreach (var invoice in invoices)
            {
                var invoiceCardViewModel = new InvoiceCardViewModel
                {
                    InvoiceId = invoice.InvoiceId,
                    InvoiceNumber = invoice.InvoiceNumber,
                    ClientName = invoice.Client_Name,
                    TotalGrossAmount = invoice.TotalGrossAmount,
                    Currency = invoice.Currency,
                    Status = invoice.Status,
                    DueDate = invoice.DueDate,
                    Payments = invoice.Payments
                };
                ListinvoiceCardViewModels.Add(invoiceCardViewModel);
            }
            return ListinvoiceCardViewModels;

        }

        private async Task<InvoiceFormViewModel> PrepareInvoiceFormViewModel(Invoice invoice = null)
        {

            var heading = invoice == null ? "Nowa Faktura" : $"Edycja faktury {invoice.InvoiceNumber}";

            var invoiceObj = invoice ?? new Invoice
            {
                IssueDate = DateTime.Now,
                SaleDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                Currency = "PLN",
                PaymentMethod = "Przelew"
            };

            var companyId = await GetCurrentCompanyIdAsync();

            var company = await _companyRepository.GetCompanyByIdAsync(companyId);
            var clients = await _clientRepository.GetActiveClientsAsync(companyId);
            var products = await _productRepository.GetProductsForCompanyAsync(companyId);
            var taxRates = await _taxRateRepository.GetTaxRatesByCountryAsync(company.Country);

            return new InvoiceFormViewModel
            {
                Heading = heading,
                Invoice = invoiceObj,
                Clients = clients,
                Products = products,
                TaxRates = taxRates
            };
        }
        private void RemoveSnapshotValidationErrors()
        {
            ModelState.Remove("Invoice.InvoiceNumber");

            // Company Snapshot Fields
            ModelState.Remove("Invoice.Company_Name");
            ModelState.Remove("Invoice.Company_TaxId");
            ModelState.Remove("Invoice.Company_Street");
            ModelState.Remove("Invoice.Company_City");
            ModelState.Remove("Invoice.Company_PostalCode");
            ModelState.Remove("Invoice.Company_BankName");
            ModelState.Remove("Invoice.Company_BankAccount");

            // Client Snapshot Fields
            ModelState.Remove("Invoice.Client_Name");
            ModelState.Remove("Invoice.Client_Street");
            ModelState.Remove("Invoice.Client_City");
            ModelState.Remove("Invoice.Client_PostalCode");
        }
    }
}
