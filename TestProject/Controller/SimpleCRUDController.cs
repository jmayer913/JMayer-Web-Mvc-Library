using JMayer.Data.Database.DataLayer;
using JMayer.Web.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using TestProject.Data;
using TestProject.Database;

namespace TestProject.Controller;

/// <summary>
/// The class manages HTTP requests for CRUD operations associated with the simple data object and simple memory data layer.
/// </summary>
internal class SimpleCRUDController : StandardCRUDController<SimpleDataObject, SimpleListDataLayer>
{
    /// <summary>
    /// The dependency injection constructor.
    /// </summary>
    /// <param name="dataLayer">The data layer the controller will interact with.</param>
    /// <param name="logger">The logger the controller will interact with.</param>
    public SimpleCRUDController(IDataLayer<SimpleDataObject> dataLayer, ILogger logger) : base(dataLayer, logger) { }
}
