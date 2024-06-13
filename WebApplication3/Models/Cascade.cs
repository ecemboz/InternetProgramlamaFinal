using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication3.Models
{
    public class Cascade
    {
        public IEnumerable<SelectListItem> CategoryList { get; set; }
        public IEnumerable<SelectListItem> SubCategoryList { get; set; }
    }
}
