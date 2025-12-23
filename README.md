# Web Server Library
This library will help you define a common MVC or Web API interface for your ASP.NET Core applications. It's dependent on the JMayer.Data package and this version will always match the JMayer.Data version.

## MVC
MVC is used in a server-side only ASP.NET Core project where the project uses the Model-View-Controller pattern. A Model represents the data. A View represents UI for a Model. A Controller represents actions for the View and Model. You can learn more [here](https://learn.microsoft.com/en-us/aspnet/core/mvc/overview).

### How to Create Your MVC Controller
See the JMayer.Data library readme for the Account data object and IAccountDataLayer (database datalayer). The controller utlizes dependency injection to accept an IAccountDataLayer and ILogger interfaces and it interacts with those interfaces when requests are processed.
```
public AccountController : StandardModelViewController<Account, IAccountDataLayer>
{
  public AccountController(IAccountDataLayer dataLayer, ILogger<AccountController> logger) : base(dataLayer, logger) { }
}
```
You now have a MVC controller for Account actions and your ASP.NET Core application will automatically expose that. It will accept GET & POST calls for /Account. The same can be done when using the StandardSubModelViewController.

### Ajax
Ajax is a pattern where the view contains javascript and javascript will be used to remotely interact with the controller. When the Ajax pattern is used, how the controller responds will need to be altered. In your constructor, you'll need to set a few properties to alter the default.
```
public AccountController : StandardModelViewController<Account, IAccountDataLayer>
{
  public AccountController(IAccountDataLayer dataLayer, ILogger<AccountController> logger) : base(dataLayer, logger)
  {
    //Json of the object will be returned so javascript can update a table.
    IsCUDActionRedirectedOnSuccess = false;

    //Json of the ModelState will be returned so javascript can display validation errors on the UI.
    ValidationFailedAction = ValidationFailedAction.ReturnJson;

    //Not required unless you want to use the details returned as a message to the user.
    IsDetailsIncludedInNegativeResponse = true;
  }
}
```

### Negative Responses
The StandardModelViewController and StandardSubModelViewController can return the following negative responses, 404 (Not Found), 409 (Conflict) and 500 (Internal Server Error). With the MVC pattern, the expectation is to handle those statuses with the middleware using UseStatusCodePagesWithRedirects() or using your own solution. With 
UseStatusCodePagesWithRedirects(), do not set IsDetailsIncludedInNegativeResponse to true else the redirect won't work. With the Ajax pattern, your javascript will need to handle those statuses when they are returned.

### GET Index View
The StandardModelViewController and StandardSubModelViewController will accept a GET /Account/Index web request. The standard is to return a view named "AccountIndex". The view should have UI that displays a list of accounts.

* A ViewResult object is returned by the controller with the account objects; empty list if nothing is found.
* If any exception is thrown, a 500 (Internal Server Error) response will be returned by the controller. The IsDetailsIncludedInNegativeResponse property controls if a ProblemDetails object is returned or not.

### GET Add View
The StandardModelViewController will accept a GET /Account/AddView web request. The standard is to return a view named "AccountAdd". The view should have UI for creating a new account and it should utilize the POST Create action.
* A ViewResult object is returned by the controller. No model is included.
* If any exception is thrown, a 500 (Internal Server Error) response will be returned by the controller. The IsDetailsIncludedInNegativeResponse property controls if a ProblemDetails object is returned or not.

### GET Add Partial View
The StandardModelViewController will accept a GET /Account/AddPartialView web request. The standard is to return a partial view named "_AccountAddPartial". The view should have UI for creating a new account and it should utilize the POST Create action.

* A PartialViewResult object is returned by the controller. No model is included.
* If any exception is thrown, a 500 (Internal Server Error) response will be returned by the controller. The IsDetailsIncludedInNegativeResponse property controls if a ProblemDetails object is returned or not.

### POST Create
The StandardModelViewController and StandardSubModelViewController will accept a POST /Account/Create web request; the body must contain an Account object. The standard is to call the IAccountDataLayer.CreateAsync() method.

* If the data object creation is successful, a 302 (Redirect) to Index will be returned by the controller or json of the Account object will be returned by the controller. The return action is controlled by the IsCUDActionRedirectedOnSuccess property.
* If the model state is invalid or a DataObjectValidationException is thrown, a ViewResult or PartialViewResult object is returned by the controller or a 400 (Bad Request) response is returned by the controller with a ValidationProblemDetails object. The return action is controlled by the ValidationFailedAction property.
* If any other exception is thrown, a 500 (Internal Server Error) response will be returned by the controller. The IsDetailsIncludedInNegativeResponse property controls if a ProblemDetails object is returned or not.

### GET Delete View
The StandardModelViewController and StandardSubModelViewController will accept a GET /Account/DeleteView/*id* web request; id will be DataObject.Integer64ID or DataObject.StringID. The standard is to return a view named "AccountDelete". The view should have UI for deleting an account and it should utilize the POST Delete action.

* When the data object is found using the id, a ViewResult object is returned by the controller with an account object.
* When the data object is not found using the id, a 404 (Not Found) response will be returned by the controller. The IsDetailsIncludedInNegativeResponse property controls if a ProblemDetails object is returned or not.
* On any exception thrown, a 500 (Internal Server Error) response will be returned by the controller. The IsDetailsIncludedInNegativeResponse property controls if a ProblemDetails object is returned or not.

### GET Delete Partial View
The StandardModelViewController and StandardSubModelViewController will accept a GET /Account/DeletePartialView/*id* web request; id will be DataObject.Integer64ID or DataObject.StringID. The standard is to return a partial view named "_AccountDeletePartial". The view should have UI for deleting an account and it should utilize the POST Delete action.

* When the data object is found, a PartialViewResult object is returned by the controller with an account object.
* When the data object is not found, a 404 (Not Found) response will be returned by the controller.
* On any exception thrown, a 500 (Internal Server Error) response will be returned by the controller. The IsDetailsIncludedInNegativeResponse property controls if a ProblemDetails object is returned or not.

### POST Delete
The StandardModelViewController and StandardSubModelViewController will accept a POST /Account/Delete web request; the body must contain an Account object. The standard is to call the IAccountDataLayer.DeleteAsync() method.

* If the data object deletion is successful, a 302 (Redirect) to Index will be returned by the controller or json of the Account object will be returned by the controller. The return action is controlled by the IsCUDActionRedirectedOnSuccess property.
* If a DataObjectUpdateConflictException is thrown, a 409 (Conflict) response will be returned by the controller. The IsDetailsIncludedInNegativeResponse property controls if a ProblemDetails object is returned or not.
* If a DataObjectIDNotFoundException is thrown, a 404 (Not Found) response will be returned by the controller. The IsDetailsIncludedInNegativeResponse property controls if a ProblemDetails object is returned or not.
* If any other exception is thrown, a 500 (Internal Server Error) response will be returned by the controller. The IsDetailsIncludedInNegativeResponse property controls if a ProblemDetails object is returned or not.

### GET Edit View
The StandardModelViewController and StandardSubModelViewController will accept a GET /Account/EditView/*id* web request; id will be DataObject.Integer64ID or DataObject.StringID. The standard is to return a view named "AccountEdit". The view should have UI for editing an account and it should utilize the POST Update action.

* When the data object is found using the id, a ViewResult object is returned by the controller with an account object.
* When the data object is not found using the id, a 404 (Not Found) response will be returned by the controller. The IsDetailsIncludedInNegativeResponse property controls if a ProblemDetails object is returned or not.
* On any exception thrown, a 500 (Internal Server Error) response will be returned by the controller. The IsDetailsIncludedInNegativeResponse property controls if a ProblemDetails object is returned or not.

### GET Edit Partial View
The StandardModelViewController and StandardSubModelViewController will accept a GET /Account/EditPartialView/*id* web request; id will be DataObject.Integer64ID or DataObject.StringID. The standard is to return a partial view named "_AccountEditPartial". The view should have UI for editing an account and it should utilize the POST Update action.

* When the data object is found, a PartialViewResult object is returned by the controller with an account object.
* When the data object is not found, a 404 (Not Found) response will be returned by the controller. The IsDetailsIncludedInNegativeResponse property controls if a ProblemDetails object is returned or not.
* On any exception thrown, a 500 (Internal Server Error) response will be returned by the controller. The IsDetailsIncludedInNegativeResponse property controls if a ProblemDetails object is returned or not.

### POST Update
The StandardModelViewController and StandardSubModelViewController will accept a POST /Account/Update web request; the body must contain an Account object. The standard is to call the IAccountDataLayer.UpdateAsync() method.

* If the data object update is successful, a 302 (Redirect) to Index will be returned by the controller or json of the Account object will be returned by the controller. The return action is controlled by the IsCUDActionRedirectedOnSuccess property.
* If the model state is invalid or a DataObjectValidationException is thrown, a ViewResult or PartialViewResult object is returned by the controller or a 400 (Bad Request) response is returned by the controller with a ValidationProblemDetails object. The return action is controlled by the ValidationFailedAction property.
* If a DataObjectUpdateConflictException is thrown, a 409 (Conflict) response will be returned by the controller. The IsDetailsIncludedInNegativeResponse property controls if a ProblemDetails object is returned or not.
* If a DataObjectIDNotFoundException is thrown, a 404 (Not Found) response will be returned by the controller. The IsDetailsIncludedInNegativeResponse property controls if a ProblemDetails object is returned or not.
* If any other exception is thrown, a 500 (Internal Server Error) response will be returned by the controller. The IsDetailsIncludedInNegativeResponse property controls if a ProblemDetails object is returned or not.

### How to Expand Your Controller
Now, let's say you need to add additional functionality to the controller and you can easy do so by adding a new method to your controller.
```
public AccountController : StandardModelViewController<Account, IAccountDataLayer>
{
  public AccountController(IAccountDataLayer dataLayer, ILogger<AccountController> logger) : base(dataLayer, logger) { }
  
  public async Task<IActionResult> DetailViewAsync(long id)
  {
    try
    {
        base.Logger.LogInformation("Attempting to retrieve the {Type} data object for {ID} for the Detial View.", base.DataObjectTypeName, id);

        var dataObject = await base.DataLayer.GetSingleAsync(obj => obj.Integer64ID == id);

        if (dataObject is null)
        {
            base.Logger.LogError("The {Type} data object for {ID} was not found so the Detail View could not be returned.", base.DataObjectTypeName, id);
            return base.IsDetailsIncludedInNegativeResponse ? NotFound(new NotFoundDetails(title: $"{base.DataObjectTypeName.SpaceCapitalLetters()} Detail View Error - Not Found", detail: $"The {base.DataObjectTypeName.SpaceCapitalLetters()} record was not found; please refresh the page because another user may have deleted it.")) : NotFound();
        }

        base.Logger.LogInformation("The {Type} data object for {ID} for the Detail View was successfully retrieved; returning the view.", base.DataObjectTypeName, id);
        return View($"{base.DataObjectTypeName}Detail", dataObject);
    }
    catch (Exception ex)
    {
        base.Logger.LogError(ex, "Failed to return the Edit View for the {Type} data object for {ID}.", base.DataObjectTypeName, id);
        return IsDetailsIncludedInNegativeResponse ? Problem(title: $"{base.DataObjectTypeName.SpaceCapitalLetters()} Detail View Error", detail: $"Failed to find the {base.DataObjectTypeName.SpaceCapitalLetters()} Detail View because of an error on the server.") : Problem();
    }
  }
}
```
The same can be done when using the StandardSubModelViewController.

### How to Override Base Functionality in Your Controller
Let's say you need to check the amount before the account is created. It's easy because the methods are virtual so you can override them.
```
public AccountController : StandardModelViewController<Account, IAccountDataLayer>
{
  public AccountController(IAccountDataLayer dataLayer, ILogger<AccountController> logger) : base(dataLayer, logger) { }

  public override async Task<IActionResult> CreateAsync([FromBody] Account dataObject)
  {
    if (dataObject.TotalAmount < 100)
    {
      return BadRequest();
    }

    return await base.CreateAsync(dataObject);
  }
}
```
The same can be done when using the StandardSubModelViewController.

### The Sub Controller
The StandardSubModelViewController is designed to return a subset of data objects for an owner data object so transactions for an account. The IndexAsync() uses the default pattern mapping to accept an owner id and return the subset. The owner id will be stored in the ViewBag and you can use it in the MVC pattern when navigating to an add page. The AddViewAsync() and AddPartialViewAsync() also uses the default pattern mapping to accept an owner id. The owner id will be stored in the ViewBag and when using the MVC pattern, it can be stored in a hidden input in the form so the owner id is added to the data object when posted to the Create action. When using the MVC pattern, the CUD actions, on success, will redirect back 
to the Index page and the id will be set to the owner id.

## Web API
Web API is used when there is a client-server split and the client uses the api to remotely request data from the server and to remotely create, delete & update data on the server. You can learn more [here](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/apis).

### How to Create Your Web API Controller
See the JMayer.Data library readme for the Account data object and IAccountDataLayer (database datalayer). The controller utlizes dependency injection to accept an IAccountDataLayer and ILogger interfaces and it interacts with those interfaces when requests are processed.
```
[Route("api/[controller]")]
[ApiController]
public AccountController : StandardCRUDController<Account, IAccountDataLayer>
{
  public AccountController(IAccountDataLayer dataLayer, ILogger<AccountController> logger) : base(dataLayer, logger) { }
}
```
You now have a web API controller for Account data and your ASP.NET Core application will automatically expose that. It will accept GET, POST, PUT & DELETE calls for /api/Account. The same can be done when using the StandardSubCRUDController.

### GET Count
The StandardCRUDController and StandardSubCRUDController will accept a GET api/Account/Count web request. The standard is to call the IAccountDataLayer.CountAsync() method and a web response will be return based on success or failure.
* If querying the count is successful, a 200 (OK) response will be returned by the controller. The count will be returned in the body as an integer.
* If any exception is thrown, a 500 (Internal Server Error) response will be returned by the controller. A ProblemDetails object will be returned in the body as json.

### GET All
The StandardCRUDController and StandardSubCRUDController will accept a GET api/Account/All web request. The standard is to call the IAccountDataLayer.GetAllAsync() method and a web response will be return based on success or failure.

* If querying the data objects is successful, a 200 (OK) response will be returned by the controller. A list of Account objects will be returned in the body as json.
* If any exception is thrown, a 500 (Internal Server Error) response will be returned by the controller. A ProblemDetails object will be returned in the body as json.

The StandardSubCRUDController will accept an owner id parameter and it'll return a subset of data objects using the owner id.

### GET All List View
The StandardCRUDController and StandardSubCRUDController will accept a GET api/Account/All/ListView web request. The standard is to call the IAccountDataLayer.GetAllListViewAsync() method and a web response will be return based on success or failure.

* If querying the data object is successful, a 200 (OK) response will be returned by the controller. A list of ListView objects will be returned in the body as json.
* If any exception is thrown, a 500 (Internal Server Error) response will be returned by the controller. A ProblemDetails object will be returned in the body as json.

The StandardSubCRUDController will accept an owner id parameter and it'll return a subset of data objects using the owner id.

### GET Page
The StandardCRUDController and StandardSubCRUDController will accept a GET api/Account/Page web request; the query string will be used to build the QueryDefinition object. The standard is to call the IAccountDataLayer.GetPageAsync() method and a web response will be return based on success or failure.

* If querying the page is successful, a 200 (OK) response will be returned by the controller. A PagedList object with Account objects will be returned in the body as json.
* If any exception is thrown, a 500 (Internal Server Error) response will be returned by the controller. A ProblemDetails object will be returned in the body as json.

The StandardSubCRUDController will accept an owner id parameter and it'll return a subset of data objects using the owner id.

### GET Page List View
The StandardCRUDController and StandardSubCRUDController will accept a GET api/Account/Page/ListView web request; the query string will be used to build the QueryDefinition object. The standard is to call the IAccountDataLayer.GetPageListViewAsync() method and a web response will be return based on success or failure.

* If querying the page is successful, a 200 (OK) response will be returned by the controller. A PagedList object with ListView objects will be returned in the body as json.
* If any exception is thrown, a 500 (Internal Server Error) response will be returned by the controller. A ProblemDetails object will be returned in the body as json.

The StandardSubCRUDController will accept an owner id parameter and it'll return a subset of data objects using the owner id.

### Get Single
The StandardCRUDController and StandardSubCRUDController will accept a GET api/Account/Single or GET api/Account/Single/*id* web request; id will be DataObject.Integer64ID or DataObject.StringID. The standard is to call the IAccountDataLayer.GetSingleAsync() method and a web response will be return based on success or failure.

* If querying the data object is successful, a 200 (OK) response will be returned by the controller. If found, an Account object will be returned in the body as json else nothing is returned.
* If any exception is thrown, a 500 (Internal Server Error) response will be returned by the controller. A ProblemDetails object will be returned in the body as json.

### POST
The StandardCRUDController and StandardSubCRUDController will accept a POST api/Account web request; the body must contain an Account object as json. The standard is to call the IAccountDataLayer.CreateAsync() method and a web response will be return based on success or failure.

* If the data object creation is successful, a 200 (OK) response will be returned by the controller. The Account object will be returned in the body as json.
* If a DataObjectValidationException is thrown, a 400 (Bad Request) response will be returned by the controller. A ValidationProblemDetails object will be returned in the body as json.
* If any other exception is thrown, a 500 (Internal Server Error) response will be returned by the controller. A ProblemDetails object will be returned in the body as json.

### DELETE
The StandardCRUDController and StandardSubCRUDController will accept a DELETE api/Account/*id* web request; id will be DataObject.Integer64ID or DataObject.StringID. The standard is to call the IAccountDataLayer.DeleteAsync() method and a web response will be return based on success or failure.

* If the data object deletion is successful, a 200 (OK) response will be returned by the controller. No body.
* If a DataObjectDeleteConflictException is thrown, a 409 (Conflict) response will be returned by the controller. A ProblemDetails object will be returned in the body as json.
* If any other exception is thrown, a 500 (Internal Server Error) response will be returned by the controller. A ProblemDetails object will be returned in the body as json.

### PUT
The StandardCRUDController and StandardSubCRUDController will accept a PUT api/Account web request; the body must contain an Account object as json. The standard is to call the IAccountDataLayer.UpdateAsync() method and a web response will be return based on success or failure.

* If the data object update is successful, a 200 (OK) response will be returned by the controller. The Account object will be returned in the body as json.
* If a DataObjectValidationException is thrown, a 400 (Bad Request) response will be returned by the controller. A ValidationProblemDetails object will be returned in the body as json.
* If a DataObjectDeleteConflictException is thrown, a 409 (Conflict) response will be returned by the controller. A ProblemDetails object will be returned in the body as json.
* If a DataObjectIDNotFoundException is thrown, a 404 (Not Found) response will be returned by the controller. A ProblemDetails object will be returned in the body as json.
* If any other exception is thrown, a 500 (Internal Server Error) response will be returned by the controller. A ProblemDetails object will be returned in the body as json.

### How to Expand Your Controller
Now, let's say you need to add additional functionality to the controller and you can easy do so by adding a new method to your controller.
```
public AccountController : StandardCRUDController<Account, IAccountDataLayer>
{
  public AccountController(IAccountDataLayer dataLayer, ILogger<AccountController> logger) : base(dataLayer, logger) { }

  [HttpGet("All/Accounts")]
  public async Task<IActionResult> GetSavingAccountsAsync()
  {
    try
    {
      base.Logger.LogInformation("Attempting to retrieve all the saving {Type} data objects.", base.DataObjectTypeName);
      
      var accounts = await base.DataLayer.GetSavingAccountsAsync();
      
      base.Logger.LogInformation("All the saving {Type} data objects were successfully retrieved.", base.DataObjectTypeName);
      
      return Ok(accounts);
    }
    catch (Exception ex)
    {
      base.Logger.LogError(ex, "Failed to return all the saving {Type} data objects.", base.DataObjectTypeName);
      return Problem(title: $"{base.DataObjectTypeName.SpaceCapitalLetters()} Get Saving Accounts Error", detail: $"Failed to return all the saving {base.DataObjectTypeName.SpaceCapitalLetters()} records because of an error on the server.");
    }
  }
}
```
The same can be done when using the StandardSubCRUDController.

### How to Override Base Functionality in Your Controller
Let's say you need to check the amount before the account is created. It's easy because the methods are virtual so you can override them.
```
[Route("api/[controller]")]
[ApiController]
public AccountController : StandardCRUDController<Account, IAccountDataLayer>
{
  public AccountController(IAccountDataLayer dataLayer, ILogger<AccountController> logger) : base(dataLayer, logger) { }

  public override async Task<IActionResult> CreateAsync([FromBody] Account dataObject)
  {
    if (dataObject.TotalAmount < 100)
    {
      return BadRequest();
    }

    return await base.CreateAsync(dataObject);
  }
}
```
The same can be done when using the StandardSubCRUDController.

# v9.0.0 Change Log
---
* Updated to .NET9.
* **Breaking Change:** Merged the UserEditableController into the StandardCRUDController and moved it into the Api namespace.
* **Breaking Change:** Renamed the SubUserEditableController to the StandardSubCRUDController and moved it into the Api namespace.
* **Breaking Change:** Removed ValidateAsync() from the StandardCRUDController.
* Added a title and detail when Api controllers return a ProblemDetails.
* **Breaking Change:** Switch Api controllers to return a ValidationProblemDetails when a validation issue occurs.
* Added a title when the MVC controllers return a ProblemDetails.
* **Breaking Change:** Switched MVC controllers from returning a dynamic object for non-500 errors to a ProblemDetails.
* Additional logging and changes to existing logging messages for all controllers.
---
* [ASP.NET Core MVC with Syncfusion Example Project](https://github.com/jmayer913/JMayer-Example-ASPSyncfusionMVC)
* [ASP.NET Core / React Example Project](https://github.com/jmayer913/JMayer-Example-ASPReact)
* [Blazor WebAssembly Example Project](https://github.com/jmayer913/JMayer-Example-WebAssemblyBlazor)
---
