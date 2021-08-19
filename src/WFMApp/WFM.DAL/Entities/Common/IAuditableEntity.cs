using System;

namespace WFM.DAL.Entities.Common
{
    public interface IAuditableEntity
    {
        DateTime CreatedOn { get; set; }

        Guid CreatedBy { get; set; }

        DateTime ModifiedOn { get; set; }

        Guid ModifiedBy { get; set; }
    }
}