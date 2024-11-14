using JMayer.Data.Data;
using JMayer.Data.Data.Query;
using JMayer.Data.Database.DataLayer;
using JMayer.Data.HTTP.DataLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace JMayer.Web.Mvc.Controllers;

/// <summary>
/// The class manages HTTP requests for CRUD operations associated with a data object and a data layer.
/// </summary>
/// <typeparam name="T">Must be a DataObject since the data layer requires this.</typeparam>
/// <typeparam name="U">Must be an IStandardCRUDDataLayer so the controller can interact with the collection/table associated with it.</typeparam>
[ApiController]
[Route("api/[controller]")]
public class StandardCRUDController<T, U> : ControllerBase 
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
    public StandardCRUDController(Data.Database.DataLayer.IStandardCRUDDataLayer<T> dataLayer, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(dataLayer);
        ArgumentNullException.ThrowIfNull(logger);

        DataLayer = dataLayer;
        Logger = logger;
    }

    /// <summary>
    /// The method returns the count using the data layer.
    /// </summary>
    /// <returns>The count.</returns>
    [HttpGet("Count")]
    public virtual async Task<IActionResult> CountAsync()
    {
        try
        {
            long count = await DataLayer.CountAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the count for the {Type} data objects.", DataObjectTypeName);
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// The method creates a data object using the data layer.
    /// </summary>
    /// <param name="dataObject">The data object.</param>
    /// <returns>The created data object.</returns>
    [HttpPost]
    public virtual async Task<IActionResult> CreateAsync([FromBody] T dataObject)
    {
        try
        {
            dataObject = await DataLayer.CreateAsync(dataObject);
            Logger.LogInformation("The {Type} was successfully created.", DataObjectTypeName);
            return Ok(dataObject);
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

    /// <summary>
    /// The method deletes a data object using the data layer.
    /// </summary>
    /// <param name="integerID">The id for the data object.</param>
    /// <returns>An IActionResult object.</returns>
    [HttpDelete("{integerID:long}")]
    public virtual async Task<IActionResult> DeleteAsync(long integerID)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.Integer64ID == integerID);

            if (dataObject != null)
            {
                await DataLayer.DeleteAsync(dataObject);
                Logger.LogInformation("The {ID} for the {Type} was successfully deleted.", integerID, DataObjectTypeName);
            }
            
            return Ok();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete the {ID} {Type}.", integerID, DataObjectTypeName);
            return Problem();
        }
    }

    /// <summary>
    /// The method deletes a data object using the data layer.
    /// </summary>
    /// <param name="stringID">The id for the data object.</param>
    /// <returns>An IActionResult object.</returns>
    [HttpDelete("{stringID}")]
    public virtual async Task<IActionResult> DeleteAsync(string stringID)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.StringID == stringID);

            if (dataObject != null)
            {
                await DataLayer.DeleteAsync(dataObject);
                Logger.LogInformation("The {ID} for the {Type} was successfully deleted.", stringID, DataObjectTypeName);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete the {ID} {Type}.", stringID, DataObjectTypeName);
            return Problem();
        }
    }

    /// <summary>
    /// The method returns all the data objects using the data layer.
    /// </summary>
    /// <returns>A list of data objects.</returns>
    [HttpGet("All")]
    public virtual async Task<IActionResult> GetAllAsync()
    {
        try
        {
            List<T> dataObjects = await DataLayer.GetAllAsync();
            return Ok(dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return all the {Type} data objects.", DataObjectTypeName);
            return Problem();
        }
    }

    /// <summary>
    /// The method returns a page of data objects using the data layer.
    /// </summary>
    /// <param name="queryDefinition">Defines how the data should be queried; includes filtering, paging and sorting.</param>
    /// <returns>A list of data objects.</returns>
    [HttpGet("Page")]
    public virtual async Task<IActionResult> GetPageAsync([FromQuery] QueryDefinition queryDefinition)
    {
        try
        {
            PagedList<T> dataObjects = await DataLayer.GetPageAsync(queryDefinition);
            return Ok(dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return a page of {Type} data objects.", DataObjectTypeName);
            return Problem();
        }
    }

    /// <summary>
    /// The method returns the first data object using the data layer.
    /// </summary>
    /// <returns>A data object.</returns>
    [HttpGet("Single")]
    public virtual async Task<IActionResult> GetSingleAsync()
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync();
            return Ok(dataObject);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the first {Type} data object.", DataObjectTypeName);
            return Problem();
        }
    }

    /// <summary>
    /// The method returns a data object based on the ID using the data layer.
    /// </summary>
    /// <param name="integerID">The id to search for.</param>
    /// <returns>A data object.</returns>
    [HttpGet("Single/{integerID:long}")]
    public virtual async Task<IActionResult> GetSingleAsync(long integerID)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.Integer64ID == integerID);
            return Ok(dataObject);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the {ID} {Type} data object.", integerID, DataObjectTypeName);
            return Problem();
        }
    }

    /// <summary>
    /// The method returns a data object based on the ID using the data layer.
    /// </summary>
    /// <param name="stringID">The id to search for.</param>
    /// <returns>A data object.</returns>
    [HttpGet("Single/{stringID}")]
    public virtual async Task<IActionResult> GetSingleAsync(string stringID)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.StringID == stringID);
            return Ok(dataObject);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the {ID} {Type} data object.", stringID, DataObjectTypeName);
            return Problem();
        }
    }

    /// <summary>
    /// The method updates a data object using the data layer.
    /// </summary>
    /// <param name="dataObject">The data object to update.</param>
    /// <returns>The updated data object.</returns>
    [HttpPut]
    public virtual async Task<IActionResult> UpdateAsync([FromBody] T dataObject)
    {
        try
        {
            dataObject = await DataLayer.UpdateAsync(dataObject);
            Logger.LogInformation("The {Type} was successfully updated.", DataObjectTypeName);
            return Ok(dataObject);
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

    /// <summary>
    /// The method validates a data object using the data layer.
    /// </summary>
    /// <param name="dataObject">The data object to validated.</param>
    /// <returns>The validation result.</returns>
    [HttpPost("Validate")]
    public virtual async Task<IActionResult> ValidateAsync([FromBody] T dataObject)
    {
        try
        {
            List<ValidationResult> validationResults = await DataLayer.ValidateAsync(dataObject);
            ServerSideValidationResult serverSideValidationResult = new(validationResults);
            Logger.LogInformation("The {Type} was successfully validated.", DataObjectTypeName);
            return Ok(serverSideValidationResult);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to validate the {Type}.", DataObjectTypeName);
            return Problem();
        }
    }
}
