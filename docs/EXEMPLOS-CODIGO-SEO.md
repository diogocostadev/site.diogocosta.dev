# 🔧 EXEMPLOS DE CÓDIGO: IMPLEMENTAÇÃO SEO
*Código pronto para copiar e colar*

---

## 📄 **TEMPLATES PRONTOS PARA USAR**

### **1. META TAGS COMPLETAS**

#### **Para _Layout.cshtml (Substituir seção `<head>`)**

```html
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    
    <!-- SEO Meta Tags -->
    <title>@(ViewBag.Title ?? "Diogo Costa - Programador Freelancer | Desenvolvimento Web ASP.NET")</title>
    <meta name="description" content="@(ViewBag.Description ?? "Programador freelancer especializado em desenvolvimento web com ASP.NET e Angular. Consultor tecnológico para pequenas empresas. Soluções personalizadas e automação de processos.")">
    <meta name="keywords" content="programador freelancer, desenvolvimento web, ASP.NET developer, consultor tecnológico, criação de sites, desenvolvimento de software, automação de processos">
    <meta name="author" content="Diogo Costa">
    <meta name="robots" content="index, follow">
    <link rel="canonical" href="@(ViewBag.CanonicalUrl ?? $"https://site.diogocosta.dev{Context.Request.Path}")">
    
    <!-- Open Graph / Facebook -->
    <meta property="og:type" content="website">
    <meta property="og:url" content="https://site.diogocosta.dev@(Context.Request.Path)">
    <meta property="og:title" content="@(ViewBag.OgTitle ?? ViewBag.Title ?? "Diogo Costa - Programador Freelancer & Consultor Tecnológico")">
    <meta property="og:description" content="@(ViewBag.OgDescription ?? ViewBag.Description ?? "Desenvolvimento web profissional com ASP.NET. Especialista em soluções personalizadas para pequenas empresas.")">
    <meta property="og:image" content="https://site.diogocosta.dev/images/og-image.jpg">
    <meta property="og:locale" content="pt_BR">
    <meta property="og:site_name" content="Diogo Costa - Programador Freelancer">
    
    <!-- Twitter -->
    <meta property="twitter:card" content="summary_large_image">
    <meta property="twitter:url" content="https://site.diogocosta.dev@(Context.Request.Path)">
    <meta property="twitter:title" content="@(ViewBag.TwitterTitle ?? ViewBag.Title ?? "Programador Freelancer | Desenvolvimento Web ASP.NET")">
    <meta property="twitter:description" content="@(ViewBag.TwitterDescription ?? ViewBag.Description ?? "Consultor tecnológico especializado em desenvolvimento web e automação de processos.")">
    <meta property="twitter:image" content="https://site.diogocosta.dev/images/twitter-image.jpg">
    
    <!-- Dados Estruturados -->
    <script type="application/ld+json">
    {
        "@@context": "https://schema.org",
        "@@type": "Person",
        "name": "Diogo Costa",
        "jobTitle": "Programador Freelancer e Consultor Tecnológico",
        "description": "Especialista em desenvolvimento web com ASP.NET e Angular",
        "url": "https://site.diogocosta.dev",
        "sameAs": [
            "https://linkedin.com/in/diogocostadev",
            "https://github.com/diogocostadev"
        ],
        "knowsAbout": ["Desenvolvimento Web", "ASP.NET", "Angular", "Consultoria Tecnológica"],
        "workLocation": {
            "@@type": "Place",
            "address": "Brasil"
        }
    }
    </script>
    
    <!-- Favicon e PWA -->
    <link rel="icon" type="image/x-icon" href="~/favicon.ico" />
    <link rel="manifest" href="~/manifest.json">
    <meta name="theme-color" content="#007bff">
    
    <!-- Preconnect para Performance -->
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link rel="dns-prefetch" href="//www.google-analytics.com">
    
    <!-- CSS -->
    <link rel="preload" href="~/css/critical.css" as="style" onload="this.onload=null;this.rel='stylesheet'">
    <noscript><link rel="stylesheet" href="~/css/critical.css"></noscript>
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</head>
```

---

