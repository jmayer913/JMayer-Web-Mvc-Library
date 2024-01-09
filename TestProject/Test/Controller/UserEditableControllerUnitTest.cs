using JMayer.Data.Data;
using JMayer.Data.Data.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
/// </remarks>
public class UserEditableControllerUnitTest
{
    /// <summary>
    /// The method returns a console logger.
    /// </summary>
    /// <returns>An ILogger.</returns>
    private static ILogger CreateConsoleLogger()
    {
        return LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<SimpleUserEditableController>();
    }

    /// <summary>
    /// The method confirms the UserEditableContoller.GetAllListViewAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task GetAllListViewAsyncInternalErrorResponse()
    {
        throw new NotImplementedException("Cannot test this. There isn't an easy way to force an exception in the SimpleUserEditableMemoryDataLayer.");
    }

    /// <summary>
    /// The method confirms the UserEditableContoller.GetAllListViewAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task GetAllListViewAsyncOkResponse()
    {
        SimpleUserEditableMemoryDataLayer dataLayer = new();

        _ = await dataLayer.CreateAsync(new SimpleUserEditableDataObject() { Name = "10", Value = 10 });
        _ = await dataLayer.CreateAsync(new SimpleUserEditableDataObject() { Name = "20", Value = 20 });
        _ = await dataLayer.CreateAsync(new SimpleUserEditableDataObject() { Name = "30", Value = 30 });
        _ = await dataLayer.CreateAsync(new SimpleUserEditableDataObject() { Name = "40", Value = 40 });
        _ = await dataLayer.CreateAsync(new SimpleUserEditableDataObject() { Name = "50", Value = 50 });
        _ = await dataLayer.CreateAsync(new SimpleUserEditableDataObject() { Name = "60", Value = 60 });
        _ = await dataLayer.CreateAsync(new SimpleUserEditableDataObject() { Name = "70", Value = 70 });
        _ = await dataLayer.CreateAsync(new SimpleUserEditableDataObject() { Name = "80", Value = 80 });
        _ = await dataLayer.CreateAsync(new SimpleUserEditableDataObject() { Name = "90", Value = 90 });
        _ = await dataLayer.CreateAsync(new SimpleUserEditableDataObject() { Name = "100", Value = 100 });

        SimpleUserEditableController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.GetAllListViewAsync();

        Assert.True
        (
            actionResult is OkObjectResult okObjectResult //Confirm the correct action is returned.
            && okObjectResult.Value is List<ListView> list //Confirm the action is responding with a list of list views.
            && list.Count == 10 //Confirm the list matches the amount created.
        );
    }

    /// <summary>
    /// The method confirms the UserEditableContoller.GetPageListViewAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task GetPageListViewAsyncInternalErrorResponse()
    {
        throw new NotImplementedException("Cannot test this. There isn't an easy way to force an exception in the SimpleUserEditableMemoryDataLayer.");
    }

    /// <summary>
    /// The method confirms the UserEditableContoller.GetPageListViewAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task GetPageListViewAsyncOkResponse()
    {
        SimpleUserEditableMemoryDataLayer dataLayer = new();

        for (int index = 1; index <= 100; index++)
        {
            _ = await dataLayer.CreateAsync(new SimpleUserEditableDataObject() { Name = index.ToString(), Value = index });
        }

        QueryDefinition queryDefinition = new()
        {
            Skip = 0,
            Take = 20,
        };
        SimpleUserEditableController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.GetPageListViewAsync(queryDefinition);

        Assert.True
        (
            actionResult is OkObjectResult okObjectResult //Confirm the correct action is returned.
            && okObjectResult.Value is List<ListView> list //Confirm the action is responding with a list of list views.
            && list.Count == queryDefinition.Take //Confirm the list matches the amount created.
        );
    }

    /// <summary>
    /// The method confirms the StandardCRUDContoller.UpdateAsync() returns a 409 (Conflict) response when the data object is old.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task UpdateAsyncConflictResponse()
    {
        SimpleUserEditableMemoryDataLayer dataLayer = new();
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
