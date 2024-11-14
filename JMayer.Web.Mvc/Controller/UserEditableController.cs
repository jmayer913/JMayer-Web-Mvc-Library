using JMayer.Data.Data;
using JMayer.Data.Data.Query;
using JMayer.Data.Database.DataLayer;
using JMayer.Web.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace JMayer.Web.Mvc.Controller;

/// <summary>
/// The class manages HTTP requests for CRUD operations associated with an user editable data object and a data layer.
/// </summary>
/// <typeparam name="T">Must be a UserEditableDataObject since the data layer requires this.</typeparam>
/// <typeparam name="U">Must be an IUserEditableDataLayer so the controller can interact with the collection/table associated with it.</typeparam>
[ApiController]
[Route("api/[controller]")]
public class UserEditableController<T, U> : StandardCRUDController<T, U>
    where T : UserEditableDataObject
    where U : IUserEditableDataLayer<T>
{
    /// <summary>
    /// The dependency injection constructor.
    /// </summary>
    /// <param name="dataLayer">The data layer the controller will interact with.</param>
    /// <param name="logger">The logger the controller will interact with.</param>
    public UserEditableController(IUserEditableDataLayer<T> dataLayer, ILogger logger) : base(dataLayer, logger) { }

    /// <summary>
    /// The method returns all the data objects as list views using the data layer.
    /// </summary>
    /// <returns>A list of data objects.</returns>
    [HttpGet("All/ListView")]
    public async Task<IActionResult> GetAllListViewAsync()
    {
        try
        {
            List<ListView> listViews = await ((IUserEditableDataLayer<T>)DataLayer).GetAllListViewAsync();
            return Ok(listViews);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return all the {Type} data objects as list views.", DataObjectTypeName);
            return Problem();
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
            PagedList<ListView> listViews = await ((IUserEditableDataLayer<T>)DataLayer).GetPageListViewAsync(queryDefinition);
            return Ok(listViews);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return a page of {Type} data objects as list views.", DataObjectTypeName);
            return Problem();
        }
    }
}
