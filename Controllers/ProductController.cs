using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Invoice_Manager.Models;
using Invoice_Manager.Models.Domains;
using Invoice_Manager.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace Invoice_Manager.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private ApplicationDbContext _context;
        private ApplicationUserManager _userManager;

        public ProductController()
        {
            _context = new ApplicationDbContext();
        }

        public ProductController(ApplicationDbContext context, ApplicationUserManager userManager)
        {
            _context = context;
            _userManager = userManager;
        }

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

        // GET: Product
        public async Task<ActionResult> Index(string searchQuery = "")
        {
            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);

            // 1. Podstawowe zapytanie
            // Używamy .Include(p => p.DefaultTaxRate), żeby pobrać też nazwę stawki VAT (np. "23%")
            var query = _context.Products
                .Include(p => p.DefaultTaxRate)
                .Where(p => p.CompanyId == user.CompanyId);

            // 2. Obsługa wyszukiwania
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(p =>
                    p.Name.Contains(searchQuery) ||
                    p.Description.Contains(searchQuery)
                // Wyszukiwanie po cenie/liczbach w SQL bywa trudne przez .ToString(), 
                // na razie skupmy się na tekstach dla bezpieczeństwa.
                );
            }

            var vm = new ProductIndexViewModel
            {
                Products = await query.ToListAsync(),
                SearchQuery = searchQuery
            };

            return View(vm);
        }

        // GET: Product/Create
        public async Task<ActionResult> Create()
        {
            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);

            // Przygotuj listę stawek VAT do dropdowna
            await PopulateTaxRatesDropDownList(user.CompanyId);

            return View(new Product());
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Product product)
        {
            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);

            product.CompanyId = user.CompanyId;
            // Produkt nie ma pola IsActive w naszym modelu, więc go nie ustawiamy.

            if (ModelState.IsValid)
            {
                _context.Products.Add(product); // Zmieniono Clients na Products!
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            // Jeśli błąd, musimy ponownie załadować listę VAT, bo ViewBag znika po przeładowaniu
            await PopulateTaxRatesDropDownList(user.CompanyId, product.DefaultTaxRateId);
            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id && p.CompanyId == user.CompanyId);

            if (product == null) return HttpNotFound();

            // Przygotuj listę stawek VAT i zaznacz aktualną
            await PopulateTaxRatesDropDownList(user.CompanyId, product.DefaultTaxRateId);

            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Product product)
        {
            if (!ModelState.IsValid)
            {
                // Jeśli błąd, odnów listę VAT
                var userId = User.Identity.GetUserId();
                var user = await UserManager.FindByIdAsync(userId);
                await PopulateTaxRatesDropDownList(user.CompanyId, product.DefaultTaxRateId);
                return View(product);
            }

            // Logika zapisu
            var userIdForSave = User.Identity.GetUserId();
            var userForSave = await UserManager.FindByIdAsync(userIdForSave);

            var productInDb = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == product.ProductId && p.CompanyId == userForSave.CompanyId);

            if (productInDb == null) return HttpNotFound();

            // Ręczne mapowanie (Aktualizacja pól)
            productInDb.Name = product.Name;
            productInDb.Description = product.Description;
            productInDb.Unit = product.Unit;
            productInDb.UnitPriceNet = product.UnitPriceNet;
            productInDb.DefaultTaxRateId = product.DefaultTaxRateId; // Zmiana stawki VAT

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: Product/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);

            var product = await _context.Products
                .Include(p => p.DefaultTaxRate)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.CompanyId == user.CompanyId);

            if (product == null) return HttpNotFound();

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id && p.CompanyId == user.CompanyId);

            if (product == null) return HttpNotFound();

            // HARD DELETE (Fizyczne usunięcie)
            // Jeśli chcesz Soft Delete, musisz dodać pole IsActive do modelu Product
            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // --- METODY POMOCNICZE (Helpers) ---

        // Ta metoda pobiera stawki VAT dla danej firmy i wrzuca je do ViewBag
        // Dzięki temu w widoku będziemy mogli użyć: @Html.DropDownListFor(...)
        private async Task PopulateTaxRatesDropDownList(int companyId, object selectedTaxRate = null)
        {
            var taxRatesQuery = _context.TaxRates
                .Where(t => t.CompanyId == companyId)
                .OrderBy(t => t.Rate); // Sortuj np. od najmniejszej stawki

            var taxRates = await taxRatesQuery.ToListAsync();

            // "TaxRateId" -> to co zapisujemy w bazie (Value)
            // "Name" -> to co widzi użytkownik (Text) np. "VAT 23%"
            ViewBag.TaxRates = new SelectList(taxRates, "TaxRateId", "Name", selectedTaxRate);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_context != null)
                {
                    _context.Dispose();
                    _context = null;
                }
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}