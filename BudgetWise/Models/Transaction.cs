using BudgetWise.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetWise.Models
{
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Amount should be greater than 0.")]
        public int Amount { get; set; }

        [Column(TypeName = "varchar(75)")]
        public string? Note { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public Category? Category { get; set; }

        [NotMapped]
        public string? CategoryTitleWithIcon 
        {
            get
            {
                return Category == null ? "" : Category.Icon + " " + Category.Title;
            }
        }
        [NotMapped]
        public string? FormattedAmount
        {
            get
            {
                return ((Category == null || Category.Type == "Expense") ? "-" : "+") + Amount.ToString("C0");
            }
        }
        public string TitleWithoutEmoji
        {
            get
            {
                if (string.IsNullOrEmpty(CategoryTitleWithIcon)) return string.Empty;

                // Split on first whitespace to remove emoji
                var parts = CategoryTitleWithIcon.Split(new[] { ' ' }, 2);
                return parts.Length > 1 ? parts[1] : CategoryTitleWithIcon;
            }
        }

        public string UserId { get; set; }
        //public ApplicationUser User { get; set; }
    }
}
