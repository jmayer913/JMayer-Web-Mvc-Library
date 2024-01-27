﻿using JMayer.Data.Data;
using JMayer.Data.Data.Query;
using JMayer.Data.Database.DataLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace JMayer.Web.Mvc.Controller;

/// <summary>
/// The class manages HTTP requests for CRUD operations associated with a sub user editable data object & a data layer.
/// </summary>
/// <typeparam name="T">Must be a SubUserEditableDataObject since the data layer requires this.</typeparam>
/// <typeparam name="U">Must be an IUserEditableDataLayer so the controller can interact with the collection/table associated with it.</typeparam>
[ApiController]
[Route("api/[controller]")]
public class SubUserEditableController<T, U> : UserEditableController<T, U>
    where T : SubUserEditableDataObject
    where U : IUserEditableDataLayer<T>
{
    /// <inheritdoc/>
    public SubUserEditableController(IUserEditableDataLayer<T> dataLayer, ILogger logger) : base(dataLayer, logger) { }

    /// <summary>
    /// The method returns all the sub data objects for an owner data object using the data layer.
    /// </summary>
    /// <param name="ownerIntegerId">The owner ID to filter for.</param>
    /// <returns>A list of data objects.</returns>
    [HttpGet("All/{ownerIntegerId:long}")]
    public virtual async Task<IActionResult> GetAllAsync(long ownerIntegerId)
    {
        try
        {
            List<T> dataObjects = await DataLayer.GetAllAsync(obj => obj.Integer64ID == ownerIntegerId);
            return Ok(dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return all the {Type} data objects for {OwnerID}.", DataObjectTypeName, ownerIntegerId);
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// The method returns all the sub data objects for an owner data object using the data layer.
    /// </summary>
    /// <param name="ownerStringId">The owner ID to filter for.</param>
    /// <returns>A list of data objects.</returns>
    [HttpGet("All/{ownerStringId}")]
    public virtual async Task<IActionResult> GetAllAsync(string ownerStringId)
    {
        try
        {
            List<T> dataObjects = await DataLayer.GetAllAsync(obj => obj.StringID == ownerStringId);
            return Ok(dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return all the {Type} data objects for {OwnerID}.", DataObjectTypeName, ownerStringId);
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// The method returns all the sub data objects for an owner data object as list views using the data layer.
    /// </summary>
    /// <param name="ownerIntegerId">The owner ID to filter for.</param>
    /// <returns>A list of list views.</returns>
    [HttpGet("All/ListView/{ownerIntegerId:long}")]
    public virtual async Task<IActionResult> GetAllListViewAsync(long ownerIntegerId)
    {
        try
        {
            List<ListView> dataObjects = await ((IUserEditableDataLayer<T>)DataLayer).GetAllListViewAsync(obj => obj.Integer64ID == ownerIntegerId);
            return Ok(dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return all the {Type} data objects as list views for {OwnerID}.", DataObjectTypeName, ownerIntegerId);
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// The method returns all the sub data objects for an owner data object as list views using the data layer.
    /// </summary>
    /// <param name="ownerStringId">The owner ID to filter for.</param>
    /// <returns>A list of list views.</returns>
    [HttpGet("All/ListView/{ownerStringId}")]
    public virtual async Task<IActionResult> GetAllListViewAsync(string ownerStringId)
    {
        try
        {
            List<ListView> dataObjects = await ((IUserEditableDataLayer<T>)DataLayer).GetAllListViewAsync(obj => obj.StringID == ownerStringId);
            return Ok(dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return all the {Type} data objects as list views for {OwnerID}.", DataObjectTypeName, ownerStringId);
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// The method returns a page of sub data objects for an owner data object using the data layer.
    /// </summary>
    /// <param name="ownerIntegerId">The owner ID to filter for.</param>
    /// <param name="queryDefinition">Defines how the data should be queried; includes filtering, paging and sorting.</param>
    /// <returns>A list of list views.</returns>
    [HttpGet("Page/{ownerIntegerId:long}")]
    public virtual async Task<IActionResult> GetPageAsync(long ownerIntegerId, [FromQuery] QueryDefinition queryDefinition)
    {
        try
        {
            //Insert the owner ID as a filter so its always returning a subset based on the owner.
            queryDefinition.FilterDefinitions.Insert(0, new FilterDefinition()
            {
                FilterOn = nameof(SubUserEditableDataObject.OwnerInteger64ID),
                Operator = FilterDefinition.EqualsOperator,
                Value = ownerIntegerId.ToString(),
            });

            PagedList<T> dataObjects = await DataLayer.GetPageAsync(queryDefinition);
            return Ok(dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return a page of the {Type} data objects for {OwnerID}.", DataObjectTypeName, ownerIntegerId);
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// The method returns a page of sub data objects for an owner data object using the data layer.
    /// </summary>
    /// <param name="ownerStringId">The owner ID to filter for.</param>
    /// <param name="queryDefinition">Defines how the data should be queried; includes filtering, paging and sorting.</param>
    /// <returns>A list of list views.</returns>
    [HttpGet("Page/{ownerStringId}")]
    public virtual async Task<IActionResult> GetPageAsync(string ownerStringId, [FromQuery] QueryDefinition queryDefinition)
    {
        try
        {
            //Insert the owner ID as a filter so its always returning a subset based on the owner.
            queryDefinition.FilterDefinitions.Insert(0, new FilterDefinition()
            {
                FilterOn = nameof(SubUserEditableDataObject.OwnerStringID),
                Operator = FilterDefinition.EqualsOperator,
                Value = ownerStringId.ToString(),
            });

            PagedList<T> dataObjects = await DataLayer.GetPageAsync(queryDefinition);
            return Ok(dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return a page of the {Type} data objects for {OwnerID}.", DataObjectTypeName, ownerStringId);
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// The method returns a page of sub data objects for an owner data object as list views using the data layer.
    /// </summary>
    /// <param name="ownerIntegerId">The owner ID to filter for.</param>
    /// <param name="queryDefinition">Defines how the data should be queried; includes filtering, paging and sorting.</param>
    /// <returns>A list of list views.</returns>
    [HttpGet("Page/ListView/{ownerIntegerId:long}")]
    public virtual async Task<IActionResult> GetPageListViewAsync(long ownerIntegerId, [FromQuery] QueryDefinition queryDefinition)
    {
        try
        {
            //Insert the owner ID as a filter so its always returning a subset based on the owner.
            queryDefinition.FilterDefinitions.Insert(0, new FilterDefinition()
            {
                FilterOn = nameof(SubUserEditableDataObject.OwnerInteger64ID),
                Operator = FilterDefinition.EqualsOperator,
                Value = ownerIntegerId.ToString(),
            });

            PagedList<ListView> dataObjects = await ((IUserEditableDataLayer<T>)DataLayer).GetPageListViewAsync(queryDefinition);
            return Ok(dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return a page of the {Type} data objects as list views for {OwnerID}.", DataObjectTypeName, ownerIntegerId);
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// The method returns a page of sub data objects for an owner data object as list views using the data layer.
    /// </summary>
    /// <param name="ownerStringId">The owner ID to filter for.</param>
    /// <param name="queryDefinition">Defines how the data should be queried; includes filtering, paging and sorting.</param>
    /// <returns>A list of list views.</returns>
    [HttpGet("Page/ListView/{ownerStringId}")]
    public virtual async Task<IActionResult> GetPageListViewAsync(string ownerStringId, [FromQuery] QueryDefinition queryDefinition)
    {
        try
        {
            //Insert the owner ID as a filter so its always returning a subset based on the owner.
            queryDefinition.FilterDefinitions.Insert(0, new FilterDefinition()
            {
                FilterOn = nameof(SubUserEditableDataObject.OwnerStringID),
                Operator = FilterDefinition.EqualsOperator,
                Value = ownerStringId.ToString(),
            });

            PagedList<ListView> dataObjects = await ((IUserEditableDataLayer<T>)DataLayer).GetPageListViewAsync(queryDefinition);
            return Ok(dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return a page of the {Type} data objects as list views for {OwnerID}.", DataObjectTypeName, ownerStringId);
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
}