### **2. PÁGINAS OTIMIZADAS**

#### **Home Page (Views/Home/Index.cshtml)**

```html
@{
    ViewBag.Title = "Programador Freelancer | Desenvolvimento Web & Consultoria Tecnológica";
    ViewBag.Description = "Especialista em desenvolvimento web com ASP.NET e Angular. Consultor tecnológico para automação de processos e soluções personalizadas para pequenas empresas.";
    ViewBag.OgTitle = "Diogo Costa - Programador Freelancer e Consultor Tecnológico";
    ViewBag.OgDescription = "Desenvolvimento web profissional, consultoria tecnológica e automação de processos. Especialista em ASP.NET e Angular.";
}

<div class="hero-section">
    <div class="container">
        <div class="row">
            <div class="col-lg-8">
                <h1 class="display-4">Programador Freelancer Especializado em Desenvolvimento Web</h1>
                <p class="lead">
                    Como <strong>consultor tecnológico</strong> e <strong>desenvolvedor ASP.NET</strong>, 
                    ajudo pequenas empresas a criar soluções web personalizadas e automação de processos.
                </p>
                <div class="cta-buttons">
                    <a href="/servicos" class="btn btn-primary btn-lg">Ver Serviços</a>
                    <a href="/contato" class="btn btn-outline-secondary btn-lg">Solicitar Orçamento</a>
                </div>
            </div>
        </div>
    </div>
</div>

<section class="services-preview">
    <div class="container">
        <h2 class="text-center mb-5">Serviços de Desenvolvimento Web e Consultoria</h2>
        
        <div class="row">
            <div class="col-md-4">
                <div class="service-card">
                    <h3>Desenvolvimento ASP.NET</h3>
                    <p>Criação de sites e sistemas web robustos usando <strong>ASP.NET Core</strong> 
                    e <strong>Angular</strong>. Soluções escaláveis para pequenas empresas.</p>
                </div>
            </div>
            
            <div class="col-md-4">
                <div class="service-card">
                    <h3>Consultoria Tecnológica</h3>
                    <p>Análise de necessidades, arquitetura de sistemas e planejamento estratégico. 
                    Ajudo empresas a tomar decisões tecnológicas inteligentes.</p>
                </div>
            </div>
            
            <div class="col-md-4">
                <div class="service-card">
                    <h3>Automação de Processos</h3>
                    <p>Desenvolvimento de soluções para <strong>automação de processos empresariais</strong>. 
                    Redução de trabalho manual e integração de sistemas.</p>
                </div>
            </div>
        </div>
    </div>
</section>

<!-- FAQ Schema para Rich Snippets -->
<script type="application/ld+json">
{
    "@@context": "https://schema.org",
    "@@type": "FAQPage",
    "mainEntity": [
        {
            "@@type": "Question",
            "name": "Quanto custa contratar um programador freelancer?",
            "acceptedAnswer": {
                "@@type": "Answer",
                "text": "Os valores variam conforme complexidade do projeto. Projetos simples começam em R$ 2.000, enquanto sistemas complexos podem chegar a R$ 15.000 ou mais. Faço orçamentos personalizados."
            }
        },
        {
            "@@type": "Question",
            "name": "Quanto tempo leva para desenvolver um site?",
            "acceptedAnswer": {
                "@@type": "Answer",
                "text": "Sites institucionais: 2-4 semanas. Sistemas web complexos: 2-6 meses. O prazo depende do escopo, funcionalidades e disponibilidade de conteúdo."
            }
        },
        {
            "@@type": "Question",
            "name": "Trabalha com quais tecnologias?",
            "acceptedAnswer": {
                "@@type": "Answer",
                "text": "Especializo em ASP.NET Core, Angular, TypeScript, SQL Server e Entity Framework. Também trabalho com APIs REST, Azure e automação de processos."
            }
        }
    ]
}
</script>
```

#### **Página Sobre (Views/Sobre/Index.cshtml)**

