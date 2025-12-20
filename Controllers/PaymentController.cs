using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Data.Entity;
using Invoice_Manager.Models;
using Invoice_Manager.Models.Domains;
using Invoice_Manager.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System.Web;
using Microsoft.AspNet.Identity.Owin;

namespace Invoice_Manager.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private ApplicationUserManager _userManager;

        // Add a public property for UserManager with a custom getter
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // Parameterless constructor for default controller instantiation
        public PaymentController()
        {
            _context = new ApplicationDbContext();
        }

        // Constructor for dependency injection (e.g., for testing)
        public PaymentController(ApplicationDbContext context, ApplicationUserManager userManager)
        {
            _context = context;
            UserManager = userManager;
        }

        // POST: Payment/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Add(PaymentFormViewModel model)
        {
            var userId = User.Identity.GetUserId();
           
            var user = await UserManager.FindByIdAsync(userId);

            if (user == null)
            {                 
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Unauthorized);
            }

            var invoice = await _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.InvoiceId == model.InvoiceId && i.CompanyId == user.CompanyId);

            if (invoice == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }

            var totalPaidSoFar = invoice.Payments.Sum(p => p.Amount);
            var remainingAmount = invoice.TotalGrossAmount - totalPaidSoFar;

            if (model.Amount > remainingAmount)
            {
                
                ModelState.AddModelError(nameof(model.Amount), $"Kwota wpłaty nie może być wyższa niż pozostała należność: {remainingAmount:C}.");

                TempData["PaymentError"] = $"Kwota wpłaty nie może być wyższa niż pozostała należność: {remainingAmount:C}.";
                return RedirectToAction("Index", "Invoice");
            }

            var payment = new Payment
            {
                InvoiceId = invoice.InvoiceId,
                PaymentDate = model.PaymentDate,
                Amount = model.Amount,
                Method = model.Method
            };

            _context.Payments.Add(payment);

            
            var newTotalPaid = totalPaidSoFar + model.Amount;

            if (newTotalPaid >= invoice.TotalGrossAmount)
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
            

            await _context.SaveChangesAsync();

            
            return RedirectToAction("Index", "Invoice");
        }

        public async Task<ActionResult> GetPaymentsForInvoice(int invoiceId)
        {
            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                   return new HttpStatusCodeResult(System.Net.HttpStatusCode.Unauthorized);
            }

            var payments = await _context.Payments
               .Include(p => p.Invoice) // Jawnie dołączamy powiązaną fakturę
               .Where(p => p.InvoiceId == invoiceId && p.Invoice.CompanyId == user.CompanyId)
               .OrderByDescending(p => p.PaymentDate)
               .ToListAsync();

            return PartialView("_PaymentList", payments);
        }

        
    }
}