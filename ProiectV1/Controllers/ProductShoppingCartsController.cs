using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProiectV1.Data;
using ProiectV1.Models;
using System.Security.Claims;

namespace ProiectV1.Controllers
{
    public class ProductShoppingCartsController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public ProductShoppingCartsController(
                ApplicationDbContext context,
                UserManager<ApplicationUser> userManager,
                RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "User")]
        public IActionResult Index()
        {
            string currentUserId = _userManager.GetUserId(User);

            var productscart = db.ProductShoppingCarts.Include("Product").Include("ShoppingCart").Where(g => g.ShoppingCart.UserId == currentUserId);

            ViewBag.ProductShoppingCarts = productscart; //stocam produsele

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }


            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public IActionResult Delete(int id)
        {
            ProductShoppingCart prod = db.ProductShoppingCarts.Include("ShoppingCart")
                                             .Where(c => c.Id == id)
                                             .First();

            if (prod.ShoppingCart.UserId == _userManager.GetUserId(User))
            {
                db.ProductShoppingCarts.Remove(prod);
                db.SaveChanges();
                TempData["message"] = "Produsul a fost sters";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti produsul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }
    }
}
