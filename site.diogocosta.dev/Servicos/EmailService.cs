using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos.Interfaces;

namespace site.diogocosta.dev.Servicos;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task<bool> EnviarEmailAsync(string destinatario, string assunto, string corpo, bool isHtml = true)
    {
        try
        {
            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password);
            client.EnableSsl = true;

            using var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
            mailMessage.To.Add(destinatario);
            mailMessage.Subject = assunto;
            mailMessage.Body = corpo;
            mailMessage.IsBodyHtml = isHtml;

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email enviado com sucesso para {Email}", destinatario);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email para {Email}", destinatario);
            return false;
        }
    }

    public async Task<bool> EnviarEmailDownloadAsync(string email, string nome)
    {
        try
        {
            var nomeUsuario = !string.IsNullOrEmpty(nome) ? nome : "Caro(a) leitor(a)";
            var downloadUrl = "https://diogocosta.dev/desbloqueio/download-pdf";

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