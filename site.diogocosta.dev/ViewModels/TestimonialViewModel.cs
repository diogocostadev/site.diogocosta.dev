using site.diogocosta.dev.Models;

namespace site.diogocosta.dev.ViewModels;

public class TestimonialViewModel
{
    public List<Testimonial> Testimonials { get; set; } = new List<Testimonial>();

    // Dados de sumário que aparecem no topo da página
    public class CourseRatingSummary
    {
        public decimal AverageRating { get; set; }
        public int TotalStudents { get; set; }
        public List<string> StudentAvatars { get; set; } = new List<string>();
    }

    public CourseRatingSummary RatingSummary { get; set; } = default!;
}