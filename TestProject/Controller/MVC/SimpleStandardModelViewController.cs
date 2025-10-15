using JMayer.Web.Mvc.Controller.Mvc;
using Microsoft.Extensions.Logging;
using TestProject.Data;
using TestProject.Database;

namespace TestProject.Controller.Mvc;

/// <summary>
/// The class manages HTTP action requests associated with a simple data object and simple memory data layer.
/// </summary>
internal class SimpleStandardModelViewController : StandardModelViewController<SimpleDataObject, SimpleStandardCRUDDataLayer>
{
    /// <inheritdoc/>
    public SimpleStandardModelViewController(SimpleStandardCRUDDataLayer dataLayer, ILogger logger) : base(dataLayer, logger) { }
}
