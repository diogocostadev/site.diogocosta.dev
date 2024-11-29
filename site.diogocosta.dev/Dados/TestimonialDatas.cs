using site.diogocosta.dev.Models;

namespace site.diogocosta.dev.Dados;

public static class TestimonialsData
{
    public static List<Testimonial> GetTestimonials()
    {
        return new List<Testimonial>
        {
            new Testimonial
            {
                Id = 1,
                Name = "Dave Sharp",
                Content = "Brilliant. This course absolutely delivers, hands down. Justin makes it clear and simple how to get the most out of LinkedIn by providing solid value to your audience. Easy to follow and no BS.",
                ImageUrl = "/images/testimonials/dave-sharp.jpg",
                Role = "Business Owner"
            },
            new Testimonial
            {
                Id = 2,
                Name = "Colin Murray",
                Content = "Justin's course is truly outstanding! I took more away from this two hour course than I took away from any marketing course or even my MBA.",
                ImageUrl = "/images/testimonials/colin-murray.jpg",
                Role = "Marketing Professional"
            },
            new Testimonial
            {
                Id = 3,
                Name = "Brianne Ramsay",
                Content = "This course has been life changing for my business. I've seen a 100% increase in website visits and this course only took an afternoon to complete. I love how direct and to the point this online training is.",
                ImageUrl = "/images/testimonials/brianne-ramsay.jpg",
                Role = "Entrepreneur"
            },
            new Testimonial
            {
                Id = 4,
                Name = "Joanne Schonheim",
                Content = "I hesitated a little while before signing up to Justin's course as I didn't feel I was ready. I'm so glad I finally bit the bullet. I have gained more traction in the 30 days since, then I likely would have if I waited.",
                ImageUrl = "/images/testimonials/joanne-schonheim.jpg",
                Role = "Digital Creator"
            },
            new Testimonial
            {
                Id = 5,
                Name = "Manish Gvalani",
                Content = "I was overwhelmed with the idea of creating content. Justin's course made it very simple in bite size videos on how to make it happen, one step at a time. It was crisp, effective, and empowering. If you are considering buying this course, get over analysis paralysis and take action. It will be your best investment.",
                ImageUrl = "/images/testimonials/manish-gvalani.jpg",
                Role = "Business Consultant"
            },
            new Testimonial
            {
                Id = 6,
                Name = "Teodor Dimokenchev",
                Content = "The Content OS is the most straightforward way to learn how to start posting regular, high-quality content on social media.",
                ImageUrl = "/images/testimonials/teodor-dimokenchev.jpg",
                Role = "Content Creator"
            }
        };
    }
}