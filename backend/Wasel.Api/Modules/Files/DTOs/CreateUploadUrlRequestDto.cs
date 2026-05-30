using System.ComponentModel.DataAnnotations;
using Wasel.Api.Modules.Files.Enums;

namespace Wasel.Api.Modules.Files.DTOs;

public class CreateUploadUrlRequestDto
{
    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string FileType { get; set; } = string.Empty;

    [Required]
    public FileContext Context { get; set; }
}
