using JMayer.Data.Data;
using JMayer.Data.Database.DataLayer;
using JMayer.Data.HTTP.Details;
using JMayer.Web.Mvc.Extension;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;

namespace JMayer.Web.Mvc.Controller.Mvc;

/// <summary>
/// The class manages HTTP action requests associated with a data object and a data layer.
/// <br/>
/// <br/>
/// The IsCUDActionRedirectedOnSuccess and ValidationFailedAction properties dictate if the controller uses the MVC
/// pattern (redirects or returning views with the model state) or the Ajax pattern (returning json to be processed 
/// by javascript). The default functionality is the MVC pattern and you can switch to the Ajax pattern in the constructor 
/// of your child class.
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
/// <typeparam name="T">Must be a DataObject since the data layer requires this.</typeparam>
/// <typeparam name="U">Must be an IStandardCRUDDataLayer so the controller can interact with the collection/table associated with it.</typeparam>
public class StandardModelViewController<T, U> : Microsoft.AspNetCore.Mvc.Controller
    where T : DataObject
    where U : IStandardCRUDDataLayer<T>
{
    /// <summary>
    /// The property gets data layer the controller will interact with.
    /// </summary>
    protected IStandardCRUDDataLayer<T> DataLayer { get; private init; }

    /// <summary>
    /// The property gets name of the data object.
    /// </summary>
    protected string DataObjectTypeName { get; private init; } = typeof(T).Name;

    /// <summary>
    /// The property gets/sets if the controller redirects a CUD (Create, Delete or Update) action on success to the Index view.
    /// </summary>
    /// <remarks>
    /// The default functionality is to do a redirect but if you need to return json then in the constructor of your child class, 
    /// set this property to false.
    /// </remarks>
    public bool IsCUDActionRedirectedOnSuccess { get; init; } = true;

    /// <summary>
    /// The property gets/sets if the controller includes details on a negative response.
    /// </summary>
    /// <remarks>
    /// The default functionality is to return no details (the MVC pattern requires this with UseStatusCodePagesWithRedirects()) but if 
    /// you need to return details then in the constructor of your child class, set this property to true.
    /// </remarks>
    public bool IsDetailsIncludedInNegativeResponse { get; init; }

    /// <summary>
    /// The property gets logger the controller will interact with.
    /// </summary>
    protected ILogger Logger { get; private init; }

    /// <summary>
    /// The property gets/sets what the controller will do when the model fails server-side validation.
    /// </summary>
    /// <remarks>
    /// The default functionality is to return the view associated with the action but if you need a different action (partial view or
    /// 400 bad request with a model state dictionary) then in the constructor of your child class, set this property the action you need.
    /// </remarks>
    public ValidationFailedAction ValidationFailedAction { get; init; }

    /// <summary>
    /// The dependency injection constructor.
    /// </summary>
    /// <param name="dataLayer">The data layer the controller will interact with.</param>
    /// <param name="logger">The logger the controller will interact with.</param>
    /// <exception cref="ArgumentNullException">Thrown if the dataLayer or logger parameter is null.</exception>
    public StandardModelViewController(U dataLayer, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(dataLayer);
        ArgumentNullException.ThrowIfNull(logger);

        DataLayer = dataLayer;
        Logger = logger;
    }

    /// <summary>
    /// The method returns the add partial view.
    /// </summary>
    /// <returns>The add partial view or a negative status code.</returns>
    public virtual async Task<IActionResult> AddPartialViewAsync()
    {
        try
        {
            PartialViewResult partialViewResult = PartialView($"_{DataObjectTypeName}AddPartial");
            return await Task.FromResult(partialViewResult);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Add Partial View for the {Type}.", DataObjectTypeName);
            return IsDetailsIncludedInNegativeResponse ? Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Add Partial View Error", detail: $"Failed to find the {DataObjectTypeName.SpaceCapitalLetters()} Add Partial View because of an error on the server.") : Problem();
        }
    }

    /// <summary>
    /// The method returns the add view.
    /// </summary>
    /// <returns>The add view or a negative status code.</returns>
    public virtual async Task<IActionResult> AddViewAsync()
    {
        try
        {
            ViewResult viewResult = View($"{DataObjectTypeName}Add");
            return await Task.FromResult(viewResult);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Add View for the {Type}.", DataObjectTypeName);
            return IsDetailsIncludedInNegativeResponse ? Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Add View Error", detail: $"Failed to find the {DataObjectTypeName.SpaceCapitalLetters()} Add View because of an error on the server.") : Problem();
        }
    }

    /// <summary>
    /// The method creates a data object using the data layer.
    /// </summary>
    /// <param name="dataObject">The data object.</param>
    /// <returns>The created data object or a negative status code.</returns>
    [HttpPost]
    public virtual async Task<IActionResult> CreateAsync(T dataObject)
    {
        try
        {
            if (ModelState.IsValid is false)
            {
                Logger.LogWarning("Failed to create the {Type} because of a model validation error.", DataObjectTypeName);
                return ValidationFailedAction switch
                {
                    ValidationFailedAction.ReturnView => View($"{DataObjectTypeName}Add", dataObject),
                    ValidationFailedAction.ReturnPartialView => new PartialViewResult()
                    {
                        ViewData = new ViewDataDictionary<T>(ViewData, dataObject),
                        ViewName = $"_{DataObjectTypeName}AddPartial",
                    },
                    _ => ValidationProblem(ModelState)
                };
            }

            await DataLayer.CreateAsync(dataObject);
            Logger.LogInformation("The {Type} was successfully created.", DataObjectTypeName);

            if (IsCUDActionRedirectedOnSuccess)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return Json(dataObject);
            }
        }
        catch (DataObjectValidationException ex)
        {
            Logger.LogWarning(ex, "Failed to create the {Type} because of a server-side validation error.", DataObjectTypeName);
            ex.CopyToModelState(ModelState);
            return ValidationFailedAction switch
            {
                ValidationFailedAction.ReturnView => View($"{DataObjectTypeName}Add", dataObject),
                ValidationFailedAction.ReturnPartialView => new PartialViewResult()
                {
                    ViewData = new ViewDataDictionary<T>(ViewData, dataObject),
                    ViewName = $"_{DataObjectTypeName}AddPartial",
                },
                _ => ValidationProblem(ModelState)
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create the {Type}.", DataObjectTypeName);
            return IsDetailsIncludedInNegativeResponse ? Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Create Error", detail: $"Failed to create the {DataObjectTypeName.SpaceCapitalLetters()} record because of an error on the server.") : Problem();
        }
    }

    /// <summary>
    /// The method deletes a data object using the data layer.
    /// </summary>
    /// <param name="id">The id to delete.</param>
    /// <returns>The deleted data object or a negative status code.</returns>
    [HttpPost("[controller]/Delete/{id:long}")]
    public virtual async Task<IActionResult> DeleteAsync(long id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.Integer64ID == id);

            if (dataObject is null)
            {
                Logger.LogWarning("The {ID} for the {Type} was not found so no delete occurred.", id.ToString(), DataObjectTypeName);
                return IsDetailsIncludedInNegativeResponse ? NotFound(new NotFoundDetails(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Delete Error - Not Found", detail: $"The {DataObjectTypeName.SpaceCapitalLetters()} record was not found; please refresh the page because another user may have deleted it.")) : NotFound();
            }

            await DataLayer.DeleteAsync(dataObject);
            Logger.LogInformation("The {ID} for the {Type} was successfully deleted.", id.ToString(), DataObjectTypeName);

            if (IsCUDActionRedirectedOnSuccess)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return Json(dataObject);
            }
        }
        catch (DataObjectDeleteConflictException ex)
        {
            Logger.LogError(ex, "Failed to delete the {ID} {Type} because of a data conflict.", id.ToString(), DataObjectTypeName);
            return IsDetailsIncludedInNegativeResponse ? Conflict(new ConflictDetails(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Delete Error - Data Conflict", detail: $"The {DataObjectTypeName.SpaceCapitalLetters()} record has a dependency that prevents it from being deleted; the dependency needs to be deleted first.")) : Conflict();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete the {ID} {Type}.", id.ToString(), DataObjectTypeName);
            return IsDetailsIncludedInNegativeResponse ? Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Delete Error", detail: $"Failed to delete the {DataObjectTypeName.SpaceCapitalLetters()} record because of an error on the server.") : Problem();
        }
    }

    /// <summary>
    /// The method deletes a data object using the data layer.
    /// </summary>
    /// <param name="id">The id to delete.</param>
    /// <returns>The deleted data object or a negative status code.</returns>
    [HttpPost("[controller]/Delete/{id}")]
    public virtual async Task<IActionResult> DeleteAsync(string id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.StringID == id);

            if (dataObject is null)
            {
                Logger.LogWarning("The {ID} for the {Type} was not found so no delete occurred.", id, DataObjectTypeName);
                return IsDetailsIncludedInNegativeResponse ? NotFound(new NotFoundDetails(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Delete Error - Not Found", detail: $"The {DataObjectTypeName.SpaceCapitalLetters()} record was not found; please refresh the page because another user may have deleted it.")) : NotFound();
            }

            await DataLayer.DeleteAsync(dataObject);
            Logger.LogInformation("The {ID} for the {Type} was successfully deleted.", id, DataObjectTypeName);

            if (IsCUDActionRedirectedOnSuccess)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return Json(dataObject);
            }
        }
        catch (DataObjectDeleteConflictException ex)
        {
            Logger.LogError(ex, "Failed to delete the {ID} {Type} because of a data conflict.", id.ToString(), DataObjectTypeName);
            return IsDetailsIncludedInNegativeResponse ? Conflict(new ConflictDetails(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Delete Error - Data Conflict", detail: $"The {DataObjectTypeName.SpaceCapitalLetters()} record has a dependency that prevents it from being deleted; the dependency needs to be deleted first.")) : Conflict();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete the {ID} {Type}.", id.ToString(), DataObjectTypeName);
            return IsDetailsIncludedInNegativeResponse ? Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Delete Error", detail: $"Failed to delete the {DataObjectTypeName.SpaceCapitalLetters()} record because of an error on the server.") : Problem();
        }
    }

    /// <summary>
    /// The method returns the delete partial view.
    /// </summary>
    /// <param name="id">The id for the record.</param>
    /// <returns>The delete partial view or a negative status code.</returns>
    [HttpGet("[controller]/DeletePartialView/{id:long}")]
    public virtual async Task<IActionResult> DeletePartialViewAsync(long id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.Integer64ID == id);

            if (dataObject is null)
            {
                Logger.LogError("Failed to find the {ID} when fetching the Delete Partial View for the {Type}.", id, DataObjectTypeName);
                return IsDetailsIncludedInNegativeResponse ? NotFound(new NotFoundDetails(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Delete Partial View Error - Not Found", detail: $"The {DataObjectTypeName.SpaceCapitalLetters()} record was not found; please refresh the page because another user may have deleted it.")) : NotFound();
            }

            return new PartialViewResult()
            {
                ViewData = new ViewDataDictionary<T>(ViewData, dataObject),
                ViewName = $"_{DataObjectTypeName}DeletePartial",
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Delete Partial View for the {Type}.", DataObjectTypeName);
            return IsDetailsIncludedInNegativeResponse ? Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Delete Partial View Error", detail: $"Failed to find the {DataObjectTypeName.SpaceCapitalLetters()} Delete View because of an error on the server.") : Problem();
        }
    }

    /// <summary>
    /// The method returns the delete partial view.
    /// </summary>
    /// <param name="id">The id for the record.</param>
    /// <returns>The delete partial view or a negative status code.</returns>
    [HttpGet("[controller]/DeletePartialView/{id}")]
    public virtual async Task<IActionResult> DeletePartialViewAsync(string id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.StringID == id);

            if (dataObject is null)
            {
                Logger.LogError("Failed to find the {ID} when fetching the Delete Partial View for the {Type}.", id, DataObjectTypeName);
                return IsDetailsIncludedInNegativeResponse ? NotFound(new NotFoundDetails(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Delete Partial View Error - Not Found", detail: $"The {DataObjectTypeName.SpaceCapitalLetters()} record was not found; please refresh the page because another user may have deleted it.")) : NotFound();
            }

            return new PartialViewResult()
            {
                ViewData = new ViewDataDictionary<T>(ViewData, dataObject),
                ViewName = $"_{DataObjectTypeName}DeletePartial",
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Delete Partial View for the {Type}.", DataObjectTypeName);
            return IsDetailsIncludedInNegativeResponse ? Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Delete Partial View Error", detail: $"Failed to find the {DataObjectTypeName.SpaceCapitalLetters()} Delete View because of an error on the server.") : Problem();
        }
    }

    /// <summary>
    /// The method returns the delete view.
    /// </summary>
    /// <param name="id">The id for the record.</param>
    /// <returns>The delete view or a negative status code.</returns>
    [HttpGet("[controller]/DeleteView/{id:long}")]
    public virtual async Task<IActionResult> DeleteViewAsync(long id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.Integer64ID == id);

            if (dataObject is null)
            {
                Logger.LogError("Failed to find the {ID} when fetching the Delete View for the {Type}.", id, DataObjectTypeName);
                return IsDetailsIncludedInNegativeResponse ? NotFound(new NotFoundDetails(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Delete View Error - Not Found", detail: $"The {DataObjectTypeName.SpaceCapitalLetters()} record was not found; please refresh the page because another user may have deleted it.")) : NotFound();
            }

            return View($"{DataObjectTypeName}Delete", dataObject);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Delete View for the {Type} using the {ID} ID.", DataObjectTypeName, id);
            return IsDetailsIncludedInNegativeResponse ? Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Delete View Error", detail: $"Failed to find the {DataObjectTypeName.SpaceCapitalLetters()} Delete View because of an error on the server.") : Problem();
        }
    }

    /// <summary>
    /// The method returns the delete view.
    /// </summary>
    /// <param name="id">The id for the record.</param>
    /// <returns>The delete view or a negative status code.</returns>
    [HttpGet("[controller]/DeleteView/{id}")]
    public virtual async Task<IActionResult> DeleteViewAsync(string id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.StringID == id);

            if (dataObject == null)
            {
                Logger.LogError("Failed to find the {ID} when fetching the Delete View for the {Type}.", id, DataObjectTypeName);
                return IsDetailsIncludedInNegativeResponse ? NotFound(new NotFoundDetails(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Delete View Error - Not Found", detail: $"The {DataObjectTypeName.SpaceCapitalLetters()} record was not found; please refresh the page because another user may have deleted it.")) : NotFound();
            }

            return View($"{DataObjectTypeName}Delete", dataObject);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Delete View for the {Type} using the {ID} ID.", DataObjectTypeName, id);
            return IsDetailsIncludedInNegativeResponse ? Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Delete View Error", detail: $"Failed to find the {DataObjectTypeName.SpaceCapitalLetters()} Delete View because of an error on the server.") : Problem();
        }
    }

    /// <summary>
    /// The method returns the edit partial view.
    /// </summary>
    /// <param name="id">The id for the record.</param>
    /// <returns>The edit partial view or a negative status code.</returns>
    [HttpGet("[controller]/EditPartialView/{id:long}")]
    public virtual async Task<IActionResult> EditPartialViewAsync(long id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.Integer64ID == id);

            if (dataObject is null)
            {
                Logger.LogError("Failed to find the {ID} when fetching the Edit Partial View for the {Type}.", id, DataObjectTypeName);
                return IsDetailsIncludedInNegativeResponse ? NotFound(new NotFoundDetails(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Edit Partial View Error - Not Found", detail: $"The {DataObjectTypeName.SpaceCapitalLetters()} record was not found; please refresh the page because another user may have deleted it.")) : NotFound();
            }

            return new PartialViewResult()
            {
                ViewData = new ViewDataDictionary<T>(ViewData, dataObject),
                ViewName = $"_{DataObjectTypeName}EditPartial",
            };
        }
        catch (Exception ex) 
        {
            Logger.LogError(ex, "Failed to return the Edit Partial View for the {Type}.", DataObjectTypeName);
            return IsDetailsIncludedInNegativeResponse ? Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Edit Partial View Error", detail: $"Failed to find the {DataObjectTypeName.SpaceCapitalLetters()} Edit View because of an error on the server.") : Problem();
        }
    }

    /// <summary>
    /// The method returns the edit partial view.
    /// </summary>
    /// <param name="id">The id for the record.</param>
    /// <returns>The edit partial view or a negative status code.</returns>
    [HttpGet("[controller]/EditPartialView/{id}")]
    public virtual async Task<IActionResult> EditPartialViewAsync(string id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.StringID == id);

            if (dataObject is null)
            {
                Logger.LogError("Failed to find the {ID} when fetching the Edit Partial View for the {Type}.", id, DataObjectTypeName);
                return IsDetailsIncludedInNegativeResponse ? NotFound(new NotFoundDetails(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Edit Partial View Error - Not Found", detail: $"The {DataObjectTypeName.SpaceCapitalLetters()} record was not found; please refresh the page because another user may have deleted it.")) : NotFound();
            }

            return new PartialViewResult()
            {
                ViewData = new ViewDataDictionary<T>(ViewData, dataObject),
                ViewName = $"_{DataObjectTypeName}EditPartial",
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Edit Partial View for the {Type}.", DataObjectTypeName);
            return IsDetailsIncludedInNegativeResponse ? Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Edit Partial View Error", detail: $"Failed to find the {DataObjectTypeName.SpaceCapitalLetters()} Edit View because of an error on the server.") : Problem();
        }
    }

    /// <summary>
    /// The method returns the edit view.
    /// </summary>
    /// <param name="id">The id for the record.</param>
    /// <returns>The edit view or a negative status code.</returns>
    [HttpGet("[controller]/EditView/{id:long}")]
    public virtual async Task<IActionResult> EditViewAsync(long id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.Integer64ID == id);

            if (dataObject is null)
            {
                Logger.LogError("Failed to find the {ID} when fetching the Edit View for the {Type}.", id, DataObjectTypeName);
                return IsDetailsIncludedInNegativeResponse ? NotFound(new NotFoundDetails(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Edit View Error - Not Found", detail: $"The {DataObjectTypeName.SpaceCapitalLetters()} record was not found; please refresh the page because another user may have deleted it.")) : NotFound();
            }

            return View($"{DataObjectTypeName}Edit", dataObject);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Edit View for the {Type} using the {ID} ID.", DataObjectTypeName, id);
            return IsDetailsIncludedInNegativeResponse ? Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Edit View Error", detail: $"Failed to find the {DataObjectTypeName.SpaceCapitalLetters()} Edit View because of an error on the server.") : Problem();
        }
    }

    /// <summary>
    /// The method returns the edit view.
    /// </summary>
    /// <param name="id">The id for the record.</param>
    /// <returns>The edit view or a negative status code.</returns>
    [HttpGet("[controller]/EditView/{id}")]
    public virtual async Task<IActionResult> EditViewAsync(string id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.StringID == id);

            if (dataObject is null)
            {
                Logger.LogError("Failed to find the {ID} when fetching the Edit View for the {Type}.", id, DataObjectTypeName);
                return IsDetailsIncludedInNegativeResponse ? NotFound(new NotFoundDetails(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Edit View Error - Not Found", detail: $"The {DataObjectTypeName.SpaceCapitalLetters()} record was not found; please refresh the page because another user may have deleted it.")) : NotFound();
            }

            return View($"{DataObjectTypeName}Edit", dataObject);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Edit View for the {Type} using the {ID} ID.", DataObjectTypeName, id);
            return IsDetailsIncludedInNegativeResponse ? Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Edit View Error", detail: $"Failed to find the {DataObjectTypeName.SpaceCapitalLetters()} Edit View because of an error on the server.") : Problem();
        }
    }

    /// <summary>
    /// The method returns the index view.
    /// </summary>
    /// <returns>The index view or a negative status code.</returns>
    public virtual async Task<IActionResult> IndexAsync()
    {
        try
        {
            List<T>? dataObjects = await DataLayer.GetAllAsync();
            return View($"{DataObjectTypeName}{nameof(Index)}", dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Index View for the {Type}.", DataObjectTypeName);
            return IsDetailsIncludedInNegativeResponse ? Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Index View Error", detail: $"Failed to find the {DataObjectTypeName.SpaceCapitalLetters()} Index View because of an error on the server.") : Problem();
        }
    }

    /// <summary>
    /// The method updates a data object using the data layer.
    /// </summary>
    /// <param name="dataObject">The data object.</param>
    /// <returns>The updated data object or a negative status code.</returns>
    [HttpPost]
    public virtual async Task<IActionResult> UpdateAsync(T dataObject)
    {
        string id = string.Empty;

        try
        {
            id = string.IsNullOrEmpty(dataObject.StringID) ? dataObject.Integer64ID.ToString() : dataObject.StringID;

            if (ModelState.IsValid is false)
            {
                Logger.LogWarning("Failed to update the {ID} {Type} because of a model validation error.", id, DataObjectTypeName);
                return ValidationFailedAction switch
                {
                    ValidationFailedAction.ReturnView => View($"{DataObjectTypeName}Edit", dataObject),
                    ValidationFailedAction.ReturnPartialView => new PartialViewResult()
                    {
                        ViewData = new ViewDataDictionary<T>(ViewData, dataObject),
                        ViewName = $"_{DataObjectTypeName}EditPartial",
                    },
                    _ => ValidationProblem(ModelState)
                };
            }

            dataObject = await DataLayer.UpdateAsync(dataObject);
            Logger.LogInformation("The {ID} for the {Type} was successfully updated.", id, DataObjectTypeName);

            if (IsCUDActionRedirectedOnSuccess)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return Json(dataObject);
            }
        }
        catch (DataObjectUpdateConflictException ex)
        {
            Logger.LogWarning(ex, "Failed to update {ID} {Type} because the data was considered old.", id, DataObjectTypeName);
            return IsDetailsIncludedInNegativeResponse ? Conflict(new ConflictDetails(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Update Error - Data Conflict", detail: $"The submitted {DataObjectTypeName.SpaceCapitalLetters()} data was detected to be out of date; please refresh the page and try again.")) : Conflict();
        }
        catch (DataObjectValidationException ex)
        {
            Logger.LogWarning(ex, "Failed to update the {ID} {Type} because of a server-side validation error.", id, DataObjectTypeName);
            ex.CopyToModelState(ModelState);
            return ValidationFailedAction switch
            {
                ValidationFailedAction.ReturnView => View($"{DataObjectTypeName}Edit", dataObject),
                ValidationFailedAction.ReturnPartialView => new PartialViewResult()
                {
                    ViewData = new ViewDataDictionary<T>(ViewData, dataObject),
                    ViewName = $"_{DataObjectTypeName}EditPartial",
                },
                _ => ValidationProblem(ModelState)
            };
        }
        catch (IDNotFoundException ex)
        {
            Logger.LogWarning(ex, "Failed to update the {ID} {Type} because it was not found.", id, DataObjectTypeName);
            return IsDetailsIncludedInNegativeResponse ? NotFound(new NotFoundDetails(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Update Error - Not Found", detail: $"The {DataObjectTypeName.SpaceCapitalLetters()} record was not found; please refresh the page because another user may have deleted it.")) : NotFound();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to update the {Type} for {ID}.", DataObjectTypeName, id);
            return IsDetailsIncludedInNegativeResponse ? Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Update Error", detail: $"Failed to update the {DataObjectTypeName.SpaceCapitalLetters()} record because of an error on the server.") : Problem();
        }
    }
}
