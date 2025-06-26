using System.ComponentModel.DataAnnotations;

namespace site.diogocosta.dev.Models
{
    public class DesafioModel
    {
        public string Nome { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Subtitulo { get; set; } = string.Empty;
        public string Produto { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public List<string> Funcionalidades { get; set; } = new List<string>();
        public List<string> Aprendizados { get; set; } = new List<string>();
        public decimal Valor { get; set; } = 197;
        public string CheckoutUrl { get; set; } = string.Empty;
        public bool Ativo { get; set; } = true;
        public bool EhVSL { get; set; } = false;
        public string VideoUrl { get; set; } = string.Empty;
        public decimal PrecoOriginal { get; set; } = 997;
        public decimal PrecoPromocional { get; set; } = 197;
        public DateTime? ValidadePromocao { get; set; }
        
        // Seção de Autoridade
        public AutoridadeModel Autoridade { get; set; } = new AutoridadeModel();
        
        // Seção de Pré-Requisitos
        public PreRequisitosModel PreRequisitos { get; set; } = new PreRequisitosModel();
    }

    public class AutoridadeModel
    {
        public string Nome { get; set; } = "Diogo Costa";
        public string Titulo { get; set; } = "Founder. Desenvolvedor. MVP Microsoft. Criador do DC360 e do Desafio SaaS.";
        public string Bio { get; set; } = "Eu não sou designer. Eu não sou influencer. Eu sou desenvolvedor há mais de 20 anos, vivendo de software, SaaS, deploy, produto e código real.";
        public string Missao { get; set; } = "Já criei outros cursos, mas este é diferente. Aqui eu vendi e construí software. SaaS. Produto. Empresa. Ajudei empresas, clientes e agora decidi fazer diferente: Ensinar programadores a saírem do ciclo de freelas, empregos que não escalam, e se tornarem donos de produtos digitais.";
        public string Filosofia { get; set; } = "Aqui não tem fórmula mágica. Aqui não tem click-deploy. Aqui não tem no-code. Aqui tem VPS. API. Banco. Stripe. Deploy. Código real. Produto no ar. Cliente pagando.";
        public string Promessa { get; set; } = "Isso não é um curso. Isso é um sistema. Isso é sua empresa em construção.";
        public List<string> Tecnologias { get; set; } = new List<string> 
        { 
            "Microsoft MVP Developer Technologies",
            ".NET & C#", 
            "PostgreSQL", 
            "Redis", 
            "Docker", 
            "Stripe", 
            "DevOps", 
            "VPS & Deploy", 
            "SaaS Architecture" 
        };
        public string ProvaSocial { get; set; } = "+20 anos de software. Primeira turma. Primeiros SaaS saindo do forno.";
        public string Garantia { get; set; } = "Se você fizer o desafio, seguir as aulas, e não tiver seu SaaS no ar, eu te ajudo pessoalmente até colocar. Aqui é compromisso. Aqui é guerra. Aqui é produto real.";
        public string Microcopy { get; set; } = "Se você tá procurando mágica, não é aqui. Aqui é código. Aqui é empresa.";
    }

    public class PreRequisitosModel
    {
        public string Titulo { get; set; } = "🚩 Antes de Começar — Alinhamento de Expectativa";
        public string Aviso { get; set; } = "🛑 Isso não é um curso pra qualquer um. Isso aqui é pra quem sabe pelo menos o básico de programação em C#.";
        public string Requisito { get; set; } = "Se você já entende variáveis, classes, métodos e APIs REST... você tá dentro. Não precisa ser senior. Mas precisa saber programar.";
        public List<string> Necessario { get; set; } = new List<string>
        {
            "🧠 Conhecimento básico de C# e lógica de programação.",
            "🔥 Vontade de ter um SaaS no ar.",
            "🌐 Uma VPS e um domínio (DNS)."
        };
        public string Explicacao { get; set; } = "Se não tiver VPS ou domínio, eu te mostro exatamente como contratar, configurar e subir.";
        public string Filosofia { get; set; } = "Porque aqui não tem click-deploy. Aqui não tem mágica. Aqui é produto real, rodando em servidor próprio, do jeito certo.";
        public string Tranquilidade { get; set; } = "Se você nunca mexeu com VPS ou domínio… não tem problema. Eu te guio no processo. Você aprende. Você executa. Você domina.";
        public string ObjetivoFinal { get; set; } = "E sai do outro lado dono do seu próprio sistema.";
        public List<string> ParaQuem { get; set; } = new List<string>
        {
            "✔️ Ter um SaaS no ar.",
            "✔️ Ter domínio próprio.",
            "✔️ Rodar API, banco, deploy, SSL e faturamento.",
            "✔️ E nunca mais depender de template, no-code, ou plataforma de terceiros."
        };
        public string Filtro { get; set; } = "Se não quer isso... esse não é seu lugar.";
        public string Chamada { get; set; } = "Se quer... Bora. Aqui é guerra. Aqui é produto. Aqui é Founder.";
        public string MicrocopyRodape { get; set; } = "🧠 É necessário saber C# (nível básico já funciona) e ter uma VPS + domínio próprio. Se não tiver, te ensino no desafio como subir, configurar e colocar no ar.";
    }

    public class DesafioLeadModel
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        public string DesafioSlug { get; set; } = string.Empty;
    }
} 