```html
@{
    ViewBag.Title = "Sobre - Consultor Tecnológico e Desenvolvedor ASP.NET | Diogo Costa";
    ViewBag.Description = "Conheça Diogo Costa, programador freelancer com expertise em desenvolvimento web, ASP.NET e consultoria tecnológica para empresas. Anos de experiência em soluções personalizadas.";
}

<div class="container">
    <div class="row">
        <div class="col-lg-8">
            <h1>Consultor Tecnológico e Desenvolvedor Web</h1>
            
            <div class="intro-section">
                <p class="lead">
                    Sou <strong>Diogo Costa</strong>, <strong>programador freelancer</strong> e 
                    <strong>consultor tecnológico</strong> especializado em desenvolvimento web 
                    com <strong>ASP.NET</strong> e <strong>Angular</strong>.
                </p>
                
                <p>
                    Com mais de X anos de experiência, ajudo pequenas empresas e startups a 
                    transformar ideias em soluções digitais robustas. Minha especialidade é 
                    <strong>desenvolvimento de software</strong> personalizado e 
                    <strong>automação de processos empresariais</strong>.
                </p>
            </div>
            
            <section class="expertise-section">
                <h2>Especialidades em Desenvolvimento Web</h2>
                
                <div class="row">
                    <div class="col-md-6">
                        <h3>Tecnologias Principais</h3>
                        <ul class="tech-list">
                            <li><strong>ASP.NET Core MVC</strong> - Framework principal</li>
                            <li><strong>Angular & TypeScript</strong> - Frontend moderno</li>
                            <li><strong>SQL Server</strong> - Banco de dados</li>
                            <li><strong>Entity Framework</strong> - ORM avançado</li>
                            <li><strong>APIs REST</strong> - Integração de sistemas</li>
                        </ul>
                    </div>
                    
                    <div class="col-md-6">
                        <h3>Serviços como Consultor</h3>
                        <ul class="services-list">
                            <li>Análise de necessidades técnicas</li>
                            <li>Arquitetura de sistemas</li>
                            <li>Planejamento de projetos</li>
                            <li>Revisão de código</li>
                            <li>Mentoria técnica</li>
                        </ul>
                    </div>
                </div>
            </section>
            
            <section class="approach-section">
                <h2>Minha Abordagem como Programador Freelancer</h2>
                
                <p>
                    Como <strong>consultor tecnológico</strong>, acredito que cada empresa tem 
                    necessidades únicas. Por isso, minha metodologia inclui:
                </p>
                
                <ol>
                    <li><strong>Análise detalhada</strong> dos requisitos e objetivos</li>
                    <li><strong>Planejamento estratégico</strong> da solução técnica</li>
                    <li><strong>Desenvolvimento iterativo</strong> com feedback constante</li>
                    <li><strong>Entrega com documentação</strong> completa e treinamento</li>
                    <li><strong>Suporte pós-entrega</strong> para garantir o sucesso</li>
                </ol>
            </section>
            
            <section class="why-choose-section">
                <h2>Por que Escolher um Especialista em ASP.NET?</h2>
                
                <p>
                    O <strong>desenvolvimento web</strong> com ASP.NET oferece vantagens únicas 
                    para empresas que precisam de:
                </p>
                
                <ul>
                    <li><strong>Performance excepcional</strong> - Aplicações rápidas e responsivas</li>
                    <li><strong>Segurança avançada</strong> - Proteção nativa contra ameaças</li>
                    <li><strong>Escalabilidade</strong> - Crescimento sem reescrita</li>
                    <li><strong>Integração Microsoft</strong> - Compatibilidade com Office 365, Azure</li>
                    <li><strong>Suporte de longo prazo</strong> - Tecnologia estável e madura</li>
                </ul>
            </section>
            
            <div class="cta-section">
                <h2>Vamos Conversar Sobre Seu Projeto?</h2>
                <p>
                    Se você precisa de um <strong>programador freelancer</strong> experiente ou 
                    <strong>consultoria tecnológica</strong> para seu negócio, vamos conversar.
                </p>
                <a href="/contato" class="btn btn-primary btn-lg">Solicitar Consulta Gratuita</a>
            </div>
        </div>
        
        <div class="col-lg-4">
            <div class="sidebar">
                <div class="contact-card">
                    <h3>Contato Direto</h3>
                    <p>📧 contato@diogocosta.dev</p>
                    <p>📱 WhatsApp: (XX) XXXXX-XXXX</p>
                    <p>💼 LinkedIn: /in/diogocostadev</p>
                </div>
                
                <div class="stats-card">
                    <h3>Números</h3>
                    <ul>
                        <li><strong>X+</strong> Anos de experiência</li>
                        <li><strong>XX+</strong> Projetos entregues</li>
                        <li><strong>XX+</strong> Clientes satisfeitos</li>
                        <li><strong>100%</strong> Projetos no prazo</li>
                    </ul>
                </div>
            </div>
        </div>
    </div>
</div>
```

