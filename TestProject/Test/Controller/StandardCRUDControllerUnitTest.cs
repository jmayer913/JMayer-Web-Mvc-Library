using JMayer.Data.HTTP.DataLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TestProject.Controller;
using TestProject.Data;
using TestProject.Database;

#warning Right now, I've created a console logger just so the test don't fail. Need to come up with a better solution.

namespace TestProject.Test.Controller
{
    /// <summary>
    /// The class manages tests for the StandardCRUDContoller object.
    /// </summary>
    /// <remarks>
    /// The tests are against a SimpleCRUDController object which inherits from the StandardCRUDController and
    /// the SimpleCRUDController doesn't override any of the base methods. Because of this, we're testing 
    /// the methods in the StandardCRUDController class.
    /// </remarks>
    public class StandardCRUDControllerUnitTest
    {
        /// <summary>
        /// The method confirms the StandardCRUDContoller.CountAsync() returns a 200 (OK) response when ran successfully.
        /// </summary>
        /// <returns>A Task object for the async.</returns>
        [Fact]
        public async Task CountAsyncOkResponse()
        {
            SimpleListDataLayer dataLayer = new();
            _ = await dataLayer.CreateAsync(new SimpleDataObject());

            SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
            IActionResult actionResult = await simpleCRUDController.CountAsync();

            Assert.True
            (
                actionResult is OkObjectResult okObjectResult //Confirm the correct action is returned.
                && okObjectResult.Value is int value //Confirm the action is responding with an integer value.
                && value == 1 //Confirm its the correct count.
            );
        }

        /// <summary>
        /// The method confirms the StandardCRUDContoller.CountAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
        /// </summary>
        /// <returns>A Task object for the async.</returns>
        [Fact]
        public async Task CountAsyncInternalErrorResponse()
        {
            //TO DO: Figure out how to force an error in the data layer.
        }

        /// <summary>
        /// The method confirms the StandardCRUDContoller.CreateAsync() returns a 200 (OK) response when ran successfully.
        /// </summary>
        /// <returns>A Task object for the async.</returns>
        [Fact]
        public async Task CreateAsyncOkResponse()
        {
            SimpleDataObject originalSimpleDataObject = new() { Value = 10 };
            SimpleListDataLayer dataLayer = new();
            SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
            IActionResult actionResult = await simpleCRUDController.CreateAsync(originalSimpleDataObject);

            Assert.True
            (
                actionResult is OkObjectResult okObjectResult //Confirm the correct action is returned.
                && okObjectResult.Value is SimpleDataObject returnedSimpleDataObject //Confirm the action is responding with a data object.
                && returnedSimpleDataObject.Integer64ID == 1 && returnedSimpleDataObject.Value == originalSimpleDataObject.Value //Confirm the action is responding with the created data object.
            );
        }

        /// <summary>
        /// The method confirms the StandardCRUDContoller.CreateAsync() returns a 400 (Bad Request) response when the data object is not valid.
        /// </summary>
        /// <returns>A Task object for the async.</returns>
        [Fact]
        public async Task CreateAsyncBadRequestResponse()
        {
            SimpleListDataLayer dataLayer = new();
            SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
            IActionResult actionResult = await simpleCRUDController.CreateAsync(new SimpleDataObject() { Value = 9999 });

            Assert.True
            (
                actionResult is BadRequestObjectResult badRequestObjectResult //Confirm the correct action is returned.
                && badRequestObjectResult.Value is ServerSideValidationResult serverSideValidationResult //Confirm the action is responding with a validation result.
                && !serverSideValidationResult.IsSuccess //Confirm the action is responding with a non-successful validation result.
            );
        }

        /// <summary>
        /// The method confirms the StandardCRUDContoller.CreateAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
        /// </summary>
        /// <returns>A Task object for the async.</returns>
        [Fact]
        public async Task CreateAsyncInternalErrorResponse()
        {
            //TO DO: Figure out how to force an error in the data layer.
        }

        /// <summary>
        /// The method returns a console logger.
        /// </summary>
        /// <returns>An ILogger.</returns>
        private static ILogger CreateConsoleLogger()
        {
            return LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<SimpleCRUDController>();
        }

        /// <summary>
        /// The method confirms the StandardCRUDContoller.DeleteAsync() returns a 200 (OK) response when ran successfully.
        /// </summary>
        /// <returns>A Task object for the async.</returns>
        [Fact]
        public async Task DeleteAsyncOkResponse()
        {
            SimpleListDataLayer dataLayer = new();
            SimpleDataObject dataObject = await dataLayer.CreateAsync(new SimpleDataObject());

            SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
            IActionResult actionResult = await simpleCRUDController.DeleteAsync(dataObject?.Integer64ID ?? 0);

            //Confirm the correct action is returned.
            Assert.IsType<OkResult>(actionResult);
        }

        /// <summary>
        /// The method confirms the StandardCRUDContoller.DeleteAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
        /// </summary>
        /// <returns>A Task object for the async.</returns>
        [Fact]
        public async Task DeleteAsyncInternalErrorResponse()
        {
            //TO DO: Figure out how to force an error in the data layer.
        }

        /// <summary>
        /// The method confirms the StandardCRUDContoller.GetAllAsync() returns a 200 (OK) response when ran successfully.
        /// </summary>
        /// <returns>A Task object for the async.</returns>
        [Fact]
        public async Task GetAllAsyncOkResponse()
        {
            SimpleListDataLayer dataLayer = new();

            _ = await dataLayer.CreateAsync(new SimpleDataObject() { Value = 10 });
            _ = await dataLayer.CreateAsync(new SimpleDataObject() { Value = 20 });
            _ = await dataLayer.CreateAsync(new SimpleDataObject() { Value = 30 });
            _ = await dataLayer.CreateAsync(new SimpleDataObject() { Value = 40 });
            _ = await dataLayer.CreateAsync(new SimpleDataObject() { Value = 50 });
            _ = await dataLayer.CreateAsync(new SimpleDataObject() { Value = 60 });
            _ = await dataLayer.CreateAsync(new SimpleDataObject() { Value = 70 });
            _ = await dataLayer.CreateAsync(new SimpleDataObject() { Value = 80 });
            _ = await dataLayer.CreateAsync(new SimpleDataObject() { Value = 90 });
            _ = await dataLayer.CreateAsync(new SimpleDataObject() { Value = 100 });

            SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
            IActionResult actionResult = await simpleCRUDController.GetAllAsync();

            Assert.True
            (
                actionResult is OkObjectResult okObjectResult //Confirm the correct action is returned.
                && okObjectResult.Value is List<SimpleDataObject> list //Confirm the action is responding with a list of data objects.
                && list.Count == 10 //Confirm the list matches the amount created.
            );
        }

        /// <summary>
        /// The method confirms the StandardCRUDContoller.GetAllAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
        /// </summary>
        /// <returns>A Task object for the async.</returns>
        [Fact]
        public async Task GetAllAsyncInternalErrorResponse()
        {
            //TO DO: Figure out how to force an error in the data layer.
        }
    }
}
