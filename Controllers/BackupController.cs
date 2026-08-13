using CosturaProducao.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosturaProducao.Controllers;

public class BackupController : Controller
{
    private readonly BackupService _backupService;

    public BackupController(BackupService backupService)
    {
        _backupService = backupService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar()
    {
        try
        {
            var caminhoArquivo =
                await _backupService.CriarBackupAsync();

            var nomeArquivo =
                Path.GetFileName(caminhoArquivo);

            var stream = new FileStream(
                caminhoArquivo,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 64 * 1024,
                useAsync: true);

            Response.OnCompleted(() =>
            {
                try
                {
                    if (System.IO.File.Exists(caminhoArquivo))
                        System.IO.File.Delete(caminhoArquivo);
                }
                catch
                {
                    // Não interrompe o download por erro de limpeza.
                }

                return Task.CompletedTask;
            });

            return File(
                stream,
                "application/vnd.rar",
                nomeArquivo);
        }
        catch (Exception ex)
        {
            TempData["BackupErro"] = ex.Message;

            return RedirectToAction(nameof(Index));
        }
    }
}