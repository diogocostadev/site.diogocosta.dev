using Microsoft.EntityFrameworkCore;
using site.diogocosta.dev.Models;

namespace site.diogocosta.dev.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<LeadModel> Leads { get; set; }
        public DbSet<LeadSourceModel> LeadSources { get; set; }
        public DbSet<LeadInteractionModel> LeadInteractions { get; set; }
        public DbSet<EmailTemplateModel> EmailTemplates { get; set; }
        public DbSet<EmailLogModel> EmailLogs { get; set; }
        public DbSet<VSLConfigModel> VSLConfigs { get; set; }
        public DbSet<VSLVideoModel> VSLVideos { get; set; }
        public DbSet<PdfDownloadModel> PdfDownloads { get; set; }
        public DbSet<AntiSpamRule> AntiSpamRules { get; set; }
        public DbSet<InteressadoLiveModel> InteressadosLives { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar o schema
            modelBuilder.HasDefaultSchema("leads_system");

            // Configurar a tabela de leads
            modelBuilder.Entity<LeadModel>(entity =>
            {
                entity.ToTable("leads", "leads_system");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                // Índice único composto para email + desafio
                entity.HasIndex(e => new { e.Email, e.DesafioSlug })
                      .IsUnique()
                      .HasDatabaseName("idx_leads_email_desafio_unique");

                // Outros índices
                entity.HasIndex(e => e.Email).HasDatabaseName("idx_leads_email");
                entity.HasIndex(e => e.DesafioSlug).HasDatabaseName("idx_leads_desafio_slug");
                entity.HasIndex(e => e.Status).HasDatabaseName("idx_leads_status");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("idx_leads_created_at");

                // Configurar propriedades arrays para PostgreSQL
                entity.Property(e => e.Tags)
                      .HasColumnType("text[]");

                // Relacionamentos
                entity.HasOne(e => e.LeadSource)
                      .WithMany(ls => ls.Leads)
                      .HasForeignKey(e => e.SourceId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(e => e.Interactions)
                      .WithOne(i => i.Lead)
                      .HasForeignKey(i => i.LeadId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.EmailLogs)
                      .WithOne(el => el.Lead)
                      .HasForeignKey(el => el.LeadId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configurar a tabela de lead sources
            modelBuilder.Entity<LeadSourceModel>(entity =>
            {
                entity.ToTable("lead_sources", "leads_system");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                entity.HasIndex(e => e.Slug)
                      .IsUnique()
                      .HasDatabaseName("idx_lead_sources_slug_unique");
            });

            // Configurar a tabela de interações
            modelBuilder.Entity<LeadInteractionModel>(entity =>
            {
                entity.ToTable("lead_interactions", "leads_system");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                entity.HasIndex(e => e.LeadId).HasDatabaseName("idx_interactions_lead_id");
                entity.HasIndex(e => e.Tipo).HasDatabaseName("idx_interactions_tipo");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("idx_interactions_created_at");
            });

            // Configurar a tabela de templates de email
            modelBuilder.Entity<EmailTemplateModel>(entity =>
            {
                entity.ToTable("email_templates", "leads_system");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                entity.HasIndex(e => e.Slug)
                      .IsUnique()
                      .HasDatabaseName("idx_email_templates_slug_unique");

                // Configurar propriedades arrays para PostgreSQL
                entity.Property(e => e.Variaveis)
                      .HasColumnType("text[]");
            });

            // Configurar a tabela de logs de email
            modelBuilder.Entity<EmailLogModel>(entity =>
            {
                entity.ToTable("email_logs", "leads_system");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                entity.HasIndex(e => e.LeadId).HasDatabaseName("idx_email_logs_lead_id");
                entity.HasIndex(e => e.EmailTo).HasDatabaseName("idx_email_logs_email_to");
                entity.HasIndex(e => e.Status).HasDatabaseName("idx_email_logs_status");
                entity.HasIndex(e => e.EnviadoEm).HasDatabaseName("idx_email_logs_enviado_em");

                // Relacionamentos
                entity.HasOne(e => e.EmailTemplate)
                      .WithMany(et => et.EmailLogs)
                      .HasForeignKey(e => e.TemplateId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configurar a tabela de vídeos VSL
            modelBuilder.Entity<VSLVideoModel>(entity =>
            {
                entity.ToTable("vsl_videos", "leads_system");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                entity.HasIndex(e => e.Slug)
                      .IsUnique()
                      .HasDatabaseName("idx_vsl_videos_slug_unique");
                entity.HasIndex(e => e.Ativo).HasDatabaseName("idx_vsl_videos_ativo");
                entity.HasIndex(e => e.Ambiente).HasDatabaseName("idx_vsl_videos_ambiente");
            });

            // Configurar a tabela de configurações VSL
            modelBuilder.Entity<VSLConfigModel>(entity =>
            {
                entity.ToTable("vsl_configs", "leads_system");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                entity.HasIndex(e => e.Slug)
                      .IsUnique()
                      .HasDatabaseName("idx_vsl_configs_slug_unique");
                entity.HasIndex(e => e.Ativo).HasDatabaseName("idx_vsl_configs_ativo");

                // Relacionamentos com vídeos
                entity.HasOne<VSLVideoModel>()
                      .WithMany()
                      .HasForeignKey(e => e.VideoId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne<VSLVideoModel>()
                      .WithMany()
                      .HasForeignKey(e => e.VideoIdTeste)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configurar a tabela de downloads de PDF
            modelBuilder.Entity<PdfDownloadModel>(entity =>
            {
                entity.ToTable("pdf_downloads", "leads_system");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                // Índices para performance
                entity.HasIndex(e => e.ArquivoNome).HasDatabaseName("idx_pdf_downloads_arquivo_nome");
                entity.HasIndex(e => e.Email).HasDatabaseName("idx_pdf_downloads_email");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("idx_pdf_downloads_created_at");
                entity.HasIndex(e => e.LeadId).HasDatabaseName("idx_pdf_downloads_lead_id");
                entity.HasIndex(e => e.IpAddress).HasDatabaseName("idx_pdf_downloads_ip_address");
                entity.HasIndex(e => e.Origem).HasDatabaseName("idx_pdf_downloads_origem");
                entity.HasIndex(e => e.Sucesso).HasDatabaseName("idx_pdf_downloads_sucesso");

                // Relacionamento com Lead (opcional)
                entity.HasOne(e => e.Lead)
                      .WithMany()
                      .HasForeignKey(e => e.LeadId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configurar a tabela de regras anti-spam
            modelBuilder.Entity<AntiSpamRule>(entity =>
            {
                entity.ToTable("antispam_rules", "public");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                // Índices para performance
                entity.HasIndex(e => new { e.RuleType, e.IsActive }).HasDatabaseName("idx_antispam_rules_type_active");
                entity.HasIndex(e => e.RuleValue).HasDatabaseName("idx_antispam_rules_value");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("idx_antispam_rules_created_at");
                entity.HasIndex(e => e.Severity).HasDatabaseName("idx_antispam_rules_severity");
            });
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is LeadModel || e.Entity is LeadSourceModel || e.Entity is EmailTemplateModel || 
                           e.Entity is VSLConfigModel || e.Entity is VSLVideoModel || e.Entity is PdfDownloadModel ||
                           e.Entity is AntiSpamRule)
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity is LeadModel lead)
                    {
                        lead.CreatedAt = DateTime.UtcNow;
                        lead.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (entry.Entity is LeadSourceModel source)
                    {
                        source.CreatedAt = DateTime.UtcNow;
                        source.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (entry.Entity is EmailTemplateModel template)
                    {
                        template.CreatedAt = DateTime.UtcNow;
                        template.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (entry.Entity is VSLConfigModel vslConfig)
                    {
                        vslConfig.CreatedAt = DateTime.UtcNow;
                        vslConfig.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (entry.Entity is VSLVideoModel vslVideo)
                    {
                        vslVideo.CreatedAt = DateTime.UtcNow;
                        vslVideo.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (entry.Entity is AntiSpamRule antiSpamRule)
                    {
                        antiSpamRule.CreatedAt = DateTime.UtcNow;
                        antiSpamRule.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    if (entry.Entity is LeadModel lead)
                    {
                        lead.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (entry.Entity is LeadSourceModel source)
                    {
                        source.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (entry.Entity is EmailTemplateModel template)
                    {
                        template.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (entry.Entity is VSLConfigModel vslConfig)
                    {
                        vslConfig.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (entry.Entity is VSLVideoModel vslVideo)
                    {
                        vslVideo.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (entry.Entity is AntiSpamRule antiSpamRule)
                    {
                        antiSpamRule.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }
        }
    }
} 