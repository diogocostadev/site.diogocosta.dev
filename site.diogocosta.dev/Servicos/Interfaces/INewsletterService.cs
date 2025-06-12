using site.diogocosta.dev.Contratos.Entrada;

namespace site.diogocosta.dev.Servicos.Interfaces;

public interface INewsletterService
{
    /// <summary>
    /// Cadastra um usuário na newsletter
    /// </summary>
    /// <param name="usuario">Dados do usuário a ser cadastrado</param>
    /// <returns>True se o cadastro foi bem-sucedido, False caso contrário</returns>
    Task<bool> CadastrarUsuarioAsync(UsuarioNewsletter usuario);
}