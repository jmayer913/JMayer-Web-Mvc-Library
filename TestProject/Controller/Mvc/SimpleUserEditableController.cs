using JMayer.Web.Mvc.Controller.Mvc;
using Microsoft.Extensions.Logging;
using TestProject.Data;
using TestProject.Database;

namespace TestProject.Controller.Mvc;

/// <summary>
/// The class manages HTTP action requests associated with a simple user editable data object and simple user editable memory data layer.
/// </summary>
internal class SimpleUserEditableController : StandardModelViewController<SimpleUserEditableDataObject, SimpleUserEditableDataLayer>
{
    /// <inheritdoc/>
    public SimpleUserEditableController(SimpleUserEditableDataLayer dataLayer, ILogger logger) : base(dataLayer, logger) { }
}
