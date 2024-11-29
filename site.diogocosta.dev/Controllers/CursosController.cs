using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using site.diogocosta.dev.Dados;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.ViewModels;

namespace site.diogocosta.dev.Controllers;


public class CursosController : Controller
{
    public IActionResult Index()
    {
        var viewModel = new TestimonialViewModel
        {
            Testimonials = TestimonialsData.GetTestimonials(),
            RatingSummary = new TestimonialViewModel.CourseRatingSummary
            {
                AverageRating = 4.97M,
                TotalStudents = 30000,
                StudentAvatars = new List<string> { /* urls dos avatars */ }
            }
        };
    
        return View(viewModel);
    }

}