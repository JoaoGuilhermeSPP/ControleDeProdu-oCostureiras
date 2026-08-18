using System.Diagnostics;
using System.IO.Compression;
using Microsoft.Data.Sqlite;

namespace CosturaProducao.Services;

public class BackupService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public BackupService(
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public async Task<string> CriarBackupAsync()
    {
        var connectionString =
            _configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "A ConnectionString 'DefaultConnection' não foi configurada.");

        // Extrai o caminho do arquivo SQLite
        var builder = new SqliteConnectionStringBuilder(connectionString);
        var arquivoDb = builder.DataSource;

        if (!File.Exists(arquivoDb))
            throw new FileNotFoundException($"Arquivo do banco não encontrado: {arquivoDb}");

        // Pasta temporária
        var pastaTemp = Path.Combine(Path.GetTempPath(), "CosturaProducao", "Backups");
        Directory.CreateDirectory(pastaTemp);

        var identificador = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var nomeBackup = $"backup_{identificador}.db";
        var caminhoBackup = Path.Combine(pastaTemp, nomeBackup);

        var nomeZip = $"backup_{identificador}.zip";
        var caminhoZip = Path.Combine(pastaTemp, nomeZip);

        try
        {
            // 1. Faz o backup do banco (cópia segura)
            await BackupDatabaseAsync(arquivoDb, caminhoBackup);

            // 2. Compacta em ZIP
            CompactarParaZip(caminhoBackup, caminhoZip);

            // 3. Remove o arquivo .db temporário
            if (File.Exists(caminhoBackup))
                File.Delete(caminhoBackup);

            return caminhoZip;
        }
        catch
        {
            // Limpeza em caso de erro
            if (File.Exists(caminhoBackup)) File.Delete(caminhoBackup);
            if (File.Exists(caminhoZip)) File.Delete(caminhoZip);
            throw;
        }
    }

    private async Task BackupDatabaseAsync(string origem, string destino)
    {
        // Opção 1: cópia simples (requer que o banco não esteja em uso)
        // Se o banco estiver sendo usado, pode ocorrer corrupção.
        // Para maior segurança, use a API de backup do SQLite:
        using var sourceConn = new SqliteConnection($"Data Source={origem}");
        await sourceConn.OpenAsync();
        using var destConn = new SqliteConnection($"Data Source={destino}");
        await destConn.OpenAsync();
        sourceConn.BackupDatabase(destConn);
        // O BackupDatabase copia todo o conteúdo de source para destination.
    }

    private void CompactarParaZip(string arquivoOrigem, string arquivoDestino)
    {
        // Cria um ZIP contendo apenas o arquivo de backup
        using var zip = ZipFile.Open(arquivoDestino, ZipArchiveMode.Create);
        zip.CreateEntryFromFile(arquivoOrigem, Path.GetFileName(arquivoOrigem));
    }
}