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
                Name = "Mateus domingos da Silva lima",
                Content = "Curso sensacional, vale cada centavo !!!",
                ImageUrl = "/img/testemunhos/1.jpg",
                Role = "Business Owner"
            },
            new Testimonial
            {
                Id = 2,
                Name = "Ezequielson Correia da silva",
                Content = "Gostaria de expressar minha gratidão e admiração pelo curso ministrado pelo professor Diogo . Sem dúvida, foi o melhor curso de programação que já assinei. O professor é extremamente empolgante, consegue explicar os conceitos de forma clara e envolvente, tornando o aprendizado muito mais agradável e eficaz. O curso foi perfeito para o meu nível de conhecimento, e me sinto muito mais confiante nas minhas habilidades. Recomendo fortemente a todos que buscam um ensino de qualidade!",
                ImageUrl = "/img/testemunhos/2.jpg",
                Role = "Marketing Professional"
            },
            new Testimonial
            {
                Id = 3,
                Name = "Luiz Caetano",
                Content = "O curso é sensacional, a didática do professor é muito boa! Pra galera que tá querendo embarcar no mundo do c sharp, pode investir no curso sem medo. Dá pra ter uma base muito boa. Porém depende muito da gente praticar todos os dias (Domingo a domingo) para que consigamos a nossa tão sonhada vaga na área de programação. Pois nada cai do céu. Aqui é uma porta de entrada, uma boa oportunidade focado naquilo que as empresas pedem. Professor a única coisa que falaria como um pequeno ponto de atenção é que algumas aulas o áudio ficou meio baixo. Mas nada que um fone não resolva. Pois quem quer dá um jeito. E por último mas não menos importante tem um grupo de suporte e complementos no discord. Uma estrutura fodástica que nenhum outro curso que já fiz aqui na plataforma tem. Vamos pra cima!",
                ImageUrl = "/img/testemunhos/3.webp",
                Role = "Aluno do curso Desenvol....."
            },
            new Testimonial
            {
                Id = 4,
                Name = "Lucas Eduardo Klitzke da Silva",
                Content = "Bem acima das minhas expectativas! O professor ensina de forma divertida e com clareza os conteúdos propostos no curso, um ótimo curso!",
                ImageUrl = "/img/testemunhos/4.jpg",
                Role = "Aluno do curso Desenvol....."
            },
            new Testimonial
            {
                Id = 5,
                Name = "Manish Gvalani",
                Content = "Aulas elaboradas e didática boa.",
                ImageUrl = "/img/testemunhos/5.webp",
                Role = "Aluno do curso Desenvol....."
            },
            new Testimonial
            {
                Id = 6,
                Name = "Ricardo Aguiar dos Santos",
                Content = "O professor é excelente, conduzindo as aulas com calma e clareza, tornando o aprendizado fácil e acessível. muito, muito bom pra ingressar no mercado de trabalho. Só tenho gratidão pelo incrível trabalho do professor. Recomendo a todos. \ud83d\udc4f\ud83d\udc68\u200d\ud83c\udfeb",
                ImageUrl = "/img/testemunhos/6.webp",
                Role = "Aluno do curso Desenvol....."
            }
        };
    }
}