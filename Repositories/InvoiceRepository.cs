using Invoice_Manager.Models;
using Invoice_Manager.Models.Domains;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Invoice_Manager.Repositories
{
    public class InvoiceRepository
    {
        private readonly ApplicationDbContext _context;

        public InvoiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Invoice> GetInvoiceByIdAsync(int invoiceId)
        {
            return await _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.Payments)
                .Include(i => i.InvoiceItems)
                .Include(i => i.Company)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
        }


        public List<Invoice> GetInvoices(int companyId)
        {
            return _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.Payments)
                .Include(i => i.InvoiceItems)
                .Include(i => i.Company)
                .Include(i => i.Client)
                .Where(i => i.CompanyId == companyId)
                .OrderByDescending(i => i.InvoiceId)
                .ToList();
        }

        public List<Invoice> GetInvoicesWithFilters(int companyId, string searchQuery = null, InvoiceStatus? status = null)
        {
            var invoices = _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.Payments)
                .Where(i => i.CompanyId == companyId);

            if (!string.IsNullOrEmpty(searchQuery))
            {
                invoices = invoices.Where(i =>
                    i.InvoiceNumber.Contains(searchQuery) ||
                    i.Client_Name.Contains(searchQuery) ||
                    i.Client_TaxId.Contains(searchQuery));
            }

            if (status.HasValue)
            {
                invoices = invoices.Where(i => i.Status == status.Value);
            }
            return invoices
                  .OrderByDescending(i => i.InvoiceId)
                  .ToList();
        }
        public void UpdateInvoiceSnapshots(Invoice invoice,Company company,Client client)
        {
            invoice.CompanyId = company.CompanyId;

            invoice.Client_Name = client.ClientName;
            invoice.Client_TaxId = client.TaxId;
            invoice.Client_Street = client.Street;
            invoice.Client_City = client.City;
            invoice.Client_PostalCode = client.PostalCode;

            invoice.Company_Name = company.CompanyName;
            invoice.Company_TaxId = company.TaxId;
            invoice.Company_Street = company.Street;
            invoice.Company_City = company.City;
            invoice.Company_PostalCode = company.PostalCode;
            invoice.Company_BankName = company.BankName;
            invoice.Company_BankAccount = company.BankAccount;
        }
        public void Add(Invoice invoice)
        {
            _context.Invoices.Add(invoice);
        }

        public async Task UpdateInvoiceAsync(Invoice invoiceFromForm, Company company, Client client)
        {
            var companyId = company.CompanyId;

            var invoiceInDb = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceFromForm.InvoiceId && i.CompanyId == companyId);

            if (invoiceInDb == null) return;

            invoiceInDb.ClientId = invoiceFromForm.ClientId;
            invoiceInDb.IssueDate = invoiceFromForm.IssueDate;
            invoiceInDb.DueDate = invoiceFromForm.DueDate;
            invoiceInDb.SaleDate = invoiceFromForm.SaleDate;
            invoiceInDb.PaymentMethod = invoiceFromForm.PaymentMethod;
            invoiceInDb.Currency = invoiceFromForm.Currency;
            invoiceInDb.Notes = invoiceFromForm.Notes;

            invoiceInDb.Client_Name = invoiceFromForm.Client_Name;
            invoiceInDb.Client_TaxId = invoiceFromForm.Client_TaxId;
            invoiceInDb.Client_Street = invoiceFromForm.Client_Street;
            invoiceInDb.Client_City = invoiceFromForm.Client_City;
            invoiceInDb.Client_PostalCode = invoiceFromForm.Client_PostalCode;

            invoiceInDb.TotalNetAmount = invoiceFromForm.TotalNetAmount;
            invoiceInDb.TotalTaxAmount = invoiceFromForm.TotalTaxAmount;
            invoiceInDb.TotalGrossAmount = invoiceFromForm.TotalGrossAmount;

            _context.InvoiceItems.RemoveRange(invoiceInDb.InvoiceItems);

            foreach (var item in invoiceFromForm.InvoiceItems)
            {
                item.InvoiceItemId = 0;
                invoiceInDb.InvoiceItems.Add(item);
            }

        }
        public async Task CalculateInvoiceTotals(Invoice invoice)
        {
            decimal grandTotalNet = 0;
            decimal grandTotalTax = 0;
            decimal grandTotalGross = 0;

            foreach (var item in invoice.InvoiceItems)
            {
                // Pobranie nazwy produktu jeśli pusta (np. wybrano z listy)
                if (string.IsNullOrEmpty(item.Name) && item.ProductId.HasValue)
                {
                    var prod = await _context.Products.FindAsync(item.ProductId);
                    if (prod != null) item.Name = prod.Name;
                }

                item.TotalNetAmount = item.Quantity * item.UnitPriceNet;
                item.TotalTaxAmount = item.TotalNetAmount * (item.TaxRateValue / 100m);
                item.TotalGrossAmount = item.TotalNetAmount + item.TotalTaxAmount;

                grandTotalNet += item.TotalNetAmount;
                grandTotalTax += item.TotalTaxAmount;
                grandTotalGross += item.TotalGrossAmount;
            }

            invoice.TotalNetAmount = grandTotalNet;
            invoice.TotalTaxAmount = grandTotalTax;
            invoice.TotalGrossAmount = grandTotalGross;
        }

        public async Task<decimal> AmountToCollect(int companyId)
        {
            var amountToCollect = await _context.Invoices
                .Where(i => i.CompanyId == companyId && (i.Status == InvoiceStatus.Sent || i.Status == InvoiceStatus.Overdue))
                .SumAsync(i => (decimal?)i.TotalGrossAmount) ?? 0;
            return amountToCollect;
        }

        public async Task<decimal> AmountCollectedThisMonth(int companyId)
        {
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var amountCollectedThisMonth = await _context.Payments
                .Where(p => p.Invoice.CompanyId == companyId && p.PaymentDate >= thirtyDaysAgo)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;
            return amountCollectedThisMonth;
        }

        public async Task<int> OverdueInvoicesCount(int companyId)
        {
            var overdueCount = await _context.Invoices
                .CountAsync(i => i.CompanyId == companyId && i.Status == InvoiceStatus.Overdue);
            return overdueCount;
        }



        public async Task<int> GetNextInvoiceNumberAsync(int companyId)
        {
            var count = await _context.Invoices
                .CountAsync(i => i.CompanyId == companyId);
            return count + 1;
        }
    }
}