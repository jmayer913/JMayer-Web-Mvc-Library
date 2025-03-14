using JMayer.Data.Data;
using JMayer.Data.Database.DataLayer;
using JMayer.Data.HTTP.DataLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JMayer.Web.Mvc.Controller;

#warning I want to change the naming so fetching a view is named differently than an action.

#warning I've defined some basic views the controller returns and now, I need to figure out if those should be the standard.
#warning I need to figure out how this will work when the data object has a long and string id.
#warning I need to figure out if a User Editable and Sub User Editable needs to exist.
#warning I need to figure out server side validation.
#warning I need to figure out how conflict is handled.

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> CreateActionAsync(long? integerID, T dataObject)
    {
        try
        {
            if (integerID != dataObject.Integer64ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await DataLayer.CreateAsync(dataObject);
                return RedirectToAction($"{DataObjectTypeName}Index");
            }

            return View(dataObject);
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
            return Problem();
        }
    }

    [HttpDelete]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> DeleteActionAsync(long integerID)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.Integer64ID == integerID);

            if (dataObject != null)
            {
                await DataLayer.DeleteAsync(dataObject);
            }

            return RedirectToAction($"{DataObjectTypeName}Index");
        }
        catch (DataObjectDeleteConflictException ex)
        {
            Logger.LogError(ex, "Failed to delete the {ID} {Type} because of a data conflict.", integerID, DataObjectTypeName);
            return Conflict();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete the {ID} {Type}.", integerID, DataObjectTypeName);
            return Problem();
        }
    }

    public virtual async Task<IActionResult> GetCreateViewAsync(long? integerID)
    {
        try
        {
#warning This might be wrong because the data object hasn't been created yet so there would be no id.

            if (integerID == null)
            {
                return NotFound();
            }

            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.Integer64ID == integerID);

            if (dataObject == null)
            {
                return NotFound();
            }

            return View($"{DataObjectTypeName}Create", dataObject);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Create View for the {Type} using the {ID} ID.", DataObjectTypeName, integerID);
            return Problem();
        }
    }

    public virtual async Task<IActionResult> GetDeleteViewAsync(long? integerID)
    {
        try
        {
            if (integerID == null)
            {
                return NotFound();
            }

            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.Integer64ID == integerID);

            if (dataObject == null)
            {
                return NotFound();
            }

            return View($"{DataObjectTypeName}Delete", dataObject);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Delete View for the {Type} using the {ID} ID.", DataObjectTypeName, integerID);
            return Problem();
        }
    }

    public virtual async Task<IActionResult> GetEditViewAsync(long? integerID)
    {
        try
        {
            if (integerID == null)
            {
                return NotFound();
            }

            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.Integer64ID == integerID);

            if (dataObject == null)
            {
                return NotFound();
            }

            return View($"{DataObjectTypeName}Edit", dataObject);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Edit View for the {Type} using the {ID} ID.", DataObjectTypeName, integerID);
            return Problem();
        }
    }

    public virtual async Task<IActionResult> IndexAsync()
    {
        try
        {
            //TO DO: This will need to handle filtering, paging & sorting.

            List<T>? dataObjects = await DataLayer.GetAllAsync();
            return View($"{DataObjectTypeName}Index", dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Index View for the {Type}.", DataObjectTypeName);
            return Problem();
        }
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> UpdateActionAsync(long? integerID, T dataObject)
    {
        try
        {
            if (integerID != dataObject.Integer64ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await DataLayer.UpdateAsync(dataObject);
                return RedirectToAction($"{DataObjectTypeName}Index");
            }

            return View(dataObject);
        }
        catch (DataObjectUpdateConflictException ex)
        {
            Logger.LogWarning(ex, "Failed to update {Type} because the data was considered old.", DataObjectTypeName);
            return Conflict();
        }
        catch (DataObjectValidationException ex)
        {
            ServerSideValidationResult serverSideValidationResult = new(ex.ValidationResults);
            Logger.LogWarning(ex, "Failed to update the {Type} because of a server-side validation error.", DataObjectTypeName);
            return BadRequest(serverSideValidationResult);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to update the {Type}.", DataObjectTypeName);
            return Problem();
        }
    }
}
