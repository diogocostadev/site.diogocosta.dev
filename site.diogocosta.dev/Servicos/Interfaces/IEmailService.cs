namespace site.diogocosta.dev.Servicos.Interfaces;

public interface IEmailService
{
    Task<bool> EnviarEmailAsync(string destinatario, string assunto, string corpoHtml, string? corpoTexto = null, bool isHtml = true);
    Task<bool> EnviarEmailDownloadAsync(string email, string nome);
} 