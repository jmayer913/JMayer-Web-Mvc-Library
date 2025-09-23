namespace JMayer.Web.Mvc.Controller;

/// <summary>
/// The enumeration for what the controller should do when the model fails server-side validation.
/// </summary>
public enum ValidationFailedAction
{
    /// <summary>
    /// The controller will return a view with the model and its state.
    /// </summary>
    ReturnView = 0,

    /// <summary>
    /// The controller will return a partial view with the model and its state.
    /// </summary>
    ReturnPartialView,

    /// <summary>
    /// The controller will return json of the model state.
    /// </summary>
    ReturnJson
}
