using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Invoice_Manager.Models;
using Invoice_Manager.Models.Domains;
using Invoice_Manager.Models.ViewModels;
using Invoice_Manager.Repositories; // Dodano namespace
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace Invoice_Manager.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        // Zamiast DbContext, wstrzykujemy Repozytorium
        private readonly PaymentRepository _paymentRepository;
        private readonly ApplicationUserManager _userManager;

        public PaymentController(PaymentRepository paymentRepository, ApplicationUserManager userManager)
        {
            _paymentRepository = paymentRepository;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Add(PaymentFormViewModel model)
        {
            var user = await GetCurrentUser();
            if (user == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.Unauthorized);

            var invoice = await _paymentRepository.GetInvoiceWithPaymentsAsync(model.InvoiceId, user.CompanyId);

            if (invoice == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);

            var totalPaidSoFar = invoice.Payments.Sum(p => p.Amount);
            var remainingAmount = invoice.TotalGrossAmount - totalPaidSoFar;

            if (model.Amount > remainingAmount)
            {
                TempData["PaymentError"] = $"Kwota wpłaty ({model.Amount:C}) nie może być wyższa niż pozostała należność: {remainingAmount:C}.";
                return RedirectToAction("Index", "Invoice");
            }

            var payment = new Payment
            {
                InvoiceId = invoice.InvoiceId,
                PaymentDate = model.PaymentDate,
                Amount = model.Amount,
                Method = model.Method
            };

            _paymentRepository.AddPayment(payment);
            
            UpdateInvoiceStatus(invoice, totalPaidSoFar + model.Amount);

            await _paymentRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "Płatność została pomyślnie dodana.";
            return RedirectToAction("Index", "Invoice");
        }

        public async Task<ActionResult> GetPaymentsForInvoice(int invoiceId)
        {
            var user = await GetCurrentUser();
            if (user == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.Unauthorized);

            var invoice = await _paymentRepository.GetInvoiceWithPaymentsAsync(invoiceId, user.CompanyId);

            if (invoice == null) return HttpNotFound();

            var payments = invoice.Payments.OrderByDescending(p => p.PaymentDate).ToList();

            var viewModel = new PaymentModalViewModel
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                TotalGrossAmount = invoice.TotalGrossAmount,
                TotalPaid = payments.Sum(p => p.Amount),
                Payments = payments
            };

            return PartialView("_PaymentModalContent", viewModel);
        }

        // --- Metody Pomocnicze (Private Helpers) ---

        private void UpdateInvoiceStatus(Invoice invoice, decimal totalPaid)
        {
            if (totalPaid >= invoice.TotalGrossAmount)
            {
                invoice.Status = InvoiceStatus.Paid;
            }
            else if (invoice.DueDate < System.DateTime.Now)
            {
                invoice.Status = InvoiceStatus.Overdue;
            }
            else
            {
                invoice.Status = InvoiceStatus.Sent;
            }
        }

        private async Task<ApplicationUser> GetCurrentUser()
        {
            var userId = User.Identity.GetUserId();
            return await _userManager.FindByIdAsync(userId);
        }
    }
}