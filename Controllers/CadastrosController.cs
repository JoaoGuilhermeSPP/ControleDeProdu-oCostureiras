using CosturaProducao.Data;
using CosturaProducao.Models;
using CosturaProducao.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CosturaProducao.Controllers;


public sealed class CadastrosController(ApplicationDbContext db, IWebHostEnvironment environment) : Controller
{
    private static readonly string[] Tipos = ["costureiras", "clientes", "servicos", "pecas"];

    public async Task<IActionResult> Index(string tipo = "costureiras")
    {
        if (!Tipos.Contains(tipo)) return NotFound();
        var itens = tipo switch
        {
            "costureiras" => await db.Seamstresses.AsNoTracking().OrderBy(x => x.Name).Select(x => new CadastroRowVm(x.Id, x.Name, null, x.Phone, x.Active)).ToListAsync(),
            "clientes" => await db.Clients.AsNoTracking().OrderBy(x => x.Name).Select(x => new CadastroRowVm(x.Id, x.Name, null, x.Phone, x.Active)).ToListAsync(),
            "servicos" => await db.ServiceProcesses.AsNoTracking().OrderBy(x => x.Name).Select(x => new CadastroRowVm(x.Id, x.Name, null, $"R$ {x.DefaultPricePerPiece:N2}", x.Active)).ToListAsync(),
            _ => await db.PieceModels.AsNoTracking().OrderBy(x => x.Name).Select(x => new CadastroRowVm(x.Id, x.Name, x.Code, x.Color, x.Active)).ToListAsync()
        };
        return View(new CadastroIndexVm { Tipo = tipo, Titulo = Titulo(tipo), Itens = itens });
    }

