using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ZiraLink.Domain.Enums;

namespace ZiraLink.Domain
{
    public class Project : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public Guid ViewId { get; set; }
        [ForeignKey("CustomerId")]
        public long CustomerId { get; set; }
        public string Title { get; set; }
        public DomainType DomainType { get; set; }
        public string Domain { get; set; }
        public string InternalUrl { get; set; }
        public ProjectState State { get; set; }

        public Customer Customer { get; set; }
    }
}
