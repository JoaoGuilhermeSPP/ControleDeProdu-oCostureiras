using MySqlConnector;
using System.Diagnostics;

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

        var connectionBuilder =
            new MySqlConnectionStringBuilder(connectionString);

        // Pasta temporária do sistema
        var pastaTemporaria = Path.Combine(
            Path.GetTempPath(),
            "CosturaProducao",
            "Backups");

        Directory.CreateDirectory(pastaTemporaria);

        var identificador =
            DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        var nomeSql = $"backup_{identificador}.sql";
        var nomeRar = $"backup_{identificador}.rar";

        var caminhoSql =
            Path.Combine(pastaTemporaria, nomeSql);

        var caminhoRar =
            Path.Combine(pastaTemporaria, nomeRar);

        try
        {
            // 1. Localiza o mysqldump
            var mysqldump = LocalizarExecutavel(
                "mysqldump.exe",
                "mysqldump");

            // 2. Gera o SQL
            await GerarSqlAsync(
                mysqldump,
                connectionBuilder,
                caminhoSql);

            // 3. Localiza o WinRAR
            var winrar = LocalizarWinRar();

            // 4. Compacta o SQL em RAR
            await CriarRarAsync(
                winrar,
                caminhoRar,
                caminhoSql);

            // 5. Remove o SQL temporário
            if (File.Exists(caminhoSql))
                File.Delete(caminhoSql);

            return caminhoRar;
        }
        catch
        {
            // Limpa arquivos temporários em caso de erro
            if (File.Exists(caminhoSql))
                File.Delete(caminhoSql);

            if (File.Exists(caminhoRar))
                File.Delete(caminhoRar);

            throw;
        }
    }

    private async Task GerarSqlAsync(
        string mysqldump,
        MySqlConnectionStringBuilder connection,
        string caminhoSql)
    {
        var argumentos =
            $"--host=\"{connection.Server}\" " +
            $"--port={connection.Port} " +
            $"--user=\"{connection.UserID}\" " +
            $"--password=\"{connection.Password}\" " +
            $"--routines " +
            $"--triggers " +
            $"--events " +
            $"--single-transaction " +
            $"--databases \"{connection.Database}\" " +
            $"--result-file=\"{caminhoSql}\"";

        var processo = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = mysqldump,
                Arguments = argumentos,

                RedirectStandardOutput = true,
                RedirectStandardError = true,

                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        processo.Start();

        var erro = await processo.StandardError.ReadToEndAsync();

        await processo.WaitForExitAsync();

        if (processo.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Não foi possível gerar o backup do banco de dados.{Environment.NewLine}{erro}");
        }

        if (!File.Exists(caminhoSql))
        {
            throw new InvalidOperationException(
                "O arquivo SQL do backup não foi criado.");
        }

        var tamanho = new FileInfo(caminhoSql).Length;

        if (tamanho == 0)
        {
            throw new InvalidOperationException(
                "O backup SQL foi criado, mas está vazio.");
        }
    }

    private async Task CriarRarAsync(
        string winrar,
        string caminhoRar,
        string caminhoSql)
    {
        var pasta = Path.GetDirectoryName(caminhoSql)!;
        var arquivo = Path.GetFileName(caminhoSql);

        var argumentos =
            $"a " +
            $"-ep1 " +
            $"\"{caminhoRar}\" " +
            $"\"{caminhoSql}\"";

        var processo = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = winrar,
                Arguments = argumentos,

                WorkingDirectory = pasta,

                RedirectStandardOutput = true,
                RedirectStandardError = true,

                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        processo.Start();

        var erro = await processo.StandardError.ReadToEndAsync();

        await processo.WaitForExitAsync();

        if (processo.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Não foi possível criar o arquivo RAR.{Environment.NewLine}{erro}");
        }

        if (!File.Exists(caminhoRar))
        {
            throw new InvalidOperationException(
                "O arquivo RAR não foi criado.");
        }
    }

    private string LocalizarExecutavel(
        string nomeArquivo,
        string nomePath)
    {
        // Primeiro tenta pelo PATH do Windows
        try
        {
            var processo = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = nomePath,
                    Arguments = "--version",

                    RedirectStandardOutput = true,
                    RedirectStandardError = true,

                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            processo.Start();
            processo.WaitForExit();

            return nomePath;
        }
        catch
        {
            // Continua procurando em locais conhecidos
        }

        var caminhos = new[]
        {
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ProgramFiles),
                "MySQL",
                "MySQL Server 8.0",
                "bin",
                nomeArquivo),

            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ProgramFiles),
                "MySQL",
                "MySQL Server 8.4",
                "bin",
                nomeArquivo),

            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ProgramFiles),
                "MariaDB",
                "11.0",
                "bin",
                nomeArquivo)
        };

        foreach (var caminho in caminhos)
        {
            if (File.Exists(caminho))
                return caminho;
        }

        throw new FileNotFoundException(
            $"Não foi possível localizar {nomeArquivo}. " +
            "Verifique se o MySQL/MariaDB está instalado.");
    }

    private string LocalizarWinRar()
    {
        var caminhos = new[]
        {
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ProgramFiles),
                "WinRAR",
                "WinRAR.exe"),

            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ProgramFilesX86),
                "WinRAR",
                "WinRAR.exe")
        };

        foreach (var caminho in caminhos)
        {
            if (File.Exists(caminho))
                return caminho;
        }

        // Tenta pelo PATH
        try
        {
            var processo = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "WinRAR.exe",
                    Arguments = "-?",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            processo.Start();

            return "WinRAR.exe";
        }
        catch
        {
            throw new FileNotFoundException(
                "O WinRAR não foi encontrado. " +
                "Instale o WinRAR ou configure o WinRAR.exe no instalador do CosturaProducao.");
        }
    }
}