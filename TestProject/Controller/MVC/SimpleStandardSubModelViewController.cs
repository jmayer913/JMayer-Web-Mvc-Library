using JMayer.Data.Database.DataLayer;
using JMayer.Web.Mvc.Controller;
using Microsoft.Extensions.Logging;
using TestProject.Data;

namespace TestProject.Controller.MVC;

/// <summary>
/// The class manages HTTP view and action requests associated with a simple sub data object and simple sub memory data layer.
/// </summary>
internal class SimpleStandardSubModelViewController : StandardSubModelViewController<SimpleSubUserEditableDataObject, IUserEditableDataLayer<SimpleSubUserEditableDataObject>>
{
    /// <inheritdoc/>
    public SimpleStandardSubModelViewController(IUserEditableDataLayer<SimpleSubUserEditableDataObject> dataLayer, ILogger logger) : base(dataLayer, logger) { }
}
