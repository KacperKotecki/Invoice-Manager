using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Invoice_Manager.Models;
using Invoice_Manager.Models.Domains;
using Invoice_Manager.Models.ViewModels;
using Microsoft.AspNet.Identity;

namespace Invoice_Manager.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ApplicationUserManager _userManager;

        public ProductController(ApplicationDbContext context, ApplicationUserManager userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Product
        public async Task<ActionResult> Index(string searchQuery = "")
        {
            var userId = User.Identity.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            var query = _context.Products
                .Include(p => p.DefaultTaxRate)
                .Where(p => p.CompanyId == user.CompanyId);

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(p =>
                    p.Name.Contains(searchQuery) ||
                    p.Description.Contains(searchQuery)
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
            var user = await _userManager.FindByIdAsync(userId);

            var company = await _context.Companies.FindAsync(user.CompanyId);

            await PopulateTaxRatesDropDownList(company.Country);

            return View(new Product());
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Product product)
        {
            var userId = User.Identity.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            product.CompanyId = user.CompanyId;

            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            var company = await _context.Companies.FindAsync(user.CompanyId);
            await PopulateTaxRatesDropDownList(company.Country, product.DefaultTaxRateId);
            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var userId = User.Identity.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id && p.CompanyId == user.CompanyId);

            if (product == null) return HttpNotFound();

            var company = await _context.Companies.FindAsync(user.CompanyId);
            await PopulateTaxRatesDropDownList(company.Country, product.DefaultTaxRateId);

            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Product product)
        {
            var userId = User.Identity.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            if (!ModelState.IsValid)
            {
                var company = await _context.Companies.FindAsync(user.CompanyId);
                await PopulateTaxRatesDropDownList(company.Country, product.DefaultTaxRateId);
                return View(product);
            }

            var productInDb = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == product.ProductId && p.CompanyId == user.CompanyId);

            if (productInDb == null) return HttpNotFound();

            productInDb.Name = product.Name;
            productInDb.Description = product.Description;
            productInDb.Unit = product.Unit;
            productInDb.UnitPriceNet = product.UnitPriceNet;
            productInDb.DefaultTaxRateId = product.DefaultTaxRateId;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: Product/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var userId = User.Identity.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

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
            var user = await _userManager.FindByIdAsync(userId);

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id && p.CompanyId == user.CompanyId);

            if (product == null) return HttpNotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        private async Task PopulateTaxRatesDropDownList(string countryCode, object selectedTaxRate = null)
        {
            var taxRatesQuery = _context.TaxRates
                .Where(t => t.Country == countryCode && t.IsActive);

            var taxRates = await taxRatesQuery.ToListAsync();

            if (!taxRates.Any())
            {
                taxRates = await _context.TaxRates
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.Rate)
                    .ToListAsync();
            }

            ViewBag.TaxRates = new SelectList(taxRates, "TaxRateId", "Name", selectedTaxRate);
        }
    }
}