---

### **3. ARTIGOS DE BLOG OTIMIZADOS**

#### **Template para Artigos (Views/Blog/Artigo.cshtml)**

```html
@model BlogArticleViewModel
@{
    ViewBag.Title = $"{Model.Title} | Blog - Diogo Costa";
    ViewBag.Description = Model.Description;
    ViewBag.Keywords = Model.Keywords;
    ViewBag.CanonicalUrl = $"https://site.diogocosta.dev/blog/{Model.Slug}";
}

<article class="blog-article">
    <div class="container">
        <div class="row">
            <div class="col-lg-8">
                <header class="article-header">
                    <h1>@Model.Title</h1>
                    <div class="article-meta">
                        <time datetime="@Model.PublishDate.ToString("yyyy-MM-dd")">
                            @Model.PublishDate.ToString("dd/MM/yyyy")
                        </time>
                        <span class="reading-time">@Model.ReadingTime min de leitura</span>
                        <div class="tags">
                            @foreach(var tag in Model.Tags)
                            {
                                <span class="tag">#@tag</span>
                            }
                        </div>
                    </div>
                </header>
                
                <div class="article-content">
                    @Html.Raw(Model.Content)
                </div>
                
                <footer class="article-footer">
                    <div class="author-bio">
                        <h3>Sobre o Autor</h3>
                        <p>
                            <strong>Diogo Costa</strong> é programador freelancer especializado em 
                            desenvolvimento web com ASP.NET. Como consultor tecnológico, ajuda 
                            empresas a criar soluções personalizadas e automação de processos.
                        </p>
                        <a href="/sobre" class="btn btn-outline-primary">Saiba Mais</a>
                    </div>
                    
                    <div class="cta-box">
                        <h3>Precisa de Ajuda com Desenvolvimento Web?</h3>
                        <p>
                            Se você tem um projeto em mente ou precisa de consultoria tecnológica, 
                            vamos conversar sobre como posso ajudar.
                        </p>
                        <a href="/contato" class="btn btn-primary">Solicitar Orçamento</a>
                    </div>
                </footer>
            </div>
            
            <aside class="col-lg-4">
                <div class="sidebar">
                    <div class="related-articles">
                        <h3>Artigos Relacionados</h3>
                        <!-- Lista de artigos relacionados -->
                    </div>
                    
                    <div class="newsletter-signup">
                        <h3>Newsletter</h3>
                        <p>Receba dicas sobre desenvolvimento web e consultoria tecnológica.</p>
                        <form class="newsletter-form">
                            <input type="email" placeholder="Seu email" required>
                            <button type="submit" class="btn btn-primary">Inscrever</button>
                        </form>
                    </div>
                </div>
            </aside>
        </div>
    </div>
</article>

<!-- Schema para Artigo -->
<script type="application/ld+json">
{
    "@@context": "https://schema.org",
    "@@type": "BlogPosting",
    "headline": "@Model.Title",
    "description": "@Model.Description",
    "image": "@Model.FeaturedImage",
    "author": {
        "@@type": "Person",
        "name": "Diogo Costa",
        "url": "https://site.diogocosta.dev/sobre"
    },
    "publisher": {
        "@@type": "Organization",
        "name": "Diogo Costa - Programador Freelancer",
        "logo": {
            "@@type": "ImageObject",
            "url": "https://site.diogocosta.dev/images/logo.png"
        }
    },
    "datePublished": "@Model.PublishDate.ToString("yyyy-MM-dd")",
    "dateModified": "@Model.LastModified.ToString("yyyy-MM-dd")",
    "mainEntityOfPage": {
        "@@type": "WebPage",
        "@@id": "https://site.diogocosta.dev/blog/@Model.Slug"
    }
}
</script>
```

