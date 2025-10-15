using JMayer.Data.Database.DataLayer;
using JMayer.Data.Database.DataLayer.MemoryStorage;
using System.Linq.Expressions;
using TestProject.Data;

namespace TestProject.Database;

/// <summary>
/// The class manages CRUD interactions with a list memory storage for the simple data object.
/// </summary>
public class SimpleStandardCRUDDataLayer : StandardCRUDDataLayer<SimpleDataObject>
{
    /// <summary>
    /// The constant for the delete conflict id.
    /// </summary>
    public const int DeleteConflictId = 99;

    /// <inheritdoc/>
    /// <remarks>
    /// Overridden to test the handling of DataObjectDeleteConflictException.
    /// </remarks>
    public override async Task DeleteAsync(SimpleDataObject dataObject, CancellationToken cancellationToken = default)
    {
        if (dataObject.Integer64ID == DeleteConflictId)
        {
            throw new DataObjectDeleteConflictException();
        }

        await base.DeleteAsync(dataObject, cancellationToken);
    }
}
