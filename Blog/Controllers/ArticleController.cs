using Blog.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Blog.Controllers
{
    public class ArticleController : Controller
    {
        // GET: Article
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        // GET: Article/List
        public ActionResult List()
        {
            using (var database = new BlogDbContext())
            {
                var articles = database.Articles
                    .Include(a => a.Author)
                    .Include(a => a.Tags)
                    .ToList();

                return View(articles);
            }
        }

        // GET: Article/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                var article = database.Articles
                    .Where(a => a.Id == id)
                    .Include(a => a.Author)
                    .Include(a => a.Tags)
                    .First();

                var user = database.Users.FirstOrDefault(u => u.UserName.Equals(this.User.Identity.Name));

                var comments = database.Comments
                    .Where(a => a.Article.Id == id)
                    .ToList();
                if (article == null)
                {
                    return HttpNotFound();
                }

                var model = new TestModel();
                model.Author = user;
                model.Article = article;
                model.CommentList = comments;

                
                return View(model);
            }
        }

        // GET: Article/Create
        [HttpGet]
        [Authorize]
        public ActionResult Create()
        {
            using (var database = new BlogDbContext())
            {
                var model = new ArticleViewModel();
                model.Categories = database.Categories.OrderBy(c => c.Name).ToList();
                return View(model);
            }
        }

        // POST: Article/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ArticleViewModel model)
        {
            using (var db = new BlogDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.UserName.Equals(this.User.Identity.Name));

                var article = new Article(user.Id, model.Title, model.Content, model.CategoryId);
                this.SetArticleTags(article, model, db);

                db.Articles.Add(article);
                db.SaveChanges();

                return RedirectToAction("List");
            }
        }

        // GET: Article/Delete
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            using (var database = new BlogDbContext())
            {
                var article = database.Articles
                    .Where(a => a.Id == id)
                    .Include(a => a.Author)
                    .Include(a => a.Category)
                    .Include(a => a.Tags)
                    .First();

                ViewBag.Tags = string.Join(", ", article.Tags.Select(t => t.Name));

                if (!IsUserAuthorizedToEdit(article))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                
                if (article == null)
                {
                    return HttpNotFound();
                }

                
                return View(article);
            }
        }

        // POST: Article/Delete
        [HttpPost, Authorize]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                
                var article = database.Articles
                    .Where(a => a.Id == id)
                    .Include(a => a.Author)
                    .Include(a => a.Category)
                    .First();

                
                if (article == null)
                {
                    return HttpNotFound();
                }

                
                database.Articles.Remove(article);
                database.SaveChanges();

                
                return RedirectToAction("Index");
            }
        }

        // GET: Article/Edit
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new BlogDbContext())
            {
                var article = db.Articles.FirstOrDefault(a => a.Id == id);

                if (!this.IsUserAuthorizedToEdit(article))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                if (article == null)
                {
                    return HttpNotFound();
                }

                var model = new ArticleViewModel();
                model.AuthorId = article.AuthorId;
                model.Title = article.Title;
                model.Content = article.Content;
                model.CategoryId = article.CategoryId;
                model.Categories = db.Categories.ToList();

                model.Tags = string.Join(", ", article.Tags.Select(t => t.Name));

                return View(model);
            }
        }

        // POST: Article/Edit
        [HttpPost]
        [Authorize]
        public ActionResult Edit(int? id, ArticleViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new BlogDbContext())
                {
                    var article = db.Articles.FirstOrDefault(a => a.Id == id);
                    article.Title = model.Title;
                    article.Content = model.Content;
                    article.CategoryId = model.CategoryId;

                    this.SetArticleTags(article, model, db);

                    db.Entry(article).State = EntityState.Modified;
                    db.SaveChanges();

                    return RedirectToAction("List");
                }
            }

            return View(model);
        }

        private bool IsUserAuthorizedToEdit(Article article)
        {
            bool isAdmin = this.User.IsInRole("Admin");
            bool isAuthor = article.IsAuthor(this.User.Identity.Name);

            return isAdmin || isAuthor;
        }

        private void SetArticleTags(Article article, ArticleViewModel model, BlogDbContext db)
        {
            var tagsStrings = model.Tags
                .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Distinct();

            article.Tags.Clear();

            foreach (var tagString in tagsStrings)
            {
                Tag tag = db.Tags.FirstOrDefault(t => t.Name.Equals(tagString));

                if (tag == null)
                {
                    tag = new Tag() { Name = tagString };
                    db.Tags.Add(tag);
                }

                article.Tags.Add(tag);
            }
        }
    }
}