using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookWebShop.Models.Models;

public class Category
{
    [Key]
    public int Id { get; set; }
    [MaxLength(30)]
    public string Name { get; set; }
    [DisplayName("Display Order")]
    [Range(0, 30, ErrorMessage ="neki error")]
    public int DisplayOrder { get; set; }
}
