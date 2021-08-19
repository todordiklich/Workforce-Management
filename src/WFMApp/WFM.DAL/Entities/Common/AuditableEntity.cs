using System;

namespace WFM.DAL.Entities.Common
{
    public abstract class AuditableEntity : Entity, IAuditableEntity, IDeletableEntity
    {
        public Guid CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid ModifiedBy { get; set; }

        public DateTime ModifiedOn { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime DeletedOn { get; set; }
    }
}
