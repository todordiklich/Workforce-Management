using System;
using System.ComponentModel.DataAnnotations;

namespace WFM.DAL.Entities.Common
{
    public abstract class Entity
    {
        [Key]
        public Guid Id { get; set; }
    }
}
