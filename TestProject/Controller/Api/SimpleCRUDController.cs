using JMayer.Data.Database.DataLayer;
using JMayer.Web.Mvc.Controller;
using Microsoft.Extensions.Logging;
using TestProject.Data;

namespace TestProject.Controller.Api;

/// <summary>
/// The class manages HTTP requests for CRUD operations associated with the simple data object and simple memory data layer.
/// </summary>
internal class SimpleCRUDController : StandardCRUDController<SimpleDataObject, IStandardCRUDDataLayer<SimpleDataObject>>
{
    /// <inheritdoc/>
    public SimpleCRUDController(IStandardCRUDDataLayer<SimpleDataObject> dataLayer, ILogger logger) : base(dataLayer, logger) { }
}
