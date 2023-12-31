﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ZiraLink.Domain.Enums;

namespace ZiraLink.Domain
{
    public class AppProject : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public Guid ViewId { get; set; }
        public Guid? AppProjectViewId { get; set; }
        [ForeignKey("CustomerId")]
        public long CustomerId { get; set; }
        public string Title { get; set; }
        public AppProjectType AppProjectType { get; set; }
        public PortType PortType { get; set; }
        //public string AppUniqueName { get; set; }
        public int InternalPort { get; set; }
        public ProjectState State { get; set; }

        public Customer Customer { get; set; }
    }
}
