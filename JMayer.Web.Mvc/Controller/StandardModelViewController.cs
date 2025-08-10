using JMayer.Data.Data;
using JMayer.Data.Database.DataLayer;
using JMayer.Data.HTTP.DataLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;

namespace JMayer.Web.Mvc.Controller;

#warning I need to figure out if a User Editable and Sub User Editable needs to exist.

/// <summary>
/// The class manages HTTP view and action requests associated with a data object and a data layer.
/// </summary>
/// <typeparam name="T">Must be a DataObject since the data layer requires this.</typeparam>
/// <typeparam name="U">Must be an IStandardCRUDDataLayer so the controller can interact with the collection/table associated with it.</typeparam>
public class StandardModelViewController<T, U> : Microsoft.AspNetCore.Mvc.Controller
    where T : DataObject
    where U : Data.Database.DataLayer.IStandardCRUDDataLayer<T>
{
    /// <summary>
    /// The data layer the controller will interact with.
    /// </summary>
    protected readonly Data.Database.DataLayer.IStandardCRUDDataLayer<T> DataLayer;

    /// <summary>
    /// The name of the data object.
    /// </summary>
    protected readonly string DataObjectTypeName = typeof(T).Name;

    /// <summary>
    /// The logger the controller will interact with.
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// The dependency injection constructor.
    /// </summary>
    /// <param name="dataLayer">The data layer the controller will interact with.</param>
    /// <param name="logger">The logger the controller will interact with.</param>
    /// <exception cref="ArgumentNullException">Thrown if the dataLayer or logger parameter is null.</exception>
    public StandardModelViewController(Data.Database.DataLayer.IStandardCRUDDataLayer<T> dataLayer, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(dataLayer);
        ArgumentNullException.ThrowIfNull(logger);

        DataLayer = dataLayer;
        Logger = logger;
    }

    /// <summary>
    /// The method creates a data object using the data layer.
    /// </summary>
    /// <param name="dataObject">The data object.</param>
    /// <returns>The created data object or a negative status code.</returns>
    [HttpPost]
    public virtual async Task<IActionResult> CreateAsync([FromBody] T dataObject)
    {
        try
        {
            if (ModelState.IsValid)
            {
                await DataLayer.CreateAsync(dataObject);
                Logger.LogInformation("The {Type} was successfully created.", DataObjectTypeName);
                return Json(dataObject);
            }
            else
            {
                Logger.LogWarning("Failed to create the {Type} because of a model validation error.", DataObjectTypeName);
                return ValidationProblem(ModelState);
            }
        }
        catch (DataObjectValidationException ex)
        {
            ServerSideValidationResult serverSideValidationResult = new(ex.ValidationResults);
            Logger.LogWarning(ex, "Failed to create the {Type} because of a server-side validation error.", DataObjectTypeName);
            return BadRequest(serverSideValidationResult);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create the {Type}.", DataObjectTypeName);
            return Problem(detail: "Failed to create the record because of an error on the server.");
        }
    }

    /// <summary>
    /// The method deletes a data object using the data layer.
    /// </summary>
    /// <param name="id">The id to delete.</param>
    /// <returns>The deleted data object or a negative status code.</returns>
    [HttpPost("[controller]/Delete/{id:long}")]
    [HttpDelete("[controller]/{id:long}")]
    public virtual async Task<IActionResult> DeleteAsync(long id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.Integer64ID == id);

            if (dataObject is null)
            {
                Logger.LogWarning("The {ID} for the {Type} was not found.", id.ToString(), DataObjectTypeName);
                return NotFound(new { UserMessage = "The record was not found; please refresh the page because another user may have deleted it." });
            }
            else
            {
                await DataLayer.DeleteAsync(dataObject);
                Logger.LogInformation("The {ID} for the {Type} was successfully deleted.", id.ToString(), DataObjectTypeName);
                return Json(dataObject);
            }
        }
        catch (DataObjectDeleteConflictException ex)
        {
            Logger.LogError(ex, "Failed to delete the {Key} {Type} because of a data conflict.", id.ToString(), DataObjectTypeName);
            return Conflict(new { UserMessage = "The record has a dependency that prevents it from being deleted." });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete the {Key} {Type}.", id.ToString(), DataObjectTypeName);
            return Problem(detail: "Failed to delete the record because of an error on the server.");
        }
    }

    /// <summary>
    /// The method deletes a data object using the data layer.
    /// </summary>
    /// <param name="id">The id to delete.</param>
    /// <returns>The deleted data object or a negative status code.</returns>
    [HttpPost("[controller]/Delete/{id}")]
    [HttpDelete("[controller]/{id}")]
    public virtual async Task<IActionResult> DeleteAsync(string id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.StringID == id);

            if (dataObject is null)
            {
                Logger.LogWarning("The {ID} for the {Type} was not found.", id, DataObjectTypeName);
                return NotFound(new { UserMessage = "The record was not found; please refresh the page because another user may have deleted it." });
            }
            else
            {
                await DataLayer.DeleteAsync(dataObject);
                Logger.LogInformation("The {ID} for the {Type} was successfully deleted.", id, DataObjectTypeName);
                return Json(dataObject);
            }
        }
        catch (DataObjectDeleteConflictException ex)
        {
            Logger.LogError(ex, "Failed to delete the {ID} {Type} because of a data conflict.", id, DataObjectTypeName);
            return Conflict(new { UserMessage = "The record has a dependency that prevents it from being deleted." });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete the {ID} {Type}.", id, DataObjectTypeName);
            return Problem(detail: "Failed to delete the record because of an error on the server.");
        }
    }

    /// <summary>
    /// The method returns the add partial view.
    /// </summary>
    /// <returns>The add partial view or a negative status code.</returns>
    public virtual async Task<IActionResult> GetAddPartialViewAsync()
    {
        try
        {
            PartialViewResult partialViewResult = PartialView($"_{DataObjectTypeName}AddPartial");
            return await Task.FromResult(partialViewResult);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Add Partial View for the {Type}.", DataObjectTypeName);
            return Problem(detail: "Failed to find the Add Partial View.");
        }
    }

    /// <summary>
    /// The method returns the add view.
    /// </summary>
    /// <returns>The add view or a negative status code.</returns>
    public virtual async Task<IActionResult> GetAddViewAsync()
    {
        try
        {
            ViewResult viewResult = View($"{DataObjectTypeName}Add");
            return await Task.FromResult(viewResult);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Add View for the {Type}.", DataObjectTypeName);
            return Problem(detail: "Failed to find the Add View.");
        }
    }

    /// <summary>
    /// The method returns the edit partial view.
    /// </summary>
    /// <param name="id">The id for the record.</param>
    /// <returns>The edit partial view or a negative status code.</returns>
    [HttpGet("[controller]/GetEditPartialView/{id:long}")]
    public virtual async Task<IActionResult> GetEditPartialViewAsync(long id)
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
            return Problem(detail: "Failed to find the Edit View.");
        }
    }

    /// <summary>
    /// The method returns the edit partial view.
    /// </summary>
    /// <param name="id">The id for the record.</param>
    /// <returns>The edit partial view or a negative status code.</returns>
    [HttpGet("[controller]/GetEditPartialView/{id}")]
    public virtual async Task<IActionResult> GetEditPartialViewAsync(string id)
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
            return Problem(detail: "Failed to find the Edit View.");
        }
    }

    /// <summary>
    /// The method returns the edit view.
    /// </summary>
    /// <param name="id">The id for the record.</param>
    /// <returns>The edit view or a negative status code.</returns>
    [HttpGet("[controller]/GetEditView/{id:long}")]
    public virtual async Task<IActionResult> GetEditViewAsync(long id)
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
            return Problem(detail: "Failed to find the Edit View.");
        }
    }

    /// <summary>
    /// The method returns the edit view.
    /// </summary>
    /// <param name="id">The id for the record.</param>
    /// <returns>The edit view or a negative status code.</returns>
    [HttpGet("[controller]/GetEditView/{id}")]
    public virtual async Task<IActionResult> GetEditViewAsync(string id)
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
            return Problem(detail: "Failed to find the Edit View.");
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
            return Problem();
        }
    }

    /// <summary>
    /// The method updates a data object using the data layer.
    /// </summary>
    /// <param name="dataObject">The data object.</param>
    /// <returns>The updated data object or a negative status code.</returns>
    [HttpPost]
    [HttpPut]
    public virtual async Task<IActionResult> UpdateAsync([FromBody] T dataObject)
    {
        string id = string.IsNullOrEmpty(dataObject.StringID) ? dataObject.Integer64ID.ToString() : dataObject.StringID;

        try
        {
            if (ModelState.IsValid)
            {
                dataObject = await DataLayer.UpdateAsync(dataObject);
                Logger.LogInformation("The {ID} for the {Type} was successfully updated.", id, DataObjectTypeName);
                return Json(dataObject);
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
            return Conflict(new { UserMessage = "The submitted data was detected to be out of date; please refresh the page and try again." });
        }
        catch (DataObjectValidationException ex)
        {
            ServerSideValidationResult serverSideValidationResult = new(ex.ValidationResults);
            Logger.LogWarning(ex, "Failed to update the {ID} {Type} because of a server-side validation error.", id, DataObjectTypeName);
            return BadRequest(serverSideValidationResult);
        }
        catch (IDNotFoundException ex)
        {
            Logger.LogWarning(ex, "Failed to update the {ID} {Type} because it was not found.", id, DataObjectTypeName);
            return NotFound(new { UserMessage = "The record was not found; please refresh the page because another user may have deleted it." });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to update the {Type} for {ID}.", DataObjectTypeName, id);
            return Problem(detail: "Failed to update the record because of an error on the server.");
        }
    }
}
