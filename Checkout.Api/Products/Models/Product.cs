using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Checkout.Api.Products.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        //public string Category { get; set; }
        public string ImageFilename { get; set; }
        public int Quantity { get; set; }

        [JsonIgnore]
        [NotMapped]
        public IFormFile Image { get; set; }
    }

    public class ProductForm
    {
        public IFormFile Image { get; set; }
        public Product Product { get; set; }
    }
}