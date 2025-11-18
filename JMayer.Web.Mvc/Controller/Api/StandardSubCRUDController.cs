using JMayer.Data.Data;
using JMayer.Data.Data.Query;
using JMayer.Data.Database.DataLayer;
using JMayer.Web.Mvc.Extension;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JMayer.Web.Mvc.Controller.Api;

/// <summary>
/// The class manages HTTP requests for CRUD operations associated with a sub data object and a data layer.
/// </summary>
/// <typeparam name="T">Must be a SubUserEditableDataObject since the data layer requires this.</typeparam>
/// <typeparam name="U">Must be an IUserEditableDataLayer so the controller can interact with the collection/table associated with it.</typeparam>
[ApiController]
[Route("api/[controller]")]
public class StandardSubCRUDController<T, U> : StandardCRUDController<T, U>
    where T : SubDataObject
    where U : IStandardSubCRUDDataLayer<T>
{
    /// <inheritdoc/>
    public StandardSubCRUDController(U dataLayer, ILogger logger) : base(dataLayer, logger) { }

    /// <summary>
    /// The method returns all the sub data objects for an owner data object using the data layer.
    /// </summary>
    /// <param name="ownerId">The owner ID to filter for.</param>
    /// <returns>A list of data objects.</returns>
    [HttpGet("All/{ownerId:long}")]
    public virtual async Task<IActionResult> GetAllAsync(long ownerId)
    {
        try
        {
            Logger.LogInformation("Attempting to retrieve all the {Type} data objects for the owner {OwnerID}.", DataObjectTypeName, ownerId);

            List<T> dataObjects = await DataLayer.GetAllAsync(obj => obj.OwnerInteger64ID == ownerId);

            Logger.LogInformation("All the {Type} data objects were successfully retrieved for the owner {OwnerID}.", DataObjectTypeName, ownerId);

            return Ok(dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return all the {Type} data objects for owner {OwnerID}.", DataObjectTypeName, ownerId);
            return Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Get All Error", detail: $"Failed to return all the {DataObjectTypeName.SpaceCapitalLetters()} records for an owner because of an error on the server.");
        }
    }

    /// <summary>
    /// The method returns all the sub data objects for an owner data object using the data layer.
    /// </summary>
    /// <param name="ownerId">The owner ID to filter for.</param>
    /// <returns>A list of data objects.</returns>
    [HttpGet("All/{ownerId}")]
    public virtual async Task<IActionResult> GetAllAsync(string ownerId)
    {
        try
        {
            Logger.LogInformation("Attempting to retrieve all the {Type} data objects for the owner {OwnerID}.", DataObjectTypeName, ownerId);

            List<T> dataObjects = await DataLayer.GetAllAsync(obj => obj.OwnerStringID == ownerId);

            Logger.LogInformation("All the {Type} data objects were successfully retrieved for the owner {OwnerID}.", DataObjectTypeName, ownerId);

            return Ok(dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return all the {Type} data objects for owner {OwnerID}.", DataObjectTypeName, ownerId);
            return Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Get All Error", detail: $"Failed to return all the {DataObjectTypeName.SpaceCapitalLetters()} records for an owner because of an error on the server.");
        }
    }

    /// <summary>
    /// The method returns all the sub data objects for an owner data object as list views using the data layer.
    /// </summary>
    /// <param name="ownerId">The owner ID to filter for.</param>
    /// <returns>A list of list views.</returns>
    [HttpGet("All/ListView/{ownerId:long}")]
    public virtual async Task<IActionResult> GetAllListViewAsync(long ownerId)
    {
        try
        {
            Logger.LogInformation("Attempting to retrieve all the {Type} data objects as list views for the owner {OwnerID}.", DataObjectTypeName, ownerId);

            List<ListView> dataObjects = await DataLayer.GetAllListViewAsync(obj => obj.OwnerInteger64ID == ownerId);

            Logger.LogInformation("All the {Type} data objects as list views were successfully retrieved for the owner {OwnerID}.", DataObjectTypeName, ownerId);

            return Ok(dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return all the {Type} data objects as list views for owner {OwnerID}.", DataObjectTypeName, ownerId);
            return Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Get All List View Error", detail: $"Failed to return all the {DataObjectTypeName.SpaceCapitalLetters()} records as list views for an owner because of an error on the server.");
        }
    }

    /// <summary>
    /// The method returns all the sub data objects for an owner data object as list views using the data layer.
    /// </summary>
    /// <param name="ownerId">The owner ID to filter for.</param>
    /// <returns>A list of list views.</returns>
    [HttpGet("All/ListView/{ownerId}")]
    public virtual async Task<IActionResult> GetAllListViewAsync(string ownerId)
    {
        try
        {
            Logger.LogInformation("Attempting to retrieve all the {Type} data objects as list views for the owner {OwnerID}.", DataObjectTypeName, ownerId);

            List<ListView> dataObjects = await DataLayer.GetAllListViewAsync(obj => obj.OwnerStringID == ownerId);

            Logger.LogInformation("All the {Type} data objects as list views were successfully retrieved for the owner {OwnerID}.", DataObjectTypeName, ownerId);

            return Ok(dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return all the {Type} data objects as list views for owner {OwnerID}.", DataObjectTypeName, ownerId);
            return Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Get All List View Error", detail: $"Failed to return all the {DataObjectTypeName.SpaceCapitalLetters()} records as list views for an owner because of an error on the server.");
        }
    }

    /// <summary>
    /// The method returns a page of sub data objects for an owner data object using the data layer.
    /// </summary>
    /// <param name="ownerId">The owner ID to filter for.</param>
    /// <param name="queryDefinition">Defines how the data should be queried; includes filtering, paging and sorting.</param>
    /// <returns>A list of list views.</returns>
    [HttpGet("Page/{ownerId:long}")]
    public virtual async Task<IActionResult> GetPageAsync(long ownerId, [FromQuery] QueryDefinition queryDefinition)
    {
        try
        {
            Logger.LogInformation("Attempting to retrieve a page of {Type} data objects for the owner {OwnerID}.\n{QueryDefinition}", DataObjectTypeName, ownerId, queryDefinition.ToJson());

            //Insert the owner ID as a filter so its always returning a subset based on the owner.
            queryDefinition.FilterDefinitions.Insert(0, new FilterDefinition()
            {
                FilterOn = nameof(SubDataObject.OwnerInteger64ID),
                Operator = FilterDefinition.EqualsOperator,
                Value = ownerId.ToString(),
            });

            PagedList<T> dataObjects = await DataLayer.GetPageAsync(queryDefinition);

            Logger.LogInformation("A page of {Type} data objects for the owner {OwnerID} were successfully retrieved.\n{QueryDefinition}", DataObjectTypeName, ownerId, queryDefinition.ToJson());

            return Ok(dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return a page of the {Type} data objects for owner {OwnerID}.\n{QueryDefinition}", DataObjectTypeName, ownerId, queryDefinition.ToJson());
            return Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Get Page Error", detail: $"Failed to return a paged of {DataObjectTypeName.SpaceCapitalLetters()} records for an owner because of an error on the server.");
        }
    }

    /// <summary>
    /// The method returns a page of sub data objects for an owner data object using the data layer.
    /// </summary>
    /// <param name="ownerId">The owner ID to filter for.</param>
    /// <param name="queryDefinition">Defines how the data should be queried; includes filtering, paging and sorting.</param>
    /// <returns>A list of list views.</returns>
    [HttpGet("Page/{ownerId}")]
    public virtual async Task<IActionResult> GetPageAsync(string ownerId, [FromQuery] QueryDefinition queryDefinition)
    {
        try
        {
            Logger.LogInformation("Attempting to retrieve a page of {Type} data objects for the owner {OwnerID}\n{QueryDefinition}", DataObjectTypeName, ownerId, queryDefinition.ToJson());

            //Insert the owner ID as a filter so its always returning a subset based on the owner.
            queryDefinition.FilterDefinitions.Insert(0, new FilterDefinition()
            {
                FilterOn = nameof(SubDataObject.OwnerStringID),
                Operator = FilterDefinition.StringEqualsOperator,
                Value = ownerId.ToString(),
            });

            PagedList<T> dataObjects = await DataLayer.GetPageAsync(queryDefinition);

            Logger.LogInformation("A page of {Type} data objects for the owner {OwnerID} were successfully retrieved.\n{QueryDefinition}", DataObjectTypeName, ownerId, queryDefinition.ToJson());

            return Ok(dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return a page of the {Type} data objects for owner {OwnerID}.\n{QueryDefinition}", DataObjectTypeName, ownerId, queryDefinition.ToJson());
            return Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Get Page Error", detail: $"Failed to return a paged of {DataObjectTypeName.SpaceCapitalLetters()} records for an owner because of an error on the server.");
        }
    }

    /// <summary>
    /// The method returns a page of sub data objects for an owner data object as list views using the data layer.
    /// </summary>
    /// <param name="ownerId">The owner ID to filter for.</param>
    /// <param name="queryDefinition">Defines how the data should be queried; includes filtering, paging and sorting.</param>
    /// <returns>A list of list views.</returns>
    [HttpGet("Page/ListView/{ownerId:long}")]
    public virtual async Task<IActionResult> GetPageListViewAsync(long ownerId, [FromQuery] QueryDefinition queryDefinition)
    {
        try
        {
            Logger.LogInformation("Attempting to retrieve a page of {Type} data objects as list views for the owner {OwnerID}.\n{QueryDefinition}", DataObjectTypeName, ownerId, queryDefinition.ToJson());

            //Insert the owner ID as a filter so its always returning a subset based on the owner.
            queryDefinition.FilterDefinitions.Insert(0, new FilterDefinition()
            {
                FilterOn = nameof(SubDataObject.OwnerInteger64ID),
                Operator = FilterDefinition.EqualsOperator,
                Value = ownerId.ToString(),
            });

            PagedList<ListView> dataObjects = await DataLayer.GetPageListViewAsync(queryDefinition);

            Logger.LogInformation("A page of {Type} data objects  as list views for the owner {OwnerID} were successfully retrieved.\n{QueryDefinition}", DataObjectTypeName, ownerId, queryDefinition.ToJson());

            return Ok(dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return a page of the {Type} data objects as list views for owner {OwnerID}.\n{QueryDefinition}", DataObjectTypeName, ownerId, queryDefinition.ToJson());
            return Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Get Page List View Error", detail: $"Failed to return a paged of {DataObjectTypeName.SpaceCapitalLetters()} records as list views for an owner because of an error on the server.");
        }
    }

    /// <summary>
    /// The method returns a page of sub data objects for an owner data object as list views using the data layer.
    /// </summary>
    /// <param name="ownerId">The owner ID to filter for.</param>
    /// <param name="queryDefinition">Defines how the data should be queried; includes filtering, paging and sorting.</param>
    /// <returns>A list of list views.</returns>
    [HttpGet("Page/ListView/{ownerId}")]
    public virtual async Task<IActionResult> GetPageListViewAsync(string ownerId, [FromQuery] QueryDefinition queryDefinition)
    {
        try
        {
            Logger.LogInformation("Attempting to retrieve a page of {Type} data objects as list views for the owner {OwnerID}.\n{QueryDefinition}", DataObjectTypeName, ownerId, queryDefinition.ToJson());

            //Insert the owner ID as a filter so its always returning a subset based on the owner.
            queryDefinition.FilterDefinitions.Insert(0, new FilterDefinition()
            {
                FilterOn = nameof(SubDataObject.OwnerStringID),
                Operator = FilterDefinition.StringEqualsOperator,
                Value = ownerId.ToString(),
            });

            PagedList<ListView> dataObjects = await DataLayer.GetPageListViewAsync(queryDefinition);

            Logger.LogInformation("A page of {Type} data objects  as list views for the owner {OwnerID} were successfully retrieved.\n{QueryDefinition}", DataObjectTypeName, ownerId, queryDefinition.ToJson());

            return Ok(dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return a page of the {Type} data objects as list views for owner {OwnerID}.\n{QueryDefinition}", DataObjectTypeName, ownerId, queryDefinition.ToJson());
            return Problem(title: $"{DataObjectTypeName.SpaceCapitalLetters()} Get Page List View Error", detail: $"Failed to return a paged of {DataObjectTypeName.SpaceCapitalLetters()} records as list views for an owner because of an error on the server.");
        }
    }
}
