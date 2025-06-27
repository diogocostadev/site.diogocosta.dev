using Microsoft.EntityFrameworkCore;
using site.diogocosta.dev.Data;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos.Interfaces;

namespace site.diogocosta.dev.Servicos
{
    public class VSLService : IVSLService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VSLService> _logger;

        public VSLService(ApplicationDbContext context, ILogger<VSLService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<VSLConfigModel?> ObterVSLPorSlugAsync(string slug)
        {
            try
            {
                var vsl = await _context.VSLConfigs
                    .Where(v => v.Slug == slug && v.Ativo)
                    .FirstOrDefaultAsync();
                
                if (vsl != null)
                {
                    // Buscar URLs dos vídeos baseado no ambiente ativo
                    var videoProd = await _context.VSLVideos
                        .Where(v => v.Id == vsl.VideoId)
                        .FirstOrDefaultAsync();
                    
                    var videoTeste = await _context.VSLVideos
                        .Where(v => v.Id == vsl.VideoIdTeste)
                        .FirstOrDefaultAsync();

                    // Preencher as propriedades navegacionais
                    vsl.VideoUrlProducao = videoProd?.VideoUrl;
                    vsl.VideoUrlTeste = videoTeste?.VideoUrl;
                    vsl.ThumbnailUrlProducao = videoProd?.ThumbnailUrl;
                    vsl.ThumbnailUrlTeste = videoTeste?.ThumbnailUrl;
                    vsl.DuracaoProducao = videoProd?.DuracaoSegundos;
                    vsl.DuracaoTeste = videoTeste?.DuracaoSegundos;
                    
                    // Definir vídeo ativo baseado no ambiente
                    vsl.VideoUrlAtivo = vsl.AmbienteAtivo == "teste" ? vsl.VideoUrlTeste : vsl.VideoUrlProducao;

                    _logger.LogInformation("🎬 VSL encontrada: {Slug} - Ambiente: {Ambiente}", 
                        vsl.Slug, vsl.AmbienteAtivo);
                }
                
                return vsl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao buscar VSL: {Slug}", slug);
                return null;
            }
        }

        public async Task<List<VSLConfigModel>> ObterTodasVSLsAtivasAsync()
        {
            try
            {
                var vsls = await _context.VSLConfigs
                    .Where(v => v.Ativo)
                    .OrderByDescending(v => v.CreatedAt)
                    .ToListAsync();

                // Preencher dados dos vídeos para cada VSL
                foreach (var vsl in vsls)
                {
                    var videoProd = await _context.VSLVideos
                        .Where(v => v.Id == vsl.VideoId)
                        .FirstOrDefaultAsync();
                    
                    var videoTeste = await _context.VSLVideos
                        .Where(v => v.Id == vsl.VideoIdTeste)
                        .FirstOrDefaultAsync();

                    vsl.VideoUrlProducao = videoProd?.VideoUrl;
                    vsl.VideoUrlTeste = videoTeste?.VideoUrl;
                    vsl.VideoUrlAtivo = vsl.AmbienteAtivo == "teste" ? vsl.VideoUrlTeste : vsl.VideoUrlProducao;
                }
                
                _logger.LogInformation("📋 {Count} VSLs encontradas", vsls.Count);
                
                return vsls;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao buscar VSLs");
                return new List<VSLConfigModel>();
            }
        }

        public async Task<bool> TrocarAmbienteVSLAsync(string slug, string novoAmbiente)
        {
            try
            {
                if (novoAmbiente != "teste" && novoAmbiente != "producao")
                {
                    _logger.LogWarning("Ambiente inválido: {Ambiente}. Deve ser 'teste' ou 'producao'", novoAmbiente);
                    return false;
                }

                var vsl = await _context.VSLConfigs
                    .Where(v => v.Slug == slug && v.Ativo)
                    .FirstOrDefaultAsync();

                if (vsl == null)
                {
                    _logger.LogWarning("VSL não encontrada: {Slug}", slug);
                    return false;
                }

                vsl.AmbienteAtivo = novoAmbiente;
                vsl.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("🔄 Ambiente VSL alterado: {Slug} -> {Ambiente}", slug, novoAmbiente);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao trocar ambiente VSL: {Slug} -> {Ambiente}", 
                    slug, novoAmbiente);
                return false;
            }
        }

        public async Task<List<VSLVideoModel>> ObterVideosAsync()
        {
            try
            {
                var videos = await _context.VSLVideos
                    .Where(v => v.Ativo)
                    .OrderBy(v => v.Ambiente)
                    .ThenByDescending(v => v.CreatedAt)
                    .ToListAsync();
                
                _logger.LogInformation("🎥 {Count} vídeos encontrados", videos.Count);
                
                return videos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao buscar vídeos");
                return new List<VSLVideoModel>();
            }
        }

        public async Task<VSLVideoModel?> ObterVideoPorSlugAsync(string slug)
        {
            try
            {
                var video = await _context.VSLVideos
                    .Where(v => v.Slug == slug && v.Ativo)
                    .FirstOrDefaultAsync();
                
                if (video != null)
                {
                    _logger.LogInformation("🎥 Vídeo encontrado: {Slug} - Ambiente: {Ambiente}", 
                        video.Slug, video.Ambiente);
                }
                
                return video;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao buscar vídeo: {Slug}", slug);
                return null;
            }
        }
    }
} 