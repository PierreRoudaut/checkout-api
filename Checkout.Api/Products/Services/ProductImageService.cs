
using System.IO;
using System.Linq;
using Humanizer.Bytes;
using Microsoft.AspNetCore.Http;

namespace Checkout.Api.Products.Services
{
    public class ProductImageService
    {
        private static readonly long MaxImageAllowedSize = (long)new ByteSize().AddMegabytes(10).Bytes;

        private static readonly string[] ImageMimeTypes = { "image/jpeg", "image/png" };

        /// <summary>
        /// Stores the uploaded image of a given product.
        /// Delete any files
        /// </summary>
        /// <param name="image"></param>
        /// <param name="filename"></param>
        public void SaveProductImage(IFormFile image, string filename)
        {
            Program.ProductImagesDirInfo.Refresh();
            var filePath = Path.Combine(Program.ProductImagesDirInfo.FullName, filename);
            var target = new FileStream(filePath, FileMode.Create);
            image.CopyTo(target);
        }

        public void DeleteProductImage(string filename)
        {
            Program.ProductImagesDirInfo.Refresh();
            var filePath = Path.Combine(Program.ProductImagesDirInfo.FullName, filename);
        }


        /// <summary>
        /// Validates that the uploaded file associated to the product is a valid image smaller than 10 MB
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool IsImageValid(IFormFile file)
        {
            if (!ImageMimeTypes.Contains(file.ContentType))
            {
                return false;
            }

            if (file.Length > MaxImageAllowedSize)
            {
                return false;
            }

            return true;
        }
    }
}