---

### **4. CONFIGURAÇÃO DE CONTROLLERS**

#### **Blog Controller Otimizado**

```csharp
using Microsoft.AspNetCore.Mvc;
using site.diogocosta.dev.Models;

namespace site.diogocosta.dev.Controllers
{
    public class BlogController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Title = "Blog - Desenvolvimento Web e Consultoria Tecnológica";
            ViewBag.Description = "Artigos sobre desenvolvimento web, ASP.NET, Angular e consultoria tecnológica. Dicas práticas para programadores e empresários.";
            ViewBag.Keywords = "blog desenvolvimento web, artigos ASP.NET, consultoria tecnológica, programação";
            
            return View();
        }
        
        public IActionResult Como_Escolher_Programador_Freelancer()
        {
            ViewBag.Title = "Como Escolher um Programador Freelancer para Seu Projeto Web";
            ViewBag.Description = "Guia completo para empresas que precisam contratar um programador freelancer. Dicas, critérios e o que avaliar antes de fechar contrato.";
            ViewBag.Keywords = "programador freelancer, desenvolvimento web, como contratar programador";
            ViewBag.CanonicalUrl = "https://site.diogocosta.dev/blog/como-escolher-programador-freelancer";
            
            return View();
        }
        
        public IActionResult ASP_NET_vs_Outras_Tecnologias()
        {
            ViewBag.Title = "ASP.NET vs Outras Tecnologias: Guia para Empresários";
            ViewBag.Description = "Comparação técnica entre ASP.NET e outras plataformas de desenvolvimento web. Vantagens, desvantagens e quando usar cada tecnologia.";
            ViewBag.Keywords = "ASP.NET developer, desenvolvimento web, tecnologias web, comparação frameworks";
            ViewBag.CanonicalUrl = "https://site.diogocosta.dev/blog/aspnet-vs-outras-tecnologias";
            
            return View();
        }
    }
}
```

#### **Serviços Controller**

```csharp
using Microsoft.AspNetCore.Mvc;

namespace site.diogocosta.dev.Controllers
{
    public class ServicosController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Title = "Serviços - Desenvolvimento Web e Consultoria Tecnológica";
            ViewBag.Description = "Desenvolvimento de sites profissionais, sistemas web personalizados, consultoria tecnológica e automação de processos para pequenas empresas.";
            ViewBag.Keywords = "serviços desenvolvimento web, consultoria tecnológica, programador freelancer, ASP.NET";
            ViewBag.CanonicalUrl = "https://site.diogocosta.dev/servicos";
            
            return View();
        }
        
        public IActionResult Desenvolvimento_Web()
        {
            ViewBag.Title = "Desenvolvimento Web ASP.NET - Serviços Profissionais";
            ViewBag.Description = "Criação de sites e sistemas web usando ASP.NET Core e Angular. Soluções personalizadas, responsivas e otimizadas para pequenas empresas.";
            ViewBag.Keywords = "desenvolvimento web ASP.NET, criação de sites, sistemas web personalizados";
            
            return View();
        }
        
        public IActionResult Consultoria_Tecnologica()
        {
            ViewBag.Title = "Consultoria Tecnológica - Especialista em Soluções Web";
            ViewBag.Description = "Análise de necessidades, arquitetura de sistemas e planejamento estratégico. Consultoria especializada para decisões tecnológicas inteligentes.";
            ViewBag.Keywords = "consultoria tecnológica, consultor sistemas web, arquitetura software";
            
            return View();
        }
        
        public IActionResult Automacao_Processos()
        {
            ViewBag.Title = "Automação de Processos - Otimização Empresarial";
            ViewBag.Description = "Desenvolvimento de soluções para automação de processos empresariais. Redução de trabalho manual, integração de sistemas e otimização de fluxos.";
            ViewBag.Keywords = "automação de processos, otimização empresarial, integração sistemas";
            
            return View();
        }
    }
}
```

