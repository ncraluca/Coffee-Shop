using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProiectV1.Data;
using ProiectV1.Models;

namespace ProiectV1.Controllers
{
    public class CommentsController : Controller
    {

        // PASUL 10 - useri si roluri


        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public CommentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }

        /*
        
        // Adaugarea unui comentariu asociat unui articol in baza de date
        [HttpPost]
        public IActionResult New(Comment comm)
        {
            comm.Date = DateTime.Now;

            if(ModelState.IsValid)
            {
                db.Comments.Add(comm);
                db.SaveChanges();
                return Redirect("/Articles/Show/" + comm.ArticleId);
            }

            else
            {
                return Redirect("/Articles/Show/" + comm.ArticleId);
            }

        }

        
        */


        // Stergerea unui comentariu asociat unui articol din baza de date
        // Se poate sterge comentariul doar de catre userii cu rolul Admin
        // sau de catre userii cu rolul User sau Editor doar daca comentariul 
        // a fost lasat de acestia
        [HttpPost]
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Delete(int id)
        {
            Comment comm = db.Comments.Find(id);

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Comments.Remove(comm);
                db.SaveChanges();
                return Redirect("/Products/Show/" + comm.ProductId);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti comentariul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Products");
            }
        }

        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Edit(int id)
        {

            Comment comment = db.Comments.Find(id);

            if (comment.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(comment);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui comentariu care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

        }
        [HttpPost]
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Edit(int id, Comment newcomment)
        {


            Comment comment = db.Comments.Find(id);


            if (ModelState.IsValid)
            {
                if (comment.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    comment.Text = newcomment.Text;
                    comment.Stars = newcomment.Stars;
                    db.SaveChanges();
                    TempData["message"] = "Comentariul a fost modificat";
                    TempData["messageType"] = "alert-success";
                    return RedirectToAction("Show", "Products", new { id = id });

                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui comentariu care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Show", "Products", new { id = id });

                }
            }
            else
            {
                return View(comment);
            }


        }
    }
}