    [HttpGet]
    public IActionResult Create(string tipo) => Tipos.Contains(tipo) ? View(new CadastroInputVm { Tipo = tipo }) : NotFound();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CadastroInputVm input)
    {
        if (!Tipos.Contains(input.Tipo)) return NotFound();
        if (string.IsNullOrWhiteSpace(input.Nome)) ModelState.AddModelError(nameof(input.Nome), "Informe o nome.");
        if (input.Tipo == "servicos" && (input.ValorPorPeca is null || input.ValorPorPeca < 0))
            ModelState.AddModelError(nameof(input.ValorPorPeca), "Informe um valor por peça igual ou maior que zero.");
        if (input.Tipo == "pecas" && string.IsNullOrWhiteSpace(input.Codigo))
            ModelState.AddModelError(nameof(input.Codigo), "Informe o código da peça.");
        if (input.Tipo == "pecas" && !string.IsNullOrWhiteSpace(input.Codigo) && await db.PieceModels.AnyAsync(x => x.Code == input.Codigo.Trim()))
            ModelState.AddModelError(nameof(input.Codigo), "Esse código já está cadastrado.");
        if (input.Tipo == "pecas" && input.Gabarito is not null)
        {
            var extension = Path.GetExtension(input.Gabarito.FileName).ToLowerInvariant();
            if (input.Gabarito.Length > 5 * 1024 * 1024) ModelState.AddModelError(nameof(input.Gabarito), "O gabarito deve ter no máximo 5 MB.");
            if (!new[] { ".png", ".jpg", ".jpeg", ".webp" }.Contains(extension)) ModelState.AddModelError(nameof(input.Gabarito), "Use PNG, JPG ou WEBP.");
        }
        if (!ModelState.IsValid) return View(input);

        string? savedTemplatePath = null;
        try
        {
            switch (input.Tipo)
            {
                case "costureiras": db.Seamstresses.Add(new Seamstress { Name = input.Nome.Trim(), Phone = input.Telefone, Address = input.Endereco, Notes = input.Observacoes }); break;
                case "clientes": db.Clients.Add(new Client { Name = input.Nome.Trim(), Phone = input.Telefone, Address = input.Endereco, Notes = input.Observacoes }); break;
                case "servicos": db.ServiceProcesses.Add(new ServiceProcess { Name = input.Nome.Trim(), Description = input.Descricao, DefaultPricePerPiece = input.ValorPorPeca ?? 0 }); break;
                case "pecas":
                    savedTemplatePath = await SaveTemplateAsync(input.Gabarito);
                    db.PieceModels.Add(new PieceModel { Name = input.Nome.Trim(), Code = input.Codigo!.Trim(), Color = input.Cor, Description = input.Descricao, TemplateImagePath = savedTemplatePath });
                    break;
            }
            await db.SaveChangesAsync();
        }
        catch (InvalidOperationException ex)
        {
            if (savedTemplatePath is not null) DeleteTemplate(savedTemplatePath);
            ModelState.AddModelError(nameof(input.Gabarito), ex.Message);
            return View(input);
        }
        catch
        {
            if (savedTemplatePath is not null) DeleteTemplate(savedTemplatePath);
            throw;
        }
        TempData["Success"] = $"{Titulo(input.Tipo)} salvo com sucesso.";
        return RedirectToAction(nameof(Index), new { tipo = input.Tipo });
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id, string tipo)
    {
        if (!Tipos.Contains(tipo))
            return NotFound();

        CadastroInputVm? model = tipo switch
        {
            "costureiras" => await db.Seamstresses
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new CadastroInputVm
                {
                    Id = x.Id,
                    Tipo = tipo,
                    Nome = x.Name,
                    Telefone = x.Phone,
                    Endereco = x.Address,
                    Observacoes = x.Notes
                })
                .FirstOrDefaultAsync(),

            "clientes" => await db.Clients
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new CadastroInputVm
                {
                    Id = x.Id,
                    Tipo = tipo,
                    Nome = x.Name,
                    Telefone = x.Phone,
                    Endereco = x.Address,
                    Observacoes = x.Notes
                })
                .FirstOrDefaultAsync(),

            "servicos" => await db.ServiceProcesses
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new CadastroInputVm
                {
                    Id = x.Id,
                    Tipo = tipo,
                    Nome = x.Name,
                    Descricao = x.Description,
                    ValorPorPeca = x.DefaultPricePerPiece
                })
                .FirstOrDefaultAsync(),

            "pecas" => await db.PieceModels
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new CadastroInputVm
                {
                    Id = x.Id,
                    Tipo = tipo,
                    Nome = x.Name,
                    Codigo = x.Code,
                    Cor = x.Color,
                    Descricao = x.Description,
                    GabaritoAtual = x.TemplateImagePath
                })
                .FirstOrDefaultAsync(),

            _ => null
        };

        if (model is null)
            return NotFound();

        return View(model);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CadastroInputVm input)
    {
        if (!Tipos.Contains(input.Tipo))
            return NotFound();

        if (input.Id <= 0)
            return NotFound();

        if (string.IsNullOrWhiteSpace(input.Nome))
            ModelState.AddModelError(nameof(input.Nome), "Informe o nome.");

        if (input.Tipo == "servicos" &&
            (input.ValorPorPeca is null || input.ValorPorPeca < 0))
        {
            ModelState.AddModelError(
                nameof(input.ValorPorPeca),
                "Informe um valor por peça igual ou maior que zero."
            );
        }

        if (input.Tipo == "pecas" &&
            string.IsNullOrWhiteSpace(input.Codigo))
        {
            ModelState.AddModelError(
                nameof(input.Codigo),
                "Informe o código da peça."
            );
        }

        if (input.Tipo == "pecas" &&
            !string.IsNullOrWhiteSpace(input.Codigo) &&
            await db.PieceModels.AnyAsync(x =>
                x.Code == input.Codigo.Trim() &&
                x.Id != input.Id))
        {
            ModelState.AddModelError(
                nameof(input.Codigo),
                "Esse código já está cadastrado."
            );
        }

        if (!ModelState.IsValid)
            return View(input);

        switch (input.Tipo)
        {
            case "costureiras":
                {
                    var costureira = await db.Seamstresses.FindAsync(input.Id);

                    if (costureira is null)
                        return NotFound();

                    costureira.Name = input.Nome.Trim();
                    costureira.Phone = input.Telefone;
                    costureira.Address = input.Endereco;
                    costureira.Notes = input.Observacoes;

                    break;
                }

            case "clientes":
                {
                    var cliente = await db.Clients.FindAsync(input.Id);

                    if (cliente is null)
                        return NotFound();

                    cliente.Name = input.Nome.Trim();
                    cliente.Phone = input.Telefone;
                    cliente.Address = input.Endereco;
                    cliente.Notes = input.Observacoes;

                    break;
                }

            case "servicos":
                {
                    var servico = await db.ServiceProcesses.FindAsync(input.Id);

                    if (servico is null)
                        return NotFound();

                    servico.Name = input.Nome.Trim();
                    servico.Description = input.Descricao;
                    servico.DefaultPricePerPiece = input.ValorPorPeca ?? 0;

                    break;
                }

            case "pecas":
                {
                    var peca = await db.PieceModels.FindAsync(input.Id);

                    if (peca is null)
                        return NotFound();

                    peca.Name = input.Nome.Trim();
                    peca.Code = input.Codigo!.Trim();
                    peca.Color = input.Cor;
                    peca.Description = input.Descricao;

                    // Se foi enviado um novo gabarito
                    if (input.Gabarito is not null &&
                        input.Gabarito.Length > 0)
                    {
                        var oldTemplate = peca.TemplateImagePath;

                        var newTemplate = await SaveTemplateAsync(input.Gabarito);

                        peca.TemplateImagePath = newTemplate;

                        if (!string.IsNullOrWhiteSpace(oldTemplate))
                            DeleteTemplate(oldTemplate);
                    }

                    break;
                }
        }

        await db.SaveChangesAsync();

        TempData["Success"] =
            $"{Titulo(input.Tipo)} atualizado com sucesso.";

        return RedirectToAction(
            nameof(Index),
            new { tipo = input.Tipo }
        );
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id, string tipo)
    {
        if (!Tipos.Contains(tipo)) return NotFound();
        var ativo = tipo switch
        {
            "costureiras" => (AuditableEntity?)await db.Seamstresses.FindAsync(id),
            "clientes" => await db.Clients.FindAsync(id),
            "servicos" => await db.ServiceProcesses.FindAsync(id),
            _ => await db.PieceModels.FindAsync(id)
        };
        if (ativo is null) return NotFound();
        ativo.Active = !ativo.Active;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { tipo });
    }

    [HttpPost] //Exclusão
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string tipo)
    {
        if (!Tipos.Contains(tipo))
            return NotFound();

        switch (tipo)
        {
            case "costureiras":
                {
                    var costureira = await db.Seamstresses.FindAsync(id);

                    if (costureira is null)
                        return NotFound();

                    // Não exclui do banco.
                    // Apenas desativa o cadastro.
                    costureira.Active = false;

                    await db.SaveChangesAsync();

                    TempData["Success"] = "Costureira desativada com sucesso.";

                    break;
                }

            case "clientes":
                {
                    var cliente = await db.Clients.FindAsync(id);

                    if (cliente is null)
                        return NotFound();

                    // Verifica se existem produções vinculadas.
                    var possuiProducoes = await db.Productions
                        .AnyAsync(x => x.ClientId == id);

                    if (possuiProducoes)
                    {
                        TempData["Error"] =
                            "Não é possível excluir este cliente porque existem produções vinculadas a ele.";

                        return RedirectToAction(nameof(Index), new { tipo });
                    }

                    db.Clients.Remove(cliente);

                    await db.SaveChangesAsync();

                    TempData["Success"] = "Cliente excluído com sucesso.";

                    break;
                }

            case "servicos":
                {
                    var servico = await db.ServiceProcesses.FindAsync(id);

                    if (servico is null)
                        return NotFound();

                    db.ServiceProcesses.Remove(servico);

                    await db.SaveChangesAsync();

                    TempData["Success"] = "Serviço excluído com sucesso.";

                    break;
                }

            case "pecas":
                {
                    var peca = await db.PieceModels.FindAsync(id);

                    if (peca is null)
                        return NotFound();

                    // Se a peça possuir relacionamentos com produções,
                    // precisamos impedir a exclusão.

                    db.PieceModels.Remove(peca);

                    await db.SaveChangesAsync();

                    // Remove também o arquivo do gabarito.
                    if (!string.IsNullOrWhiteSpace(peca.TemplateImagePath))
                        DeleteTemplate(peca.TemplateImagePath);

                    TempData["Success"] = "Peça excluída com sucesso.";

                    break;
                }
        }

        return RedirectToAction(nameof(Index), new { tipo });
    }
    private static string Titulo(string tipo) => tipo switch
    {
        "costureiras" => "Costureiras", "clientes" => "Clientes", "servicos" => "Serviços", "pecas" => "Peças", _ => "Cadastros"
    };

    private async Task<string?> SaveTemplateAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;
        if (file.Length > 5 * 1024 * 1024) throw new InvalidOperationException("O gabarito deve ter no máximo 5 MB.");
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new[] { ".png", ".jpg", ".jpeg", ".webp" };
        if (!allowed.Contains(extension)) throw new InvalidOperationException("Formato de gabarito não permitido.");
        await using var source = file.OpenReadStream();
        var signature = new byte[12];
        var read = await source.ReadAsync(signature);
        var validSignature = extension == ".png" && read >= 8 && signature[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })
            || (extension is ".jpg" or ".jpeg" && read >= 3 && signature[..3].SequenceEqual(new byte[] { 255, 216, 255 }))
            || extension == ".webp" && read >= 12 && signature[..4].SequenceEqual("RIFF"u8.ToArray()) && signature[8..12].SequenceEqual("WEBP"u8.ToArray());
        if (!validSignature) throw new InvalidOperationException("O conteúdo não corresponde a uma imagem válida.");
        var folder = Path.Combine(environment.WebRootPath, "uploads", "templates");
        Directory.CreateDirectory(folder);
        var filename = $"{Guid.NewGuid():N}{extension}";
        await using var stream = System.IO.File.Create(Path.Combine(folder, filename));
        await file.CopyToAsync(stream);
        return $"/uploads/templates/{filename}";
    }

    private void DeleteTemplate(string relativePath)
    {
        var filename = Path.GetFileName(relativePath);
        var path = Path.Combine(environment.WebRootPath, "uploads", "templates", filename);
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
    }

}