---

### **5. SITEMAP DINÂMICO**

#### **Controller para Sitemap**

```csharp
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace site.diogocosta.dev.Controllers
{
    public class SitemapController : Controller
    {
        [Route("/sitemap.xml")]
        public IActionResult Index()
        {
            var sitemap = GenerateSitemap();
            return Content(sitemap, "application/xml", Encoding.UTF8);
        }
        
        private string GenerateSitemap()
        {
            var baseUrl = "https://site.diogocosta.dev";
            var xml = new StringBuilder();
            
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\" xmlns:image=\"http://www.google.com/schemas/sitemap-image/1.1\">");
            
            // Página inicial
            xml.AppendLine($"<url>");
            xml.AppendLine($"  <loc>{baseUrl}/</loc>");
            xml.AppendLine($"  <lastmod>{DateTime.Now:yyyy-MM-dd}</lastmod>");
            xml.AppendLine($"  <changefreq>weekly</changefreq>");
            xml.AppendLine($"  <priority>1.0</priority>");
            xml.AppendLine($"  <image:image>");
            xml.AppendLine($"    <image:loc>{baseUrl}/images/hero-programmer.jpg</image:loc>");
            xml.AppendLine($"    <image:caption>Programador freelancer especializado em desenvolvimento web</image:caption>");
            xml.AppendLine($"  </image:image>");
            xml.AppendLine($"</url>");
            
            // Sobre
            xml.AppendLine($"<url>");
            xml.AppendLine($"  <loc>{baseUrl}/sobre</loc>");
            xml.AppendLine($"  <lastmod>{DateTime.Now:yyyy-MM-dd}</lastmod>");
            xml.AppendLine($"  <changefreq>monthly</changefreq>");
            xml.AppendLine($"  <priority>0.8</priority>");
            xml.AppendLine($"</url>");
            
            // Serviços
            xml.AppendLine($"<url>");
            xml.AppendLine($"  <loc>{baseUrl}/servicos</loc>");
            xml.AppendLine($"  <lastmod>{DateTime.Now:yyyy-MM-dd}</lastmod>");
            xml.AppendLine($"  <changefreq>monthly</changefreq>");
            xml.AppendLine($"  <priority>0.9</priority>");
            xml.AppendLine($"</url>");
            
            // Blog
            xml.AppendLine($"<url>");
            xml.AppendLine($"  <loc>{baseUrl}/blog</loc>");
            xml.AppendLine($"  <lastmod>{DateTime.Now:yyyy-MM-dd}</lastmod>");
            xml.AppendLine($"  <changefreq>weekly</changefreq>");
            xml.AppendLine($"  <priority>0.7</priority>");
            xml.AppendLine($"</url>");
            
            xml.AppendLine("</urlset>");
            return xml.ToString();
        }
    }
}
```

---

## 🚀 **COMO IMPLEMENTAR**

### **Ordem de Implementação:**

1. **Substitua** o `<head>` do `_Layout.cshtml`
2. **Atualize** as páginas Home e Sobre
3. **Crie** a página de Serviços
4. **Configure** os Controllers
5. **Teste** tudo no navegador
6. **Valide** com ferramentas SEO

### **Comandos para Testar:**

```bash
# Build e run
dotnet build
dotnet run

# Verificar no navegador
# http://localhost:5000 - Página inicial
# http://localhost:5000/sobre - Página sobre
# http://localhost:5000/servicos - Página serviços
# http://localhost:5000/sitemap.xml - Sitemap
```

### **Validação:**

1. **Código-fonte**: Verificar meta tags
2. **PageSpeed**: [pagespeed.web.dev](https://pagespeed.web.dev)
3. **Schema**: [search.google.com/test/rich-results](https://search.google.com/test/rich-results)
4. **Mobile**: [search.google.com/test/mobile-friendly](https://search.google.com/test/mobile-friendly)

---

**✅ Todos os códigos estão prontos para uso! Copie, cole e ajuste conforme necessário.**
