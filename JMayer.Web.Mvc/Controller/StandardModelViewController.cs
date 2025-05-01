using JMayer.Data.Data;
using JMayer.Data.Database.DataLayer;
using JMayer.Data.HTTP.DataLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;

namespace JMayer.Web.Mvc.Controller;

#warning I need to figure out if a User Editable and Sub User Editable needs to exist.
#warning I need to figure out a standard for CUD actions.

#warning I had to create a Syncfusion controller in the example project but at the very least, I believe I can have a standard for fetching views (index, add & edit; not sure if delete is needed).

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="U"></typeparam>
public class StandardModelViewController<T, U> : Microsoft.AspNetCore.Mvc.Controller
    where T : DataObject
    where U : Data.Database.DataLayer.IStandardCRUDDataLayer<T>
{
    /// <summary>
    /// The data layer the controller will interact with.
    /// </summary>
    protected readonly Data.Database.DataLayer.IStandardCRUDDataLayer<T> DataLayer;

    /// <summary>
    /// The logger the controller will interact with.
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// The name of the data object.
    /// </summary>
    protected readonly string DataObjectTypeName = typeof(T).Name;

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

    //[HttpPost]
    ////[ValidateAntiForgeryToken]
    //public virtual async Task<IActionResult> CreateAsync(T dataObject)
    //{
    //    try
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            await DataLayer.CreateAsync(dataObject);
    //        }

    //        return Json(dataObject);
    //    }
    //    catch (DataObjectValidationException ex)
    //    {
    //        ServerSideValidationResult serverSideValidationResult = new(ex.ValidationResults);
    //        Logger.LogWarning(ex, "Failed to create the {Type} because of a server-side validation error.", DataObjectTypeName);
    //        return BadRequest(serverSideValidationResult);
    //    }
    //    catch (Exception ex)
    //    {
    //        Logger.LogError(ex, "Failed to create the {Type}.", DataObjectTypeName);
    //        return Problem();
    //    }
    //}

    //[HttpPost]
    ////[HttpDelete]
    ////[ValidateAntiForgeryToken]
    //public virtual async Task<IActionResult> DeleteAsync(int key)
    //{
    //    try
    //    {
    //        T? dataObject = await DataLayer.GetSingleAsync(obj => obj.Integer64ID == key);

    //        if (dataObject != null)
    //        {
    //            await DataLayer.DeleteAsync(dataObject);
    //        }

    //        return Ok();
    //    }
    //    catch (DataObjectDeleteConflictException ex)
    //    {
    //        Logger.LogError(ex, "Failed to delete the {ID} {Type} because of a data conflict.", key, DataObjectTypeName);
    //        return Conflict();
    //    }
    //    catch (Exception ex)
    //    {
    //        Logger.LogError(ex, "Failed to delete the {ID} {Type}.", key, DataObjectTypeName);
    //        return Problem();
    //    }
    //}

    //[HttpPut]
    ////[ValidateAntiForgeryToken]
    //public virtual async Task<IActionResult> UpdateAsync(long? integerID, T dataObject)
    //{
    //    try
    //    {
    //        if (integerID != dataObject.Integer64ID)
    //        {
    //            return NotFound();
    //        }

    //        if (ModelState.IsValid)
    //        {
    //            await DataLayer.UpdateAsync(dataObject);
    //            return RedirectToAction($"{DataObjectTypeName}Index");
    //        }

    //        return View(dataObject);
    //    }
    //    catch (DataObjectUpdateConflictException ex)
    //    {
    //        Logger.LogWarning(ex, "Failed to update {Type} because the data was considered old.", DataObjectTypeName);
    //        return Conflict();
    //    }
    //    catch (DataObjectValidationException ex)
    //    {
    //        ServerSideValidationResult serverSideValidationResult = new(ex.ValidationResults);
    //        Logger.LogWarning(ex, "Failed to update the {Type} because of a server-side validation error.", DataObjectTypeName);
    //        return BadRequest(serverSideValidationResult);
    //    }
    //    catch (Exception ex)
    //    {
    //        Logger.LogError(ex, "Failed to update the {Type}.", DataObjectTypeName);
    //        return Problem();
    //    }
    //}

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

            if (dataObject == null)
            {
                Logger.LogError("Failed to find the {ID} when fetching the Edit Partial View for the {Type}.", id, DataObjectTypeName);
                return NotFound();
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

            if (dataObject == null)
            {
                Logger.LogError("Failed to find the {ID} when fetching the Edit Partial View for the {Type}.", id, DataObjectTypeName);
                return NotFound();
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

            if (dataObject == null)
            {
                Logger.LogError("Failed to find the {ID} when fetching the Edit View for the {Type}.", id, DataObjectTypeName);
                return NotFound();
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
    /// <param name="stringID">The id for the record.</param>
    /// <returns>The edit view or a negative status code.</returns>
    [HttpGet("[controller]/GetEditView/{id}")]
    public virtual async Task<IActionResult> GetEditViewAsync(string stringID)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.StringID == stringID);

            if (dataObject == null)
            {
                Logger.LogError("Failed to find the {ID} when fetching the Edit View for the {Type}.", stringID, DataObjectTypeName);
                return NotFound();
            }

            return View($"{DataObjectTypeName}Edit", dataObject);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Edit View for the {Type} using the {ID} ID.", DataObjectTypeName, stringID);
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
}
