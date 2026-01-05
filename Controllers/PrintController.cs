using Invoice_Manager.Filters;
using Invoice_Manager.Models;
using Invoice_Manager.Models.Domains;
using Invoice_Manager.Repositories;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Rotativa;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Invoice_Manager.Controllers
{
    [CheckProfileCompletion]
    [Authorize]
    public class PrintController : Controller
    {
        private readonly ApplicationUserManager _userManager;
        private readonly InvoiceRepository _invoiceRepository;

        public PrintController(ApplicationUserManager userManager, InvoiceRepository invoiceRepository)
        {
            _userManager = userManager;
            _invoiceRepository = invoiceRepository;
        }
        public async Task<ActionResult> DownloadPdf(int invoiceId)
        {
            var invoice = await GetValidInvoiceAsync(invoiceId);

            if (invoice == null) return HttpNotFound();

            return new ViewAsPdf("InvoicePdf", invoice)
            {
                FileName = $"Faktura_{invoice.InvoiceNumber.Replace("/", "_")}.pdf"
            };
        }

        public async Task<ActionResult> ShowPdf(int invoiceId)
        {
            var invoice = await GetValidInvoiceAsync(invoiceId);

            if (invoice == null) return HttpNotFound();

            return new ViewAsPdf("InvoicePdf", invoice);
            
        }
        private async Task<Invoice> GetValidInvoiceAsync(int invoiceId)
        {
            var userId = User.Identity.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);

            if (invoice == null || invoice.CompanyId != user.CompanyId)
                return null;

            return invoice;
        }
    }
}