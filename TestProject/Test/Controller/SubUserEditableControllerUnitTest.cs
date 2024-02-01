using JMayer.Data.Data.Query;
using JMayer.Data.Data;
using Microsoft.AspNetCore.Mvc;
using TestProject.Controller;
using TestProject.Data;
using TestProject.Database;
using Microsoft.Extensions.Logging;

namespace TestProject.Test.Controller;

/// <summary>
/// The class manages tests for the SubUserEditableContoller object.
/// </summary>
/// <remarks>
/// The tests are against a SimpleSubUserEditableController object which inherits from the SubUserEditableContoller and
/// the SimpleSubUserEditableController doesn't override any of the base methods. Because of this, we're testing 
/// the methods in the SubUserEditableContoller class.
/// 
/// The memory data layer is used and that utilizes an integer ID so the two string ID methods 
/// (DeleteAsync() and SingleAsync()) are not tested.
/// 
/// SubUserEditableController class inherits from the UserEditableController. Because of this,
/// only new and overriden methods in the SubUserEditableController are tested because
/// the UserEditableControllerUnitTest already tests the UserEditableController.
/// </remarks>
public class SubUserEditableControllerUnitTest
{
    /// <summary>
    /// The method returns a console logger.
    /// </summary>
    /// <returns>An ILogger.</returns>
    private static ILogger CreateConsoleLogger()
    {
        return LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<SimpleCRUDController>();
    }

    /// <summary>
    /// The method confirms the SubUserEditableContoller.GetAllAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task GetAllAsyncInternalErrorResponse()
    {
        throw new NotImplementedException("Cannot test this. There isn't an easy way to force an exception in the SimpleSubUserEditableMemoryDataLayer.");
    }

