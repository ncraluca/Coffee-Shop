using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProiectV1.Data;
using ProiectV1.Models;
using System;

namespace ProiectV1.Controllers
{
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IWebHostEnvironment _env;

        public RequestsController(
                ApplicationDbContext context,
                UserManager<ApplicationUser> userManager,
                RoleManager<IdentityRole> roleManager,
                IWebHostEnvironment env)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }


        public IActionResult Index()
        {
            return View();
        }

        //functia cu ajutorul careia facem pagina(view) in care Colaboratorul vede toate request-urile trimise doar de el

        [Authorize(Roles = "Colaborator")]
        public IActionResult IndexColaborator()
        {
            // Alegem sa afisam 3 cereri pe pagina
            int _perPage = 3;

            // Obtinem id-ul colaboratorului curent
            string currentUserId = _userManager.GetUserId(User);
            // Filtram request-urile pentru a le afisa doar pe cele ale colaboratorului curent
            var requests = db.Requests.Include("Category").Include("User")
                               .Where(req => req.UserId == currentUserId);

            ViewBag.Requests = requests; //stocam request-urile filtrate

            if (TempData.ContainsKey("message")) 
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            var search = "";
            // MOTOR DE CAUTARE
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {

                // eliminam spatiile libere
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();
                // Cautare in articol (Title)
                // Cautare in articol (Title)
                List<int> articleIds = db.Requests
                    .Where(at => at.Title.Contains(search))
                    .Select(a => a.Id)
                    .ToList();



                // Lista articolelor care contin cuvantul cautat
                // fie in articol -> Title si Content
                // fie in comentarii -> Content
                requests = db.Requests.Where(request =>
                        articleIds.Contains(request.Id))
                                .Include("Category")
                                .Include("User");
            }
            ViewBag.SearchString = search;

            // Fiind un numar variabil de cereri, verificam de fiecare data utilizand metoda Count()
            int totalItems = requests.Count();

            // Se preia pagina curenta din View-ul asociat
            // Numarul paginii este valoarea parametrului page din ruta /Requests/IndexColaborator?page=valoare
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            // Pentru prima pagina offsetul o sa fie zero
            // Pentru pagina 2 o sa fie 3
            // Asadar offsetul este egal cu numarul de cereri care au fost deja afisate pe paginile anterioare
            var offset = 0;

            // Se calculeaza offsetul in functie de numarul paginii la care suntem
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            // Se preiau cererile corespunzatoare pentru fiecare pagina la care ne aflam in functie de offset
            var paginatedRequests = requests.Skip(offset).Take(_perPage);

            // Preluam numarul ultimei pagini
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            // Trimitem cererile cu ajutorul unui ViewBag catre View-ul corespunzator
            ViewBag.Requests = paginatedRequests;

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Requests/Index/?search="
                + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Requests/Index/?page";
            }

            return View();
        }

        //HttpGet implicit
        //Functia de afisare a unui request pentru un produs (cu toate datele pe care le contine produsul
        //Momentan din View afisam doar titlu, descriere, data si cn a facut cererea (mai tarziu o sa adaugam si poza si alte detalii), momentan e strict pt functionalitate
        [Authorize(Roles = "Colaborator")]
        public IActionResult ShowColaborator(int id)
        {
            Request request = db.Requests.Include("Category")
                                         .Include("User") //join cu tabelele category si user
                               .Where(req => req.Id == id) //deci selectam request-urile avand si informatiile auxiliare legate de categorie si user-ul care a creat request
                               .First(); 

            return View(request);
        }


        //HttpGet implicit
        //Functia de adaugare a unui nou request de catre colaborator
        [Authorize(Roles = "Colaborator")]
        public IActionResult New()
        {
            Request request = new Request(); //crearea unui nou obiect de tip request

            request.Categ = GetAllCategories(); //in Categ stocam toate categoriile existente pentru a putea face in view dropdown(pt ca user-ul sa selecteze din el ce vrea)

            return View(request);
        }

        // Se adauga requestul in baza de date
        // Doar utilizatorii cu rolul de Colaborator pot face request-uri in platforma

        [Authorize(Roles = "Colaborator")]
        [HttpPost]
        public async Task<IActionResult> New(Request request, IFormFile imageFile)
        {
            var sanitizer = new HtmlSanitizer();

            request.Date = DateTime.Now;
            request.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                request.Description = sanitizer.Sanitize(request.Description);

                if (imageFile != null && imageFile.Length > 0)
                {
                    var storagePath = Path.Combine(
                        _env.WebRootPath,
                        "images",
                        imageFile.FileName
                    );

                    request.PictureUrl = "/images/" + imageFile.FileName;

                    using (var fileStream = new FileStream(storagePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                }

                db.Requests.Add(request);
                await db.SaveChangesAsync();

                TempData["message"] = "Cererea a fost trimisă către administrator!";
                TempData["messageType"] = "alert-success";

                return RedirectToAction("IndexColaborator");
            }
            else
            {
                request.Categ = GetAllCategories();
                return View(request);
            }
        }



        //functia cu ajutorul careia facem pagina(view) in care Adminul vede toate request-urile primite de la Colaboratori
        //am implementat si in partea de layout vizualizarea ei
        [Authorize(Roles = "Admin")]
        public IActionResult IndexAdmin()
        {
            // Alegem sa afisam 3 cereri pe pagina
            int _perPage = 3;

            var requests = db.Requests.Include("Category").Include("User")
                                        .Where(req => req.Status == "Pending"); //facem asta pentru a afisa paginat doar request-urile in asteptare 
            ViewBag.Requests = requests; //stocam request-urile

   
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            // Fiind un numar variabil de cereri, verificam de fiecare data utilizand metoda Count()
            int totalItems = requests.Count();

            // Se preia pagina curenta din View-ul asociat
            // Numarul paginii este valoarea parametrului page din ruta /Requests/IndexColaborator?page=valoare
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            // Pentru prima pagina offsetul o sa fie zero
            // Pentru pagina 2 o sa fie 3
            // Asadar offsetul este egal cu numarul de cereri care au fost deja afisate pe paginile anterioare
            var offset = 0;

            // Se calculeaza offsetul in functie de numarul paginii la care suntem
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            // Se preiau cererile corespunzatoare pentru fiecare pagina la care ne aflam in functie de offset
            var paginatedRequests = requests.Skip(offset).Take(_perPage);

            // Preluam numarul ultimei pagini
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            // Trimitem cererile cu ajutorul unui ViewBag catre View-ul corespunzator
            ViewBag.Requests = paginatedRequests;


            return View();
        }

        //se putea face si acelasi show pt colaborator si admin cu ascundere de butoane in functie de rol, dar am implementat asa in caz ca
                //devine mai complicat si mai diferit show-ul mai tarziu

        //metoda care afiseaza un request in detaliu (mai tz o sa se vada si poza etc)
        //HttpGet implicit
        [Authorize(Roles = "Admin")]
        public IActionResult ShowAdmin(int id)
        {
            Request request = db.Requests.Include("Category") //join cu category si user (pt a accesa foreign key)
                                         .Include("User")
                               .Where(req => req.Id == id)
                               .First();

            return View(request);
        }


        //Metoda pt a obtine categoriile
        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul categoriei si denumirea acesteia
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),

                    Text = category.CategoryName

                });
            }
            /* Sau se poate implementa astfel:
             *
             *foreach (var category in categories)
             {
                var listItem = new SelectListItem();
                listItem.Value = category.Id.ToString();
                listItem.Text = category.CategoryName.ToString();
                selectList.Add(listItem);
             }*/

            // returnam lista de categorii
            return selectList;
        }



        //metoda prin care Adminul aproba request-urile
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Approve(int id)
        {
            //var request = db.Requests.Find(id);
            Request request = db.Requests.Include("Category")
                                        .Where(req => req.Id == id)
                                        .First();
            if (request!=null)
            {
                request.Status = "Approved"; //statusul request-ului devine approved

                //facem transformarea din request in product (se creeaza un obiect de tip product care are exact atributele request-ului
                Product product = new Product();
                product.UserId = request.UserId;
                product.Title = request.Title;
                product.Description = request.Description;
                product.Price = request.Price; //se afiseaza doar primele 2 zecimale din pret
                product.PictureUrl = request.PictureUrl;
                //product.Stele = pending.Stele;
                //product.Date = request.Date;
                product.Category = request.Category;
                product.CategoryId = request.CategoryId;
                product.User = request.User;

                db.Products.Add(product);

                db.SaveChanges();
                TempData["message"] = "Cererea a fost aprobata cu succes!";
                TempData["messageType"] = "alert-success";
            }
            return RedirectToAction("IndexAdmin");
        }

        //metoda prin care Adminul refuza request-urile (se schimba statusul in Rejected si nu se creeaza un produs nou)
        [Authorize(Roles = "Admin")]
        public IActionResult Reject(int id)
        {
            var request = db.Requests.Find(id);
            if (request!=null)
            {
                request.Status = "Rejected";
                db.SaveChanges();
                TempData["message"] = "Cererea a fost respinsa!";
                TempData["messageType"] = "alert-success";
            }
            return RedirectToAction("IndexAdmin");
        }

        
    }
}