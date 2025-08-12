using System.ComponentModel.DataAnnotations;

namespace YoutubeDiscogsWantlist.Core.WantList;

public class WantListItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public required string UserName { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Artist { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }

    [MaxLength(10)]
    public required int Year { get; set; }

    [MaxLength(200)]
    public required string Label { get; set; }

}