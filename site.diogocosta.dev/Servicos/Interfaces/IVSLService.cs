using site.diogocosta.dev.Models;

namespace site.diogocosta.dev.Servicos.Interfaces
{
    public interface IVSLService
    {
        Task<VSLConfigModel?> ObterVSLPorSlugAsync(string slug);
        Task<List<VSLConfigModel>> ObterTodasVSLsAtivasAsync();
        Task<bool> TrocarAmbienteVSLAsync(string slug, string novoAmbiente);
        Task<List<VSLVideoModel>> ObterVideosAsync();
        Task<VSLVideoModel?> ObterVideoPorSlugAsync(string slug);
    }
} 