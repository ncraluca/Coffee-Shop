using AngleSharp.Io;
using Ganss.Xss;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using ProiectV1.Data;
using ProiectV1.Models;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ProiectV1.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IWebHostEnvironment _env;

        public ProductsController(
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


        //metoda care afiseaza toate produsele(cu ajutorul view-ului sau asociat)
        public IActionResult Index()
        {
            // Alegem sa afisam 3 produse pe pagina
            int _perPage = 3;
            // Obtinem id-ul userului curent
            string currentUserId = _userManager.GetUserId(User);
            
            var products = db.Products.Include("Category").Include("User");

            ViewBag.Products = products; //stocam produsele

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
                List<int> articleIds = db.Products
                    .Where(at => at.Title.Contains(search))
                    .Select(a => a.Id)
                    .ToList();



                // Lista articolelor care contin cuvantul cautat
                // fie in articol -> Title si Content
                // fie in comentarii -> Content
                products = db.Products.Where(product =>
                        articleIds.Contains(product.Id))
                                .Include("Category")
                                .Include("User");
            }
            ViewBag.SearchString = search;

            var sortOrder = Convert.ToString(HttpContext.Request.Query["sortOrder"]);
            ViewBag.CurrentSort = sortOrder;

            switch (sortOrder)
            {
                case "priceAsc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "priceDesc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "starsAsc":
                    products = products.OrderBy(p => p.Rating); // Presupunând că există o proprietate "Stars" în clasa Product
                    break;
                case "starsDesc":
                    products = products.OrderByDescending(p => p.Rating);
                    break;
                default:
                    // Adăugați o sortare implicită, de exemplu, după titlu sau alt criteriu
                    products = products.OrderBy(p => p.Title);
                    break;
            }

            // Fiind un numar variabil de produse, verificam de fiecare data utilizand metoda Count()
            int totalItems = products.Count();

            // Se preia pagina curenta din View-ul asociat
            // Numarul paginii este valoarea parametrului page din ruta /Products/Index?page=valoare
            var currentPage =Convert.ToInt32(HttpContext.Request.Query["page"]);

            // Pentru prima pagina offsetul o sa fie zero
            // Pentru pagina 2 o sa fie 3
            // Asadar offsetul este egal cu numarul de produse care au fost deja afisate pe paginile anterioare
            var offset = 0;

            // Se calculeaza offsetul in functie de numarul paginii la care suntem
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            // Se preiau articolele corespunzatoare pentru fiecare pagina la care ne aflam in functie de offset
            var paginatedProducts = products.Skip(offset).Take(_perPage);

            // Preluam numarul ultimei pagini
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            // Trimitem produsele cu ajutorul unui ViewBag catre View-ul corespunzator
            ViewBag.Products = paginatedProducts;

            if (search != "")
            {

                ViewBag.PaginationBaseUrl = "/Products/Index/?search=" + search + "&sortOrder=" + sortOrder + "&page";
                
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Products/Index/?sortOrder=" + sortOrder + "&page";
            }

            SetAccessRights();
            
            return View();
        }
        

        //HttpGet implicit
        //Functia de afisare a unui produs (cu toate datele pe care le contine produsul
        //Momentan din View afisam doar titlu, descriere, data si cn a adaugat produsul
        //(mai tarziu o sa adaugam si poza si alte detalii), momentan e strict pt functionalitate
        //[Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Show(int id)
        {
            Product product = db.Products.Include("Category")
                                         .Include("User")
                                         .Include("Comments")
                                         .Include("Comments.User")
                                         .Where(prod => prod.Id == id) //deci selectam request-urile avand si informatiile auxiliare legate de categorie si user-ul care a creat request
                                         .First();
            calculezrating(id);
            SetAccessRights();

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            var comments = product.Comments;
           

            return View(product);
        }

        // Adaugarea unui comentariu asociat unui articol in baza de date
        // Toate rolurile pot adauga comentarii in baza de date
        [HttpPost]
        [Authorize(Roles = "User,Colaborator,Admin")]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;
            comment.UserId = _userManager.GetUserId(User);
          
            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Redirect("/Products/Show/" + comment.ProductId);
            }

            else
            {
                Product prod = db.Products.Include("Category")
                                         .Include("User")
                                         .Include("Comments")
                                         .Include("Comments.User")
                                         .Where(prod => prod.Id == comment.ProductId)
                                         .First();


                SetAccessRights();

                return View(prod);
            }
        }

        // Se sterge un articol din baza de date
        // Utilizatorii cu rolul de Editor sau Admin pot sterge articole
        // Editorii pot sterge doar articolele publicate de ei
        // Adminii pot sterge orice articol din baza de date

        [HttpPost]
        [Authorize(Roles = "Colaborator,Admin")]
        public ActionResult Delete(int id)
        {
            Product product = db.Products.Include("Comments") //pt stergerea in cascada
                                         .Where(prod => prod.Id == id)
                                         .First();
            if (product.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Products.Remove(product);
                db.SaveChanges();
                TempData["message"] = "Produsul a fost sters";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un produs care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        // Conditiile de afisare a butoanelor de stergere
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;
            ViewBag.EsteUser = false;
            if (User.Identity.IsAuthenticated)
                ViewBag.EsteUser = true;

            if (User.IsInRole("Colaborator"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }


        [Authorize(Roles = "Colaborator,Admin")]
        public IActionResult Edit(int id)
        {

            Product product = db.Products.Include("Category")
                                        .Where(prod => prod.Id == id)
                                        .First();

            product.Categ = GetAllCategories();

            if (product.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(product);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

        }

        
        [HttpPost]
        [Authorize(Roles = "Colaborator,Admin")]
        public IActionResult Edit(int id, Product requestproduct)
        {

            var sanitizer = new HtmlSanitizer();

            Product product = db.Products.Find(id);


            if (ModelState.IsValid)
            {
                if (product.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    product.Title = requestproduct.Title;

                    requestproduct.Description = sanitizer.Sanitize(requestproduct.Description);

                    product.Description = requestproduct.Description;
                    product.CategoryId = requestproduct.CategoryId;
                    product.Price = requestproduct.Price;
                    //product.PictureUrl = requestproduct.PictureUrl;
                    /*
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var storagePath = Path.Combine(
                            _env.WebRootPath,
                            "images",
                            imageFile.FileName
                        );

                        requestproduct.PictureUrl = "/images/" + imageFile.FileName;
                        using (var fileStream = new FileStream(storagePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }
                    }
                    product.PictureUrl = requestproduct.PictureUrl;*/
                    TempData["message"] = "Produsul a fost modificat";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                requestproduct.Categ = GetAllCategories();
                return View(requestproduct);
            }
        }

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
            return selectList;
        }
        public void calculezrating(int id)
        {
            double medie = 0;
            int contor = 0;
            var product = db.Products
            .Include(p => p.Comments)
            .Where(p => p.Id == id)
            .FirstOrDefault();
            if (product != null)
            {
                foreach(var comment in product.Comments)
                {
                    medie = medie + comment.Stars;
                    contor++;
                }
                if (contor != 0)
                {
                    medie = medie / contor;
                    product.Rating = medie;
                }
                else
                {
                    product.Rating = 0;
                }
            }
            db.SaveChanges();
        }

        //vom implementa metoda in care transformam produsul in productshopping..(apoi in controller la productshopping o punem in cos)
        //apoi vom afisa in cos produsele.

        [HttpPost]
        [Authorize(Roles = "User")]
        public IActionResult CreateProd(int id)
        {
            var product = db.Products.Find(id);
            if (product != null)
            {
                string userId = _userManager.GetUserId(User);
                var cart = db.ShoppingCarts.FirstOrDefault(c => c.UserId == userId);
                if (cart == null)
                {
                    cart = new ShoppingCart { UserId = userId };
                    db.ShoppingCarts.Add(cart);
                    db.SaveChanges();
                }

                //verificam daca produsul respectiv e deja in cos ca sa stim daca doar modificam cantitatea sau cream altul nou
                var existingProductCart = db.ProductShoppingCarts.FirstOrDefault(pc => pc.ProductId == product.Id && pc.ShoppingCartId == cart.Id);

                if (existingProductCart != null)
                {
                    existingProductCart.Quantity = existingProductCart.Quantity + 1;
                    existingProductCart.Price = product.Price * existingProductCart.Quantity;
                }
                else
                {
                    // facem transformarea din request in product(se creeaza un obiect de tip product care are exact atributele request - ului
                    ProductShoppingCart productcart = new ProductShoppingCart();
                    productcart.ProductId = product.Id;
                    productcart.ShoppingCartId = cart.Id;
                    productcart.Quantity = 1;
                    productcart.Price = product.Price;

                    db.ProductShoppingCarts.Add(productcart);

                }

                db.SaveChanges();
                TempData["message"] = "Produsul a fost adaugat in cosul de cumparaturi cu succes!";
                TempData["messageType"] = "alert-success";
            }
            return RedirectToAction("Index", "ProductShoppingCarts");
        }




    }
}
