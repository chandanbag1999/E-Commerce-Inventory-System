namespace EcommerceInventory.Domain.Common;


// Entity with audit tracking (created_by, updated_by)
public abstract class AuditableEntity : BaseEntity
{
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
