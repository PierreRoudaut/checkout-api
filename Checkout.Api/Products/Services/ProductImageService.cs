
using System.Linq;
using System.Net.Http;
using Humanizer.Bytes;

namespace Checkout.Api.Products.Services
{
    public class ProductImageService
    {
        private static readonly long MaxImageAllowedSize = (long)new ByteSize().AddMegabytes(10).Bytes;

        private static readonly string[] ImageMimeTypes = { "image/jpeg", "image/png" };

        /// <summary>
        /// Validates that the registered image url associated to the product is a valid image smaller than 10 MB
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <returns></returns>
        public bool IsImageValid(string imageUrl)
        {
            var request = new HttpRequestMessage(HttpMethod.Head, imageUrl);
            using (var client = new HttpClient())
            {
                HttpResponseMessage response;
                try
                {
                    response = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;
                }
                catch
                {
                    return false;
                }
                if (!ImageMimeTypes.Contains(response.Content.Headers?.ContentType.MediaType))
                {
                    return false;
                }

                return response.Content.Headers?.ContentLength <= MaxImageAllowedSize;
            }
        }
    }
}
