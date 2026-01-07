using JMayer.Data.Data;
using JMayer.Data.Database.DataLayer;
using JMayer.Web.Mvc.Extension;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JMayer.Web.Mvc.Controller.Mvc;

/// <summary>
/// The class manages HTTP action requests associated with a sub user editable data object and a data layer.
/// <br/>
/// <br/>
/// The controller is desinged to return a subset of data objects based on an owner id. The IndexAsync() uses the
/// default pattern mapping to accept an owner id and return the subset. The owner id will be stored in the ViewBag
/// and you can use it in the MVC pattern when navigating to an add page. The AddViewAsync() and AddPartialViewAsync() 
/// also uses the default pattern mapping to accept an owner id. The owner id will be stored in the ViewBag and
/// when using the MVC pattern, it can be stored in a hidden input in the form so the owner id is added to the
/// data object when posted to the Create action. When using the MVC pattern, the CUD actions, on success, will
/// redirect back to the Index page and the id will be set to the owner id.
/// <br/>
/// <br/>
/// The IsCUDActionRedirectedOnSuccess and ValidationFailedAction properties dictate if the controller uses the MVC
/// pattern (redirects or returning views with the model state) or the Ajax pattern (returning json to be processed 
/// by javascript). The default functionality is the MVC pattern and you can switch what you need to the Ajax pattern 
/// in the constructor of your child class.
/// <br/>
/// <br/>
/// When a model is not found, a 404 not found will be returned. With the Ajax pattern, javascript needs to be able to 
/// handle this type of response. You can set the IsDetailsIncludedInNegativeResponse property to true and a problem details
/// will be returned; the title and detail fields will be set. With the MVC pattern, you need to setup the middleware so a 
/// user friendly page is displayed. The suggested way is to register UseStatusCodePagesWithRedirects() with the middleware; 
/// the IsDetailsIncludedInNegativeResponse property must be set to false else redirects won't work.
/// <br/>
/// <br/>
/// If the data layer has old data object detection enabled or the data layer checks for dependencies before a delete 
/// (a DataObjectDeleteConflictException is thrown), a 409 conflict can be returned. With the Ajax pattern, javascript 
/// needs to be able to handle this type of response. You can set the IsDetailsIncludedInNegativeResponse property to true 
/// and a problem details will be returned; the title and detail fields will be set. With the MVC pattern, you need to setup 
/// the middleware so a user friendly page is displayed. The suggested way is to register UseStatusCodePagesWithRedirects() 
/// with the middleware; the IsDetailsIncludedInNegativeResponse property must be set to false else redirects won't work.
/// <br/>
/// <br/>
/// If an unexpected exception occurs, a 500 internal server error will be returned. With the Ajax pattern, javascript needs to 
/// be able to handle this type of response. You can set the IsDetailsIncludedInNegativeResponse property to true and a problem 
/// details will be returned; the title and detail fields will be set. With the MVC pattern, you need to setup the middleware so 
/// a user friendly page is displayed. The suggested way is to register UseStatusCodePagesWithRedirects() with the middleware; the
/// IsDetailsIncludedInNegativeResponse property must be set to false else redirects won't work.
/// </summary>
/// <typeparam name="T">Must be a SubUserEditableDataObject since the data layer requires this.</typeparam>
/// <typeparam name="U">Must be an IUserEditableDataLayer so the controller can interact with the collection/table associated with it.</typeparam>
public class StandardSubModelViewController<T, U> : StandardModelViewController<T, U>
    where T : SubDataObject
    where U : IStandardSubCRUDDataLayer<T>
{
    /// <summary>
    /// The dependency injection constructor.
    /// </summary>
    /// <param name="dataLayer">The data layer the controller will interact with.</param>
    /// <param name="logger">The logger the controller will interact with.</param>
    public StandardSubModelViewController(U dataLayer, ILogger logger) : base(dataLayer, logger) { }

    /// <inheritdoc/>
    /// <remarks>Overridden to supress the base action; the actions with the owner id should be called instead.</remarks>
    [NonAction]
    public override async Task<IActionResult> AddPartialViewAsync()
    {
        return await base.AddPartialViewAsync();
    }

    /// <inheritdoc/>
    /// <remarks>Overridden to supress the base action; the actions with the owner id should be called instead.</remarks>
    [NonAction]
    public override async Task<IActionResult> AddViewAsync()
    {
        return await base.AddViewAsync();
    }

    /// <summary>
    /// The method returns the add partial view.
    /// </summary>
    /// <param name="ownerId">The identifier of the owner; will be stored in the ViewBag.OwnerId.</param>
    /// <returns>The add partial view or a negative status code.</returns>
    [HttpGet("[controller]/AddPartialView/{ownerId:long}")]
    public virtual async Task<IActionResult> AddPartialViewAsync(long ownerId)
    {
        ViewBag.OwnerId = ownerId;
        return await base.AddPartialViewAsync();
    }

    /// <summary>
    /// The method returns the add partial view.
    /// </summary>
    /// <param name="ownerId">The identifier of the owner; will be stored in the ViewBag.OwnerId.</param>
    /// <returns>The add partial view or a negative status code.</returns>
    [HttpGet("[controller]/AddPartialView/{ownerId}")]
    public virtual async Task<IActionResult> AddPartialViewAsync(string ownerId)
    {
        ViewBag.OwnerId = ownerId;
        return await base.AddPartialViewAsync();
    }

    /// <summary>
    /// The method returns the add view.
    /// </summary>
    /// <param name="ownerId">The identifier of the owner; will be stored in the ViewBag.OwnerId.</param>
    /// <returns>The add view or a negative status code.</returns>
    [HttpGet("[controller]/AddView/{ownerId:long}")]
    public virtual async Task<IActionResult> AddViewAsync(long ownerId)
    {
        ViewBag.OwnerId = ownerId;
        return await base.AddViewAsync();
    }

    /// <summary>
    /// The method returns the add view.
    /// </summary>
    /// <param name="ownerId">The identifier of the owner; will be stored in the ViewBag.OwnerId.</param>
    /// <returns>The add view or a negative status code.</returns>
    [HttpGet("[controller]/AddView/{ownerId}")]
    public virtual async Task<IActionResult> AddViewAsync(string ownerId)
    {
        ViewBag.OwnerId = ownerId;
        return await base.AddViewAsync();
    }

    /// <inheritdoc/>
    /// <remarks>Overridden to inject the owner id into the redirect.</remarks>
    public override async Task<IActionResult> CreateAsync(T dataObject)
    {
        IActionResult actionResult = await base.CreateAsync(dataObject);

        if (actionResult is RedirectToActionResult redirectToActionResult && redirectToActionResult.ActionName is not null && redirectToActionResult.ActionName is nameof(Index))
        {
            redirectToActionResult.RouteValues ??= [];
            redirectToActionResult.RouteValues.Add("Id", dataObject.OwnerInteger64ID);
        }

        return actionResult;
    }

    /// <inheritdoc/>
    /// <remarks>Overridden to inject the owner id into the redirect.</remarks>
    [HttpPost("[controller]/Delete/{id:long}")]
    public override async Task<IActionResult> DeleteAsync(long id)
    {
        try
        {
            T? dataObject = null;

            //In order to inject the owner id into the redirect, I need the data object.
            //I don't want to copy & paste the parent's DeleteAsync() but instead override
            //and only do the injection. This means I need to query the data object
            //twice (once here and once in the parent) which sucks but it is what it is.
            if (IsCUDActionRedirectedOnSuccess)
            {
                dataObject = await DataLayer.GetSingleAsync(obj => obj.Integer64ID == id);
            }

            IActionResult actionResult = await base.DeleteAsync(id);

            if (dataObject is not null && actionResult is RedirectToActionResult redirectToActionResult && redirectToActionResult.ActionName is not null && redirectToActionResult.ActionName is nameof(Index))
            {
                redirectToActionResult.RouteValues ??= [];
                redirectToActionResult.RouteValues.Add("Id", dataObject.OwnerInteger64ID);
            }

            return actionResult;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete the {Type} data object for {ID}.", DataObjectTypeName, id);
            return IsDetailsIncludedInNegativeResponse ? Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Delete Error", detail: "Failed to delete the record because of an error on the server.") : StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <inheritdoc/>
    /// <remarks>Overridden to inject the owner id into the redirect.</remarks>
    [HttpPost("[controller]/Delete/{id}")]
    public override async Task<IActionResult> DeleteAsync(string id)
    {
        try
        {
            T? dataObject = null;

            //In order to inject the owner id into the redirect, I need the data object.
            //I don't want to copy & paste the parent's DeleteAsync() but instead override
            //and only do the injection. This means I need to query the data object
            //twice (once here and once in the parent) which sucks but it is what it is.
            if (IsCUDActionRedirectedOnSuccess)
            {
                dataObject = await DataLayer.GetSingleAsync(obj => obj.StringID == id);
            }

            IActionResult actionResult = await base.DeleteAsync(id);

            if (dataObject is not null && actionResult is RedirectToActionResult redirectToActionResult && redirectToActionResult.ActionName is not null && redirectToActionResult.ActionName is nameof(Index))
            {
                redirectToActionResult.RouteValues ??= [];
                redirectToActionResult.RouteValues.Add("Id", dataObject.OwnerInteger64ID);
            }

            return actionResult;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete the {Type} data object for {ID}.", DataObjectTypeName, id);
            return IsDetailsIncludedInNegativeResponse ? Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Delete Error", detail: "Failed to delete the record because of an error on the server.") : StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// The method returns the index view for an owner.
    /// </summary>
    /// <param name="ownerId">The owner to filter for; will be stored in the ViewBag.OwnerId.</param>
    /// <returns>The index view or a negative status code.</returns>
    [HttpGet("[controller]/Index/{ownerId:long}")]
    public virtual async Task<IActionResult> IndexAsync(long ownerId)
    {
        try
        {
            Logger.LogInformation("Attempting to retrieve the {Type} data objects for owner {ID} for the Index View.", DataObjectTypeName, ownerId);

            ViewBag.OwnerId = ownerId;
            List<T>? dataObjects = await DataLayer.GetAllAsync(obj => obj.OwnerInteger64ID == ownerId);

            Logger.LogInformation("All the {Type} data objects for owner {ID} for the Index View were successfully retrieved; returning the view.", DataObjectTypeName, ownerId);

            return View($"{DataObjectTypeName}{nameof(Index)}", dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Index View for the {Type} data objects for owner {ID}.", DataObjectTypeName, ownerId);
            return IsDetailsIncludedInNegativeResponse ? Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Index View Error", detail: "Failed to find the Index View because of an error on the server.") : StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// The method returns the index view for an owner.
    /// </summary>
    /// <param name="ownerId">The owner to filter for; will be stored in the ViewBag.OwnerId.</param>
    /// <returns>The index view or a negative status code.</returns>
    [HttpGet("[controller]/Index/{ownerId}")]
    public virtual async Task<IActionResult> IndexAsync(string ownerId)
    {
        try
        {
            Logger.LogInformation("Attempting to retrieve the {Type} data objects for owner {ID} for the Index View.", DataObjectTypeName, ownerId);

            ViewBag.OwnerId = ownerId;
            List<T>? dataObjects = await DataLayer.GetAllAsync(obj => obj.OwnerStringID == ownerId);

            Logger.LogInformation("All the {Type} data objects for owner {ID} for the Index View were successfully retrieved; returning the view.", DataObjectTypeName, ownerId);

            return View($"{DataObjectTypeName}{nameof(Index)}", dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Index View for the {Type} data objects for owner {ID}.", DataObjectTypeName, ownerId);
            return IsDetailsIncludedInNegativeResponse ? Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Index View Error", detail: "Failed to find the Index View because of an error on the server.") : StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <inheritdoc/>
    /// <remarks>Overridden to inject the owner id into the redirect.</remarks>
    public override async Task<IActionResult> UpdateAsync(T dataObject)
    {
        IActionResult actionResult = await base.UpdateAsync(dataObject);

        if (actionResult is RedirectToActionResult redirectToActionResult && redirectToActionResult.ActionName is not null && redirectToActionResult.ActionName is nameof(Index))
        {
            redirectToActionResult.RouteValues ??= [];
            redirectToActionResult.RouteValues.Add("Id", dataObject.OwnerInteger64ID);
        }

        return actionResult;
    }
}
