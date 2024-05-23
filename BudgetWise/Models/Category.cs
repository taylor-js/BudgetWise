using BudgetWise.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace BudgetWise.Models
{
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryId { get; set; }

        [Column(TypeName = "varchar(50)")]
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }

        [Column(TypeName = "varchar(5)")]
        [Required(ErrorMessage = "Icon is required.")]
        public string Icon { get; set; } = "";

        [Column(TypeName = "varchar(10)")]
        [Required(ErrorMessage = "Type is required.")]
        public string Type { get; set; }// = "Expense";

        [NotMapped]
        public string? TitleWithIcon
        {
            get
            {
                return this.Icon + " " + this.Title;
            }
        }
        //public string TitleWithoutEmoji => Title;
        public string TitleWithoutEmoji
        {
            get
            {
                if (string.IsNullOrEmpty(TitleWithIcon)) return string.Empty;

                // Split on first whitespace to remove emoji
                var parts = TitleWithIcon.Split(new[] { ' ' }, 2);
                return parts.Length > 1 ? parts[1] : TitleWithIcon;
            }
        }
        public string UserId { get; set; }
        //public ApplicationUser User { get; set; }
    }
}