    /// <summary>
    /// The method confirms the SubUserEditableContoller.GetAllAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task GetAllAsyncOkResponse()
    {
        SimpleSubUserEditableMemoryDataLayer dataLayer = new();

        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "1-10", OwnerInteger64ID = 1, Value = 10 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "1-20", OwnerInteger64ID = 1, Value = 20 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "1-30", OwnerInteger64ID = 1, Value = 30 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "1-40", OwnerInteger64ID = 1, Value = 40 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "1-50", OwnerInteger64ID = 1, Value = 50 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "1-60", OwnerInteger64ID = 1, Value = 60 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "1-70", OwnerInteger64ID = 1, Value = 70 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "1-80", OwnerInteger64ID = 1, Value = 80 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "1-90", OwnerInteger64ID = 1, Value = 90 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "1-100", OwnerInteger64ID = 1, Value = 100 });

        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "2-10", OwnerInteger64ID = 2, Value = 10 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "2-20", OwnerInteger64ID = 2, Value = 20 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "2-30", OwnerInteger64ID = 2, Value = 30 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "2-40", OwnerInteger64ID = 2, Value = 40 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "2-50", OwnerInteger64ID = 2, Value = 50 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "2-60", OwnerInteger64ID = 2, Value = 60 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "2-70", OwnerInteger64ID = 2, Value = 70 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "2-80", OwnerInteger64ID = 2, Value = 80 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "2-90", OwnerInteger64ID = 2, Value = 90 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "2-100", OwnerInteger64ID = 2, Value = 100 });

        SimpleSubUserEditableController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.GetAllAsync(1);

        Assert.True
        (
            actionResult is OkObjectResult okObjectResult //Confirm the correct action is returned.
            && okObjectResult.Value is List<SimpleSubUserEditableDataObject> list //Confirm the action is responding with a list of data objects.
            && list.Count == 10 //Confirm the list matches the amount created.
        );
    }

    /// <summary>
    /// The method confirms the SubUserEditableContoller.GetAllListViewAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task GetAllListViewAsyncInternalErrorResponse()
    {
        throw new NotImplementedException("Cannot test this. There isn't an easy way to force an exception in the SimpleSubUserEditableMemoryDataLayer.");
    }

    /// <summary>
    /// The method confirms the SubUserEditableContoller.GetAllListViewAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task GetAllListViewAsyncOkResponse()
    {
        SimpleSubUserEditableMemoryDataLayer dataLayer = new();

        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "1-10", OwnerInteger64ID = 1, Value = 10 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "1-20", OwnerInteger64ID = 1, Value = 20 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "1-30", OwnerInteger64ID = 1, Value = 30 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "1-40", OwnerInteger64ID = 1, Value = 40 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "1-50", OwnerInteger64ID = 1, Value = 50 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "1-60", OwnerInteger64ID = 1, Value = 60 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "1-70", OwnerInteger64ID = 1, Value = 70 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "1-80", OwnerInteger64ID = 1, Value = 80 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "1-90", OwnerInteger64ID = 1, Value = 90 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "1-100", OwnerInteger64ID = 1, Value = 100 });

        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "2-10", OwnerInteger64ID = 2, Value = 10 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "2-20", OwnerInteger64ID = 2, Value = 20 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "2-30", OwnerInteger64ID = 2, Value = 30 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "2-40", OwnerInteger64ID = 2, Value = 40 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "2-50", OwnerInteger64ID = 2, Value = 50 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "2-60", OwnerInteger64ID = 2, Value = 60 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "2-70", OwnerInteger64ID = 2, Value = 70 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "2-80", OwnerInteger64ID = 2, Value = 80 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "2-90", OwnerInteger64ID = 2, Value = 90 });
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = "2-100", OwnerInteger64ID = 2, Value = 100 });

        SimpleSubUserEditableController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.GetAllListViewAsync(2);

        Assert.True
        (
            actionResult is OkObjectResult okObjectResult //Confirm the correct action is returned.
            && okObjectResult.Value is List<ListView> list //Confirm the action is responding with a list of list views.
            && list.Count == 10 //Confirm the list matches the amount created.
        );
    }

    /// <summary>
    /// The method confirms the SubUserEditableContoller.GetPageAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task GetPageAsyncInternalErrorResponse()
    {
        throw new NotImplementedException("Cannot test this. There isn't an easy way to force an exception in the SimpleSubUserEditableMemoryDataLayer.");
    }

    /// <summary>
    /// The method confirms the SubUserEditableContoller.GetPageAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task GetPageAsyncOkResponse()
    {
        SimpleSubUserEditableMemoryDataLayer dataLayer = new();

        for (int index = 1; index <= 100; index++)
        {
            _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = $"1-{index}", OwnerInteger64ID = 1, Value = index });
        }

        for (int index = 1; index <= 100; index++)
        {
            _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = $"2-{index}", OwnerInteger64ID = 2, Value = index });
        }

        QueryDefinition queryDefinition = new()
        {
            Skip = 0,
            Take = 20,
        };
        SimpleSubUserEditableController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.GetPageAsync(1, queryDefinition);

        Assert.True
        (
            actionResult is OkObjectResult okObjectResult //Confirm the correct action is returned.
            && okObjectResult.Value is PagedList<SimpleSubUserEditableDataObject> list //Confirm the action is responding with a list of data objects.
            && list.DataObjects.Count == queryDefinition.Take //Confirm the list matches the amount taken.
            && list.TotalRecords == 100 //Confirm the list matches the amount created.
        );
    }

    /// <summary>
    /// The method confirms the SubUserEditableContoller.GetPageListViewAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task GetPageListViewAsyncInternalErrorResponse()
    {
        throw new NotImplementedException("Cannot test this. There isn't an easy way to force an exception in the SimpleSubUserEditableMemoryDataLayer.");
    }

    /// <summary>
    /// The method confirms the SubUserEditableContoller.GetPageListViewAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task GetPageListViewAsyncOkResponse()
    {
        SimpleSubUserEditableMemoryDataLayer dataLayer = new();

        for (int index = 1; index <= 100; index++)
        {
            _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = $"1-{index}", OwnerInteger64ID = 1, Value = index });
        }

        for (int index = 1; index <= 100; index++)
        {
            _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = $"2-{index}", OwnerInteger64ID = 2, Value = index });
        }

        QueryDefinition queryDefinition = new()
        {
            Skip = 0,
            Take = 20,
        };
        SimpleSubUserEditableController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.GetPageListViewAsync(2, queryDefinition);

        Assert.True
        (
            actionResult is OkObjectResult okObjectResult //Confirm the correct action is returned.
            && okObjectResult.Value is PagedList<ListView> list //Confirm the action is responding with a list of list views.
            && list.DataObjects.Count == queryDefinition.Take //Confirm the list matches the amount taken.
            && list.TotalRecords == 100 //Confirm the list matches the amount created.
        );
    }
}
