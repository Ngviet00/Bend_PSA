using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Bend_PSA.Models.Entities
{
    [Table("images")]
    public class Image
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("data_id")]
        public Guid? DataId { get; set; }

        [Column("path_url")]
        public string? PathUrl { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonIgnore]
        public Data? Data { get; set; }
    }
}
