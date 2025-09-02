using Microsoft.AspNetCore.Mvc;

namespace ASP_ITStep.Models.Api.Group
{
    public class ApiGroupFormModel
    {
        [FromForm(Name = "group-name")]
        public String Name { get; set; } = null!;

        [FromForm(Name = "group-description")]
        public String Description { get; set; } = null!;

        [FromForm(Name = "group-slug")]
        public String Slug { get; set; } = null!;

        [FromForm(Name = "group-parent-id")]
        public String? ParentId { get; set; } 

        [FromForm(Name = "group-img")]
        public IFormFile ImageUrl { get; set; } = null!;
    }
}
