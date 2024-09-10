using JMayer.Data.Data;
using JMayer.Data.Data.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using TestProject.Controller;
using TestProject.Data;
using TestProject.Database;

namespace TestProject.Test.Controller;

/// <summary>
/// The class manages tests for the UserEditableContoller object.
/// </summary>
/// <remarks>
/// The tests are against a SimpleUserEditableController object which inherits from the UserEditableContoller and
/// the SimpleUserEditableController doesn't override any of the base methods. Because of this, we're testing 
/// the methods in the UserEditableContoller class.
/// 
/// The memory data layer is used and that utilizes an integer ID so the two string ID methods 
/// (DeleteAsync() and SingleAsync()) are not tested.
/// 
/// UserEditableController class inherits from the StandardCRUDController. Because of this,
/// only new and overriden methods in the UserEditableController are tested because
/// the StandardCRUDControllerUnitTest already tests the StandardCRUDController.
/// 
/// Not all negative responses can be tested and adding a property to force a certain response
/// is risky so if there's missing fact/theory that's why.
/// </remarks>
public class UserEditableControllerUnitTest
{
    /// <summary>
    /// The constant for the maximum records.
    /// </summary>
    private const int MaxRecords = 100;

    /// <summary>
    /// The method returns a console logger.
    /// </summary>
    /// <returns>An ILogger.</returns>
    private static ILogger CreateConsoleLogger()
    {
        return LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<SimpleUserEditableController>();
    }

    /// <summary>
    /// The method populates data objects in a datalayer with values that increment by 1.
    /// </summary>
    /// <param name="dataLayer">The data layer to populate</param>
    /// <returns>A Task object for the async.</returns>
    private static async Task PopulateDataObjects(SimpleUserEditableDataLayer dataLayer)
    {
        for (int index = 1; index <= MaxRecords; index++)
        {
            _ = await dataLayer.CreateAsync(new SimpleUserEditableDataObject() { Name = index.ToString(), Value = index });
        }
    }

    /// <summary>
    /// The method verifies the UserEditableContoller.GetAllListViewAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyGetAllListViewOkResponse()
    {
        SimpleUserEditableDataLayer dataLayer = new();
        await PopulateDataObjects(dataLayer);

        SimpleUserEditableController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.GetAllListViewAsync();

        Assert.IsType<OkObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<List<ListView>>(((OkObjectResult)actionResult).Value); //Confirm the action is responding with a list of list views.
    }

    /// <summary>
    /// The method verifies the UserEditableContoller.GetPageListViewAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyGetPageListViewInternalErrorResponse()
    {
        SimpleUserEditableController simpleCRUDController = new(new SimpleUserEditableDataLayer(), CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.GetPageListViewAsync(null);

        Assert.IsType<StatusCodeResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal((int)HttpStatusCode.InternalServerError, ((StatusCodeResult)actionResult).StatusCode); //Confirm the correct HTTP status code is returned.
    }

    /// <summary>
    /// The method verifies the UserEditableContoller.GetPageListViewAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyGetPageListViewOkResponse()
    {
        SimpleUserEditableDataLayer dataLayer = new();
        await PopulateDataObjects(dataLayer);

        QueryDefinition queryDefinition = new()
        {
            Skip = 0,
            Take = 20,
        };
        SimpleUserEditableController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.GetPageListViewAsync(queryDefinition);

        Assert.IsType<OkObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<PagedList<ListView>>(((OkObjectResult)actionResult).Value); //Confirm the action is responding with a paged list of list views.
    }

    /// <summary>
    /// The method verifies the StandardCRUDContoller.UpdateAsync() returns a 409 (Conflict) response when the data object is old.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyUpdateConflictResponse()
    {
        SimpleUserEditableDataLayer dataLayer = new();
        SimpleUserEditableDataObject dataObject = await dataLayer.CreateAsync(new SimpleUserEditableDataObject() { Name = "10", Value = 10 });
        await dataLayer.UpdateAsync(new SimpleUserEditableDataObject(dataObject)
        {
            Value = 20,
        });

        dataObject.Value = 30;
        SimpleUserEditableController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.UpdateAsync(dataObject);

        Assert.IsType<ConflictResult>(actionResult);
    }
}
