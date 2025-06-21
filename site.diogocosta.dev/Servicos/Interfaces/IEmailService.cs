namespace site.diogocosta.dev.Servicos.Interfaces;

public interface IEmailService
{
    Task<bool> EnviarEmailAsync(string destinatario, string assunto, string corpo, bool isHtml = true);
    Task<bool> EnviarEmailDownloadAsync(string email, string nome);
} 