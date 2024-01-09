using JMayer.Data.Database.DataLayer;
using JMayer.Web.Mvc.Controller;
using Microsoft.Extensions.Logging;
using TestProject.Data;
using TestProject.Database;

namespace TestProject.Controller;

/// <summary>
/// The class manages HTTP requests for CRUD operations associated with the simple user editable data object and simple user editable memory data layer.
/// </summary>
internal class SimpleUserEditableController : UserEditableController<SimpleUserEditableDataObject, SimpleUserEditableMemoryDataLayer>
{
    /// <inheritdoc/>
    public SimpleUserEditableController(IUserEditableDataLayer<SimpleUserEditableDataObject> dataLayer, ILogger logger) : base(dataLayer, logger) { }
}
