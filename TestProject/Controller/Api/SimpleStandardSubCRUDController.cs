using JMayer.Data.Database.DataLayer;
using JMayer.Web.Mvc.Controller.Api;
using Microsoft.Extensions.Logging;
using TestProject.Data;

namespace TestProject.Controller.Api;

/// <summary>
/// The class manages HTTP requests for CRUD operations associated with the simple sub user editable data object and simple user editable memory data layer.
/// </summary>
public class SimpleStandardSubCRUDController : StandardSubCRUDController<SimpleSubDataObject, IStandardSubCRUDDataLayer<SimpleSubDataObject>>
{
    /// <inheritdoc/>
    public SimpleStandardSubCRUDController(IStandardSubCRUDDataLayer<SimpleSubDataObject> dataLayer, ILogger logger) : base(dataLayer, logger) { }
}
