using Microsoft.AspNetCore.Identity;

namespace CosturaProducao.Models;

public sealed class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
}