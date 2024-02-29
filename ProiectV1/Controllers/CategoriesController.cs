using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProiectV1.Data;
using ProiectV1.Models;

namespace ProiectV1.Controllers
{
    //dam dreptul doar adminului pe tot controller-ul (Deoarece toate metodele din controller ar fi tb sa aiba authorie doar pe admin)
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public CategoriesController(
                ApplicationDbContext context,
                UserManager<ApplicationUser> userManager,
                RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        
        //ceea ce vede userul(adica adminul) in pagina categories/index (view-ul principal)
        public IActionResult Index()
        {
            // Alegem sa afisam 5 categorii pe pagina
            int _perPage = 5;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            var categories = from category in db.Categories
                             orderby category.CategoryName
                             select category;  //filtram categoriile sortate crescator
            ViewBag.Categories = categories; //stocam categoriile pt a le putea adauga in view


            // Fiind un numar variabil de categorii, verificam de fiecare data utilizand metoda Count()
            int totalItems = categories.Count();

            // Se preia pagina curenta din View-ul asociat
            // Numarul paginii este valoarea parametrului page din ruta /Categories/Index?page=valoare
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            // Pentru prima pagina offsetul o sa fie zero
            // Pentru pagina 2 o sa fie 3
            // Asadar offsetul este egal cu numarul de categorii care au fost deja afisate pe paginile anterioare
            var offset = 0;

            // Se calculeaza offsetul in functie de numarul paginii la care suntem
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            // Se preiau categoriile corespunzatoare pentru fiecare pagina la care ne aflam in functie de offset
            var paginatedCategories = categories.Skip(offset).Take(_perPage);

            // Preluam numarul ultimei pagini
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            // Trimitem categoriile cu ajutorul unui ViewBag catre View-ul corespunzator
            ViewBag.Categories = paginatedCategories;

            return View();
        }

        //CRUD de catre admin

        //Show -> Read
        public ActionResult Show(int id)
        {
            Category category = db.Categories.Find(id); //stocam categoria cu id-ul dat ca parametru
            return View(category); //ii dam return in view pt a o putea vizualiza in front
        }
        

        //New -> Create
        //HttpGet implicit -> ceea ce vede userul pe pagina categories/new
        //(inainte de a se produce crearea categoriei efectiv)
        public ActionResult New()
        {
            //return View();
            //Category cat = new Category();

            return View();
        }

        //procesul de adaugare al unei categorii
        [HttpPost]
        public ActionResult New(Category cat)
        {
            if(ModelState.IsValid)
            {
                db.Categories.Add(cat); //daca formularul e valid, adaugam categoria noua
                db.SaveChanges(); //dam commit

                TempData["message"] = "Categoria a fost adaugata";
                return RedirectToAction("Index"); //redirect in Index
            }
            else
            {
                return View(cat);
            }
        }

        //Edit - Update
        //HttpGet implicit -> ceea ce vede userul pe pagina categories/edit
        //(inainte de a se produce editarea categoriei efectiv)- ceea ce vede user-ul pentru a putea edita
        public ActionResult Edit(int id)
        {
            Category category = db.Categories.Find(id); //stocam categoria cu id-ul respectiv
            return View(category); //returnam view-ul asociat acelei categorii
        }


        //procesul de editare al unei categorii
        [HttpPost]
        public ActionResult Edit(int id, Category requestCategory)
        {
            Category category = db.Categories.Find(id); //stocam categoria cu id-ul respectiv

            if(ModelState.IsValid) //daca datele sunt valide
            {

                category.CategoryName = requestCategory.CategoryName; //schimbam numele categoriei
                db.SaveChanges(); //commit

                TempData["message"] = "Categoria a fost modificata!";
                return RedirectToAction("Index"); //redirect in index cu mesajul asociat
            }
            else
            {
                return View(requestCategory);
            }
        }


        //Delete
        //procesul de stergere al unei categorii
        [HttpPost]
        public ActionResult Delete(int id)
        {
            //se vor adauga ulterior aceste linii cand o sa fie gata partea de product si comments
            Category category = db.Categories.Include("Products")
                                             .Include("Products.Comments")
                                             .Where(c => c.Id == id)
                                             .First();
            //si se va sterge linia asta de mai jos-> deoarece va deveni inutila
            //Category category = db.Categories.Find(id);


            db.Categories.Remove(category); //stergerea categoriei din BD
            TempData["message"] = "Categoria a fost stearsa";
            db.SaveChanges(); //commit
            return RedirectToAction("Index"); //te redirectioneaza pe pagina categories/index
        }
        
    }
}
