using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Blog.Models
{
    public class ArticleViewModel
    {
        [Required]
        [StringLength(50)]
        public string Title { get; set; }

        public int Id;

        public string Content { get; set; }

        [Required]
        public string AuthorId { get; set; }

        public ApplicationUser Author { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public ICollection<Category> Categories { get; set; }

        public string Tags { get; set; }

        public IEnumerable<CommentViewModel> Comments { get; set; }

    }
}