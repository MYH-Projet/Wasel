using System.ComponentModel.DataAnnotations;
using Wasel.Api.Modules.Documents.Enums;

namespace Wasel.Api.Modules.Documents.DTOs;

public class AddDriverDocumentRequestDto
{
    [Required]
    public DocumentType DocumentType { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string ObjectKey { get; set; } = string.Empty;
}
