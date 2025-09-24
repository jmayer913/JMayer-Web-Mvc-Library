using JMayer.Data.Database.DataLayer;
using JMayer.Web.Mvc.Controller;
using Microsoft.Extensions.Logging;
using TestProject.Data;

namespace TestProject.Controller;

/// <summary>
/// The class manages HTTP requests for CRUD operations associated with the simple sub user editable data object and simple user editable memory data layer.
/// </summary>
public class SimpleSubUserEditableController : SubUserEditableController<SimpleSubUserEditableDataObject, IUserEditableDataLayer<SimpleSubUserEditableDataObject>>
{
    /// <inheritdoc/>
    public SimpleSubUserEditableController(IUserEditableDataLayer<SimpleSubUserEditableDataObject> dataLayer, ILogger logger) : base(dataLayer, logger) { }
}
