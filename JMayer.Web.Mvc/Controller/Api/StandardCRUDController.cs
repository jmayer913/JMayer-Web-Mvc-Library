using JMayer.Data.Data;
using JMayer.Data.Data.Query;
using JMayer.Data.Database.DataLayer;
using JMayer.Data.HTTP.Details;
using JMayer.Web.Mvc.Extension;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JMayer.Web.Mvc.Controller.Api;

/// <summary>
/// The class manages HTTP requests for CRUD operations associated with a data object and a data layer.
/// </summary>
/// <typeparam name="T">Must be a DataObject since the data layer requires this.</typeparam>
/// <typeparam name="U">Must be an IStandardCRUDDataLayer so the controller can interact with the collection/table associated with it.</typeparam>
[ApiController]
[Route("api/[controller]")]
public class StandardCRUDController<T, U> : ControllerBase 
    where T : DataObject
    where U : IStandardCRUDDataLayer<T>
{
    /// <summary>
    /// The property gets data layer the controller will interact with.
    /// </summary>
    protected U DataLayer { get; private init; }

    /// <summary>
    /// The property gets name of the data object.
    /// </summary>
    protected string DataObjectTypeName { get; private init; } = typeof(T).Name;

    /// <summary>
    /// The property gets logger the controller will interact with.
    /// </summary>
    protected ILogger Logger { get; private init; }

    /// <summary>
    /// The dependency injection constructor.
    /// </summary>
    /// <param name="dataLayer">The data layer the controller will interact with.</param>
    /// <param name="logger">The logger the controller will interact with.</param>
    /// <exception cref="ArgumentNullException">Thrown if the dataLayer or logger parameter is null.</exception>
    public StandardCRUDController(U dataLayer, ILogger logger)
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
            return Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Count Error", detail: $"Failed to return the {DataObjectTypeName.SpaceCapitalLetters()} count because of an error on the server.");
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
            Logger.LogWarning(ex, "Failed to create the {Type} because of a server-side validation error.", DataObjectTypeName);
            ex.CopyToModelState(ModelState);
            return base.ValidationProblem(ModelState);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create the {Type}.", DataObjectTypeName);
            return Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Create Error", detail: $"Failed to create the {DataObjectTypeName.SpaceCapitalLetters()} record because of an error on the server.");
        }
    }

    /// <summary>
    /// The method deletes a data object using the data layer.
    /// </summary>
    /// <param name="id">The id for the data object.</param>
    /// <returns>An IActionResult object.</returns>
    [HttpDelete("{id:long}")]
    public virtual async Task<IActionResult> DeleteAsync(long id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.Integer64ID == id);

            if (dataObject is null)
            {
                Logger.LogWarning("The {ID} for the {Type} was not found so no delete occurred.", id.ToString(), DataObjectTypeName);
                return NotFound(new NotFoundDetails(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Delete Error - Not Found", detail: $"The {DataObjectTypeName.SpaceCapitalLetters()} record was not found; please refresh the page because another user may have deleted it."));
            }

            await DataLayer.DeleteAsync(dataObject);
            Logger.LogInformation("The {ID} for the {Type} was successfully deleted.", id, DataObjectTypeName);
            return Ok();
        }
        catch (DataObjectDeleteConflictException ex)
        {
            Logger.LogError(ex, "Failed to delete the {ID} {Type} because of a data conflict.", id, DataObjectTypeName);
            return Conflict(new ConflictDetails(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Delete Error - Data Conflict", detail: $"The {DataObjectTypeName.SpaceCapitalLetters()} record has a dependency that prevents it from being deleted; the dependency needs to be deleted first."));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete the {ID} {Type}.", id, DataObjectTypeName);
            return Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Delete Error", detail: $"Failed to delete the {DataObjectTypeName.SpaceCapitalLetters()} record because of an error on the server.");
        }
    }

    /// <summary>
    /// The method deletes a data object using the data layer.
    /// </summary>
    /// <param name="id">The id for the data object.</param>
    /// <returns>An IActionResult object.</returns>
    [HttpDelete("{id}")]
    public virtual async Task<IActionResult> DeleteAsync(string id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.StringID == id);

            if (dataObject is null)
            {
                Logger.LogWarning("The {ID} for the {Type} was not found so no delete occurred.", id.ToString(), DataObjectTypeName);
                return NotFound(new NotFoundDetails(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Delete Error - Not Found", detail: $"The {DataObjectTypeName.SpaceCapitalLetters()} record was not found; please refresh the page because another user may have deleted it."));
            }

            await DataLayer.DeleteAsync(dataObject);
            Logger.LogInformation("The {ID} for the {Type} was successfully deleted.", id, DataObjectTypeName);
            return Ok();
        }
        catch (DataObjectDeleteConflictException ex)
        {
            Logger.LogError(ex, "Failed to delete the {ID} {Type} because of a data conflict.", id, DataObjectTypeName);
            return Conflict(new ConflictDetails(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Delete Error - Data Conflict", detail: $"The {DataObjectTypeName.SpaceCapitalLetters()} record has a dependency that prevents it from being deleted; the dependency needs to be deleted first."));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete the {ID} {Type}.", id, DataObjectTypeName);
            return Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Delete Error", detail: $"Failed to delete the {DataObjectTypeName.SpaceCapitalLetters()} record because of an error on the server.");
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
            return Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Get All Error", detail: $"Failed to return all the {DataObjectTypeName.SpaceCapitalLetters()} records because of an error on the server.");
        }
    }

    /// <summary>
    /// The method returns all the data objects as list views using the data layer.
    /// </summary>
    /// <returns>A list of data objects.</returns>
    [HttpGet("All/ListView")]
    public async Task<IActionResult> GetAllListViewAsync()
    {
        try
        {
            List<ListView> listViews = await DataLayer.GetAllListViewAsync();
            return Ok(listViews);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return all the {Type} data objects as list views.", DataObjectTypeName);
            return Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Get All List View Error", detail: $"Failed to return all the {DataObjectTypeName.SpaceCapitalLetters()} records as list views because of an error on the server.");
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
            return Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Get Page Error", detail: $"Failed to return a page of the {DataObjectTypeName.SpaceCapitalLetters()} records because of an error on the server.");
        }
    }

    /// <summary>
    /// The method returns a page of data objects as list views using the data layer.
    /// </summary>
    /// <param name="queryDefinition">Defines how the data should be queried; includes filtering, paging and sorting.</param>
    /// <returns>A list of data objects.</returns>
    [HttpGet("Page/ListView")]
    public async Task<IActionResult> GetPageListViewAsync([FromQuery] QueryDefinition queryDefinition)
    {
        try
        {
            PagedList<ListView> listViews = await DataLayer.GetPageListViewAsync(queryDefinition);
            return Ok(listViews);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return a page of {Type} data objects as list views.", DataObjectTypeName);
            return Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Get Page List View Error", detail: $"Failed to return a page of the {DataObjectTypeName.SpaceCapitalLetters()} records as list views because of an error on the server.");
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
            return Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Get Single Error", detail: $"Failed to return the {DataObjectTypeName.SpaceCapitalLetters()} record because of an error on the server.");
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
            return Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Get Single Error", detail: $"Failed to return the {DataObjectTypeName.SpaceCapitalLetters()} record because of an error on the server.");
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
            return Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Get Single Error", detail: $"Failed to return the {DataObjectTypeName.SpaceCapitalLetters()} record because of an error on the server.");
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
        string id = string.IsNullOrEmpty(dataObject.StringID) ? dataObject.Integer64ID.ToString() : dataObject.StringID;

        try
        {
            dataObject = await DataLayer.UpdateAsync(dataObject);
            Logger.LogInformation("The {Type} was successfully updated.", DataObjectTypeName);
            return Ok(dataObject);
        }
        catch (DataObjectUpdateConflictException ex)
        {
            Logger.LogWarning(ex, "Failed to update {ID} {Type} because the data was considered old.", id, DataObjectTypeName);
            return Conflict(new ConflictDetails(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Update Error - Data Conflict", detail: $"The submitted {DataObjectTypeName.SpaceCapitalLetters()} data was detected to be out of date; please refresh the page and try again."));
        }
        catch (DataObjectValidationException ex)
        {
            Logger.LogWarning(ex, "Failed to update the {ID} {Type} because of a server-side validation error.", id, DataObjectTypeName);
            ex.CopyToModelState(ModelState);
            return base.ValidationProblem(ModelState);
        }
        catch (IDNotFoundException ex)
        {
            Logger.LogWarning(ex, "Failed to update the {ID} {Type} because it was not found.", id, DataObjectTypeName);
            return NotFound(new NotFoundDetails(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Update Error - Not Found", detail: $"The {DataObjectTypeName.SpaceCapitalLetters()} record was not found; please refresh the page because another user may have deleted it."));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to update the {Type} for {ID}.", DataObjectTypeName , id);
            return Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Update Error", detail: $"Failed to update the {DataObjectTypeName.SpaceCapitalLetters()} record because of an error on the server.");
        }
    }
}
