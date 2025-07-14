namespace site.diogocosta.dev.Models
{
    public class ResponsiveImageModel
    {
        public string Src { get; set; } = string.Empty;
        public string? WebPSrc { get; set; }
        public string? SrcSet { get; set; }
        public string? Sizes { get; set; }
        public string Alt { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public string? Loading { get; set; } = "lazy"; // "lazy", "eager"
        public string? CssClass { get; set; }
        public string? ImageClass { get; set; }
        public string? Style { get; set; }
        public string? Id { get; set; }
        
        // Métodos para facilitar criação de diferentes tamanhos
        public static ResponsiveImageModel CreateProfileImage(string baseName, string alt, bool isEager = false)
        {
            return new ResponsiveImageModel
            {
                Src = $"/img/{baseName}.jpg",
                WebPSrc = $"/img/{baseName}.webp",
                SrcSet = $"/img/{baseName}_400w.webp 400w, /img/{baseName}_800w.webp 800w",
                Sizes = "(max-width: 400px) 100vw, 400px",
                Alt = alt,
                Width = 400,
                Height = 400,
                Loading = isEager ? "eager" : "lazy",
                ImageClass = "rounded-full object-cover"
            };
        }
        
        public static ResponsiveImageModel CreateHeroImage(string baseName, string alt, bool isEager = true)
        {
            return new ResponsiveImageModel
            {
                Src = $"/img/{baseName}.jpg",
                WebPSrc = $"/img/{baseName}.webp",
                SrcSet = $"/img/{baseName}_600w.webp 600w, /img/{baseName}_1200w.webp 1200w, /img/{baseName}.webp 1800w",
                Sizes = "(max-width: 768px) 100vw, (max-width: 1200px) 80vw, 60vw",
                Alt = alt,
                Width = 1200,
                Height = 800,
                Loading = isEager ? "eager" : "lazy",
                ImageClass = "w-full h-auto object-cover"
            };
        }
        
        public static ResponsiveImageModel CreateLogo(string baseName, string alt, bool isEager = true)
        {
            return new ResponsiveImageModel
            {
                Src = $"/img/{baseName}.png",
                WebPSrc = $"/img/{baseName}.webp",
                Alt = alt,
                Width = 120,
                Height = 40,
                Loading = isEager ? "eager" : "lazy",
                ImageClass = "h-auto max-w-full"
            };
        }
        
        public static ResponsiveImageModel CreateThumbnail(string baseName, string alt, int width = 300, int height = 200)
        {
            return new ResponsiveImageModel
            {
                Src = $"/img/{baseName}.jpg",
                WebPSrc = $"/img/{baseName}.webp",
                SrcSet = $"/img/{baseName}_300w.webp 300w, /img/{baseName}_600w.webp 600w",
                Sizes = "(max-width: 600px) 100vw, 300px",
                Alt = alt,
                Width = width,
                Height = height,
                Loading = "lazy",
                ImageClass = "rounded-lg object-cover"
            };
        }
    }
}
