namespace EcommerceInventory.Domain.Common;


public interface ISoftDelete
{
    DateTime? DeletedAt { get; set; }
}
