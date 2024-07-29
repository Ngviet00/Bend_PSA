using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bend_PSA.Models.Entities
{
    [Table("data")]
    public class Data
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("model")]
        public string? Model { get; set; }

        [Column("roll")]
        public int? Roll { get; set; }

        [Column("result_1")]
        public int? Result1 { get; set; }

        [Column("result_2")]
        public int? Result2 { get; set; }

        [Column("timeline")]
        public string? TimeLine { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Error>? Errors { get; set; }

        public ICollection<Image>? Images { get; set; }
    }
}
