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
    public class ClientController : Controller
    {
        // 1. Usuwamy 'readonly', bo będziemy to ustawiać dynamicznie
        private ApplicationDbContext _context;
        private ApplicationUserManager _userManager;

        // 2. KONSTRUKTOR BEZPARAMETROWY (Dla MVC)
        // To ten konstruktor jest wywoływany, gdy uruchamiasz aplikację
        public ClientController()
        {
            _context = new ApplicationDbContext();
        }

        // 3. KONSTRUKTOR Z PARAMETRAMI (Dla Testów)
        // Pozwala wstrzyknąć dane, gdybyś pisał testy jednostkowe
        public ClientController(ApplicationDbContext context, ApplicationUserManager userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 4. PROPERTY DLA USER MANAGERA (Kluczowy element Identity w MVC 5)
        // Ten mechanizm mówi: "Jeśli _userManager jest null, pobierz go z OwinContext. Jeśli już jest, użyj tego co jest".
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

        // GET: Client
        public async Task<ActionResult> Index(string searchQuery = "")
        {
            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);

            // 1. Podstawowe zapytanie (bezpieczeństwo + soft delete)
            var query = _context.Clients
                .Where(c => c.CompanyId == user.CompanyId && c.IsActive);

            // 2. Obsługa wyszukiwania (jeśli użytkownik coś wpisał)
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(c =>
                    c.ClientName.Contains(searchQuery) ||
                    c.TaxId.Contains(searchQuery) ||
                    c.Email.Contains(searchQuery));
            }

            // 3. Pobranie danych i utworzenie ViewModelu
            var vm = new ClientIndexViewModel
            {
                Clients = await query.ToListAsync(),
                SearchQuery = searchQuery
            };

            return View(vm);
        }
        // GET: Client/Create
        public ActionResult Create()
        {
            return View(new Client());
        }

        // POST: Client/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Client client)
        {
            var userId = User.Identity.GetUserId();
            // Zmieniamy na UserManager
            var user = await UserManager.FindByIdAsync(userId);

            client.CompanyId = user.CompanyId;
            client.IsActive = true;

            if (ModelState.IsValid)
            {
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(client);
        }

        // GET: Client/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var userId = User.Identity.GetUserId();
            // Zmieniamy na UserManager
            var user = await UserManager.FindByIdAsync(userId);

            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.ClientId == id && c.CompanyId == user.CompanyId);

            if (client == null) return HttpNotFound();

            return View(client);
        }

        // POST: Client/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Client client)
        {
            if (!ModelState.IsValid) return View(client);

            var userId = User.Identity.GetUserId();
            // Zmieniamy na UserManager
            var user = await UserManager.FindByIdAsync(userId);

            var clientInDb = await _context.Clients
                .FirstOrDefaultAsync(c => c.ClientId == client.ClientId && c.CompanyId == user.CompanyId);

            if (clientInDb == null) return HttpNotFound();

            clientInDb.ClientName = client.ClientName;
            clientInDb.TaxId = client.TaxId;
            clientInDb.Street = client.Street;
            clientInDb.City = client.City;
            clientInDb.PostalCode = client.PostalCode;
            clientInDb.Country = client.Country;
            clientInDb.Email = client.Email;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: Client/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var userId = User.Identity.GetUserId();
            // Zmieniamy na UserManager
            var user = await UserManager.FindByIdAsync(userId);

            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.ClientId == id && c.CompanyId == user.CompanyId);

            if (client == null) return HttpNotFound();

            return View(client);
        }

        // POST: Client/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var userId = User.Identity.GetUserId();
            // Zmieniamy na UserManager
            var user = await UserManager.FindByIdAsync(userId);

            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.ClientId == id && c.CompanyId == user.CompanyId);

            if (client == null) return HttpNotFound();

            client.IsActive = false;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Ważne: zamykamy Context
                if (_context != null)
                {
                    _context.Dispose();
                    _context = null;
                }
                // UserManager też warto zamknąć jeśli był używany
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