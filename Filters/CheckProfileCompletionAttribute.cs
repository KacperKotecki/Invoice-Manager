using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Invoice_Manager.Filters
{
    public class CheckProfileCompletionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Jeœli u¿ytkownik nie jest zalogowany, nic nie rób. [Authorize] go obs³u¿y.
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            // Nie uruchamiaj tego filtra dla akcji CompanyProfile, aby unikn¹æ pêtli przekierowañ.
            if (filterContext.ActionDescriptor.ActionName == "CompanyProfile" &&
                filterContext.ActionDescriptor.ControllerDescriptor.ControllerName == "Manage")
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            var httpContext = filterContext.HttpContext;
            var userId = httpContext.User.Identity.GetUserId();
            var userManager = httpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var user = userManager.Users
                                  .Include(u => u.Company)
                                  .SingleOrDefault(u => u.Id == userId);

            if (user?.Company == null)
            {
                // Przekieruj, jeœli nie ma firmy
                RedirectToProfile(filterContext);
                return;
            }

            var company = user.Company;
            bool isProfileDataComplete = !string.IsNullOrWhiteSpace(company.CompanyName) &&
                                         !string.IsNullOrWhiteSpace(company.TaxId) &&
                                         !string.IsNullOrWhiteSpace(company.Street) &&
                                         !string.IsNullOrWhiteSpace(company.City) &&
                                         !string.IsNullOrWhiteSpace(company.PostalCode) &&
                                         !string.IsNullOrWhiteSpace(company.Country) &&
                                         !string.IsNullOrWhiteSpace(company.BankName) &&
                                         !string.IsNullOrWhiteSpace(company.BankAccount);

            if (!isProfileDataComplete)
            {
                // Przekieruj, jeœli dane s¹ niekompletne
                RedirectToProfile(filterContext);
            }
            else
            {
                // Wszystko jest w porz¹dku, kontynuuj normalnie.
                base.OnActionExecuting(filterContext);
            }
        }

        private void RedirectToProfile(ActionExecutingContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                new System.Web.Routing.RouteValueDictionary
                {
                    { "controller", "Manage" },
                    { "action", "CompanyProfile" }
                });
        }
    }
}