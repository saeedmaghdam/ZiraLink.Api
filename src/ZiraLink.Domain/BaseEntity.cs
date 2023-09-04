using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

using ZiraLink.Domain.Enums;

namespace ZiraLink.Domain
{
    public abstract class BaseEntity
    {
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public RowState State { get; set; }
    }
}
