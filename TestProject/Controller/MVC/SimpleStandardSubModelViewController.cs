using JMayer.Data.Database.DataLayer;
using JMayer.Web.Mvc.Controller.Mvc;
using Microsoft.Extensions.Logging;
using TestProject.Data;

namespace TestProject.Controller.Mvc;

/// <summary>
/// The class manages HTTP action requests associated with a simple sub data object and simple sub memory data layer.
/// </summary>
internal class SimpleStandardSubModelViewController : StandardSubModelViewController<SimpleSubDataObject, IStandardSubCRUDDataLayer<SimpleSubDataObject>>
{
    /// <inheritdoc/>
    public SimpleStandardSubModelViewController(IStandardSubCRUDDataLayer<SimpleSubDataObject> dataLayer, ILogger logger) : base(dataLayer, logger) { }
}
