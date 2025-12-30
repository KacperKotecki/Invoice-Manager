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
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ApplicationUserManager _userManager;

        public ClientController(ApplicationDbContext context, ApplicationUserManager userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Client
        public async Task<ActionResult> Index(string searchQuery = "")
        {
            var userId = User.Identity.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            var query = _context.Clients
                .Where(c => c.CompanyId == user.CompanyId && c.IsActive);

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(c =>
                    c.ClientName.Contains(searchQuery) ||
                    c.TaxId.Contains(searchQuery) ||
                    c.Email.Contains(searchQuery));
            }

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
            var user = await _userManager.FindByIdAsync(userId);

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
            var user = await _userManager.FindByIdAsync(userId);

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
            var user = await _userManager.FindByIdAsync(userId);

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
            var user = await _userManager.FindByIdAsync(userId);

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
            var user = await _userManager.FindByIdAsync(userId);

            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.ClientId == id && c.CompanyId == user.CompanyId);

            if (client == null) return HttpNotFound();

            client.IsActive = false;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}