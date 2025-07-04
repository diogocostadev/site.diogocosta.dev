using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos.Interfaces;

namespace site.diogocosta.dev.Servicos;

// Configurações de email com nomes padronizados
public class EmailConfiguration
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

public class EmailService : IEmailService
{
    private readonly EmailConfiguration _emailConfig;
    private readonly EmailSettings _emailSettings; // Para compatibilidade
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        // Usar a nova configuração padronizada
        _emailConfig = new EmailConfiguration();
        configuration.GetSection("Email").Bind(_emailConfig);
        
        // Manter compatibilidade com configuração antiga
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task<bool> EnviarEmailAsync(string destinatario, string assunto, string corpo, bool isHtml = true)
    {
        return await EnviarEmailAsync(destinatario, assunto, corpo, null, isHtml);
    }

    public async Task<bool> EnviarEmailAsync(string destinatario, string assunto, string corpoHtml, string? corpoTexto = null, bool isHtml = true)
    {
        try
        {
            // Usar configuração nova se disponível, senão usar a antiga
            var smtpServer = !string.IsNullOrEmpty(_emailConfig.SmtpServer) ? _emailConfig.SmtpServer : _emailSettings.SmtpServer;
            var smtpPort = _emailConfig.SmtpPort > 0 ? _emailConfig.SmtpPort : _emailSettings.Port;
            var username = !string.IsNullOrEmpty(_emailConfig.SmtpUsername) ? _emailConfig.SmtpUsername : _emailSettings.UserName;
            var password = !string.IsNullOrEmpty(_emailConfig.SmtpPassword) ? _emailConfig.SmtpPassword : _emailSettings.Password;
            var fromEmail = !string.IsNullOrEmpty(_emailConfig.FromEmail) ? _emailConfig.FromEmail : _emailSettings.FromEmail;
            var fromName = !string.IsNullOrEmpty(_emailConfig.FromName) ? _emailConfig.FromName : _emailSettings.FromName;

            using var client = new SmtpClient(smtpServer, smtpPort);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(username, password);
            client.EnableSsl = true;
            client.Timeout = 30000; // 30 segundos

            using var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(fromEmail, fromName);
            mailMessage.To.Add(destinatario);
            mailMessage.Subject = assunto;
            
            if (isHtml && !string.IsNullOrEmpty(corpoHtml))
            {
                mailMessage.Body = corpoHtml;
                mailMessage.IsBodyHtml = true;
                
                // Adicionar versão texto se fornecida
                if (!string.IsNullOrEmpty(corpoTexto))
                {
                    var textView = AlternateView.CreateAlternateViewFromString(corpoTexto, null, "text/plain");
                    var htmlView = AlternateView.CreateAlternateViewFromString(corpoHtml, null, "text/html");
                    mailMessage.AlternateViews.Add(textView);
                    mailMessage.AlternateViews.Add(htmlView);
                }
            }
            else
            {
                mailMessage.Body = corpoTexto ?? corpoHtml;
                mailMessage.IsBodyHtml = false;
            }

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("✅ Email enviado com sucesso para {Email} - Assunto: {Assunto}", destinatario, assunto);
            return true;
        }
        catch (SmtpException smtpEx)
        {
            _logger.LogError(smtpEx, "❌ Erro SMTP ao enviar email para {Email}: {StatusCode} - {Erro}", 
                destinatario, smtpEx.StatusCode, smtpEx.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro geral ao enviar email para {Email}: {Erro}", destinatario, ex.Message);
            return false;
        }
    }

    public async Task<bool> EnviarEmailDownloadAsync(string email, string nome)
    {
        try
        {
            var nomeUsuario = !string.IsNullOrEmpty(nome) ? nome : "Caro(a) leitor(a)";
            // Incluir email como parâmetro na URL para rastreamento
            var downloadUrl = $"https://diogocosta.dev/desbloqueio/download-pdf?email={Uri.EscapeDataString(email)}";

            var corpoEmail = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #1f2937; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background: #f9f9f9; }}
        .download-btn {{ 
            display: inline-block; 
            background: #2563eb; 
            color: white; 
            padding: 12px 24px; 
            text-decoration: none; 
            border-radius: 6px; 
            margin: 20px 0; 
            font-weight: bold; 
        }}
        .footer {{ background: #e5e7eb; padding: 15px; text-align: center; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Manual da Primeira Virada</h1>
        </div>
        <div class='content'>
            <h2>Olá, {nomeUsuario}!</h2>
            
            <p>Obrigado por se inscrever para receber o <strong>Manual da Primeira Virada</strong>!</p>
            
            <p>Este material foi desenvolvido para ajudá-lo a quebrar o ciclo da autossabotagem e começar sua jornada de transformação hoje mesmo.</p>
            
            <p>Clique no botão abaixo para fazer o download do seu PDF:</p>
            
            <div style='text-align: center;'>
                <a href='{downloadUrl}' class='download-btn'>BAIXAR PDF AGORA</a>
            </div>
            
            <p>Se o botão não funcionar, você também pode acessar diretamente através do link:</p>
            <p><a href='{downloadUrl}'>{downloadUrl}</a></p>
            
            <p>Esperamos que este material seja útil em sua jornada de crescimento pessoal e profissional.</p>
            
            <p>Atenciosamente,<br>
            <strong>Diogo Costa</strong><br>
            MVP Microsoft</p>
        </div>
        <div class='footer'>
            <p>Este email foi enviado porque você se inscreveu em diogocosta.dev<br>
            Se você não deseja mais receber nossos emails, <a href='#'>clique aqui para cancelar</a></p>
        </div>
    </div>
</body>
</html>";

            var assunto = "Seu Manual da Primeira Virada está pronto para download!";
            
            return await EnviarEmailAsync(email, assunto, corpoEmail, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email de download para {Email}", email);
            return false;
        }
    }
} 