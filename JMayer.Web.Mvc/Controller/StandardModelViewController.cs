using JMayer.Data.Data;
using JMayer.Data.Database.DataLayer;
using JMayer.Web.Mvc.Extension;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;

namespace JMayer.Web.Mvc.Controller;

#warning Figure out how the controllers should handle errors for vanilla mvc. It probably needs to redirect to a view.

/// <summary>
/// The class manages HTTP view and action requests associated with a data object and a data layer.
/// </summary>
/// <typeparam name="T">Must be a DataObject since the data layer requires this.</typeparam>
/// <typeparam name="U">Must be an IStandardCRUDDataLayer so the controller can interact with the collection/table associated with it.</typeparam>
public class StandardModelViewController<T, U> : Microsoft.AspNetCore.Mvc.Controller
    where T : DataObject
    where U : IStandardCRUDDataLayer<T>
{
    /// <summary>
    /// The data layer the controller will interact with.
    /// </summary>
    protected IStandardCRUDDataLayer<T> DataLayer { get; private init; }

    /// <summary>
    /// The name of the data object.
    /// </summary>
    protected string DataObjectTypeName { get; private init; } = typeof(T).Name;

    /// <summary>
    /// The logger the controller will interact with.
    /// </summary>
    protected ILogger Logger { get; private init; }

    /// <summary>
    /// The property gets/sets if the controller returns json for an action (Create, Delete or Update) and for any errors.
    /// </summary>
    /// <remarks>
    /// The default functionality is to do a redirect but if you need to return json because of a third party library
    /// then in the constructor of your child class, set this property to true.
    /// </remarks>
    protected bool ReturnJson { get; init; }

    /// <summary>
    /// The dependency injection constructor.
    /// </summary>
    /// <param name="dataLayer">The data layer the controller will interact with.</param>
    /// <param name="logger">The logger the controller will interact with.</param>
    /// <exception cref="ArgumentNullException">Thrown if the dataLayer or logger parameter is null.</exception>
    public StandardModelViewController(IStandardCRUDDataLayer<T> dataLayer, ILogger logger)
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

            if (ReturnJson)
            {
                return Problem(detail: "Failed to find the Add Partial View.");
            }
            else
            {
                //TO DO: Figure out what view is returned when an error occurs for retrieving a partial view.
                return View();
            }
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

            if (ReturnJson)
            {
                return Problem(detail: "Failed to find the Add View.");
            }
            else
            {
                //TO DO: Figure out what view is returned when an error occurs for retrieving a view.
                return View();
            }
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
            if (ModelState.IsValid)
            {
                await DataLayer.CreateAsync(dataObject);
                Logger.LogInformation("The {Type} was successfully created.", DataObjectTypeName);

                if (ReturnJson)
                {
                    return Json(dataObject);
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                Logger.LogWarning("Failed to create the {Type} because of a model validation error.", DataObjectTypeName);
                return ValidationProblem(ModelState);
            }
        }
        catch (DataObjectValidationException ex)
        {
            ex.CopyToModelState(ModelState);
            Logger.LogWarning(ex, "Failed to create the {Type} because of a server-side validation error.", DataObjectTypeName);
            return ValidationProblem(ModelState);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create the {Type}.", DataObjectTypeName);

            if (ReturnJson)
            {
                return Problem(detail: "Failed to create the record because of an error on the server.");
            }
            else
            {
                //TO DO: Figure out what view is returned when an error occurs creating a data object.
                return View();
            }
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
                return NotFound(new { UserMessage = "The record was not found; please refresh the page because another user may have deleted it." });
            }
            else
            {
                await DataLayer.DeleteAsync(dataObject);
                Logger.LogInformation("The {ID} for the {Type} was successfully deleted.", id.ToString(), DataObjectTypeName);

                if (ReturnJson)
                {
                    return Json(dataObject);
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }
        }
        catch (DataObjectDeleteConflictException ex)
        {
            Logger.LogError(ex, "Failed to delete the {ID} {Type} because of a data conflict.", id.ToString(), DataObjectTypeName);

            if (ReturnJson)
            {
                return Conflict(new { UserMessage = "The record has a dependency that prevents it from being deleted." });
            }
            else
            {
                //TO DO: Figure out what view is returned when a delete conflict occurs.
                return View();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete the {ID} {Type}.", id.ToString(), DataObjectTypeName);

            if (ReturnJson)
            {
                return Problem(detail: "Failed to delete the record because of an error on the server.");
            }
            else
            {
                //TO DO: Figure out what view is returned when an error occurs deleting a data object.
                return View();
            }
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
                return NotFound(new { UserMessage = "The record was not found; please refresh the page because another user may have deleted it." });
            }
            else
            {
                await DataLayer.DeleteAsync(dataObject);
                Logger.LogInformation("The {ID} for the {Type} was successfully deleted.", id, DataObjectTypeName);

                if (ReturnJson)
                {
                    return Json(dataObject);
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }
        }
        catch (DataObjectDeleteConflictException ex)
        {
            Logger.LogError(ex, "Failed to delete the {ID} {Type} because of a data conflict.", id.ToString(), DataObjectTypeName);

            if (ReturnJson)
            {
                return Conflict(new { UserMessage = "The record has a dependency that prevents it from being deleted." });
            }
            else
            {
                //TO DO: Figure out what view is returned when a delete conflict occurs.
                return View();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete the {ID} {Type}.", id.ToString(), DataObjectTypeName);

            if (ReturnJson)
            {
                return Problem(detail: "Failed to delete the record because of an error on the server.");
            }
            else
            {
                //TO DO: Figure out what view is returned when an error occurs deleting a data object.
                return View();
            }
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
                return NotFound(new { UserMessage = "The record was not found; please refresh the page because another user may have deleted it." });
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

            if (ReturnJson)
            {
                return Problem(detail: "Failed to find the Delete View.");
            }
            else
            {
                //TO DO: Figure out what view is returned when an error occurs for retrieving a partial view.
                return View();
            }
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
                return NotFound(new { UserMessage = "The record was not found; please refresh the page because another user may have deleted it." });
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

            if (ReturnJson)
            {
                return Problem(detail: "Failed to find the Delete View.");
            }
            else
            {
                //TO DO: Figure out what view is returned when an error occurs for retrieving a partial view.
                return View();
            }
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
                return NotFound(new { UserMessage = "The record was not found; please refresh the page because another user may have deleted it." });
            }

            return View($"{DataObjectTypeName}Delete", dataObject);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Delete View for the {Type} using the {ID} ID.", DataObjectTypeName, id);

            if (ReturnJson)
            {
                return Problem(detail: "Failed to find the Delete View.");
            }
            else
            {
                //TO DO: Figure out what view is returned when an error occurs for retrieving a view.
                return View();
            }
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
                return NotFound(new { UserMessage = "The record was not found; please refresh the page because another user may have deleted it." });
            }

            return View($"{DataObjectTypeName}Delete", dataObject);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Delete View for the {Type} using the {ID} ID.", DataObjectTypeName, id);

            if (ReturnJson)
            {
                return Problem(detail: "Failed to find the Delete View.");
            }
            else
            {
                //TO DO: Figure out what view is returned when an error occurs for retrieving a view.
                return View();
            }
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
                return NotFound(new { UserMessage = "The record was not found; please refresh the page because another user may have deleted it." });
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

            if (ReturnJson)
            {
                return Problem(detail: "Failed to find the Edit View.");
            }
            else
            {
                //TO DO: Figure out what view is returned when an error occurs for retrieving a partial view.
                return View();
            }
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
                return NotFound(new { UserMessage = "The record was not found; please refresh the page because another user may have deleted it." });
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

            if (ReturnJson)
            {
                return Problem(detail: "Failed to find the Edit View.");
            }
            else
            {
                //TO DO: Figure out what view is returned when an error occurs for retrieving a partial view.
                return View();
            }
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
                return NotFound(new { UserMessage = "The record was not found; please refresh the page because another user may have deleted it." });
            }

            return View($"{DataObjectTypeName}Edit", dataObject);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Edit View for the {Type} using the {ID} ID.", DataObjectTypeName, id);

            if (ReturnJson)
            {
                return Problem(detail: "Failed to find the Edit View.");
            }
            else
            {
                //TO DO: Figure out what view is returned when an error occurs for retrieving a view.
                return View();
            }
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

            if (dataObject == null)
            {
                Logger.LogError("Failed to find the {ID} when fetching the Edit View for the {Type}.", id, DataObjectTypeName);
                return NotFound(new { UserMessage = "The record was not found; please refresh the page because another user may have deleted it." });
            }

            return View($"{DataObjectTypeName}Edit", dataObject);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Edit View for the {Type} using the {ID} ID.", DataObjectTypeName, id);

            if (ReturnJson)
            {
                return Problem(detail: "Failed to find the Edit View.");
            }
            else
            {
                //TO DO: Figure out what view is returned when an error occurs for retrieving a view.
                return View();
            }
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
            return View($"{DataObjectTypeName}Index", dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Index View for the {Type}.", DataObjectTypeName);

            if (ReturnJson)
            {
                return Problem();
            }
            else
            {
                //TO DO: Figure out what view is returned when an error occurs for retrieving a view.
                return View();
            }
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
        string id = string.IsNullOrEmpty(dataObject.StringID) ? dataObject.Integer64ID.ToString() : dataObject.StringID;

        try
        {
            if (ModelState.IsValid)
            {
                dataObject = await DataLayer.UpdateAsync(dataObject);
                Logger.LogInformation("The {ID} for the {Type} was successfully updated.", id, DataObjectTypeName);

                if (ReturnJson)
                {
                    return Json(dataObject);
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                Logger.LogWarning("Failed to update the {ID} {Type} because of a model validation error.", id, DataObjectTypeName);
                return ValidationProblem(ModelState);
            }
        }
        catch (DataObjectUpdateConflictException ex)
        {
            Logger.LogWarning(ex, "Failed to update {ID} {Type} because the data was considered old.", id, DataObjectTypeName);

            if (ReturnJson)
            {
                return Conflict(new { UserMessage = "The submitted data was detected to be out of date; please refresh the page and try again." });
            }
            else
            {
                //TO DO: Figure out what view is returned when an error occurs for updating a data object.
                return View();
            }
        }
        catch (DataObjectValidationException ex)
        {
            ex.CopyToModelState(ModelState);
            Logger.LogWarning(ex, "Failed to update the {ID} {Type} because of a server-side validation error.", id, DataObjectTypeName);
            return ValidationProblem(ModelState);
        }
        catch (IDNotFoundException ex)
        {
            Logger.LogWarning(ex, "Failed to update the {ID} {Type} because it was not found.", id, DataObjectTypeName);

            if (ReturnJson)
            {
                return NotFound(new { UserMessage = "The record was not found; please refresh the page because another user may have deleted it." });
            }
            else
            {
                //TO DO: Figure out what view is returned when an error occurs for updating a data object.
                return View();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to update the {Type} for {ID}.", DataObjectTypeName, id);

            if (ReturnJson)
            {
                return Problem(detail: "Failed to update the record because of an error on the server.");
            }
            else
            {
                //TO DO: Figure out what view is returned when an error occurs for updating a data object.
                return View();
            }
        }
    }
}
