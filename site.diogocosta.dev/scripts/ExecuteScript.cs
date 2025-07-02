using Microsoft.EntityFrameworkCore;
using site.diogocosta.dev.Data;
using Npgsql;

namespace site.diogocosta.dev.Scripts
{
    public class ExecuteScript
    {
        public static async Task ExecuteSqlScript(string connectionString, string scriptPath)
        {
            var sql = await File.ReadAllTextAsync(scriptPath);
            
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            
            using var command = new NpgsqlCommand(sql, connection);
            command.CommandTimeout = 60;
            
            try
            {
                await command.ExecuteNonQueryAsync();
                Console.WriteLine("✅ Script executado com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao executar script: {ex.Message}");
                throw;
            }
        }
    }
}
