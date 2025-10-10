using JMayer.Data.Data;
using JMayer.Data.Database.DataLayer;
using JMayer.Web.Mvc.Extension;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;

#warning Conflict accepts the model state and I've been wondering if I should try to utilize it but I'm not 100% sure how that will work because the error requires a key which is a property name and the id might not be visible.
#warning This makes me think I should just redirect. By default, it'll redirect to an action local to the controller but if the user needs to redirect elsewhere I have properties for the name and controller.
#warning Ok. I think I have my solution for Conflict.
#warning I have the Conflict accepting an Id which makes me feel like I should query the record and return it as a model.

namespace JMayer.Web.Mvc.Controller;

/// <summary>
/// The class manages HTTP view and action requests associated with a data object and a data layer.
/// <br/>
/// <br/>
/// Properties dictate if the controller uses the MVC pattern (redirects or returning views with the model state) or 
/// Ajax pattern (returning json to be processed by javascript). The default functionality is the MVC pattern and you 
/// can switch what you need to the Ajax pattern in the constructor of your child class.
/// <br/>
/// <br/>
/// When a model is not found, a 404 will be returned. With the MVC pattern, 404 handling will need to setup in the middleware
/// so the user is displayed a meaningful page else the browser will display whatever it displays for a 404.
/// </summary>
/// <typeparam name="T">Must be a DataObject since the data layer requires this.</typeparam>
/// <typeparam name="U">Must be an IStandardCRUDDataLayer so the controller can interact with the collection/table associated with it.</typeparam>
public class StandardModelViewController<T, U> : Microsoft.AspNetCore.Mvc.Controller
    where T : DataObject
    where U : IStandardCRUDDataLayer<T>
{
    /// <summary>
    /// The property gets/sets the name of the conflict action.
    /// </summary>
    /// <remarks>
    /// The default is Conflict but if you need to change it then in the constructor of your child class,
    /// set this property to the name you need.
    /// </remarks>
    public string ConflictActionName { get; init; } = "ConflictView";

    /// <summary>
    /// The property gets/sets the name of the conflict controller.
    /// </summary>
    /// <remarks>
    /// The default is null but if you need to change it then in the constructor of your child class,
    /// set this property to the name you need.
    /// </remarks>
    public string? ConflictControllerName { get; init; }

    /// <summary>
    /// The data layer the controller will interact with.
    /// </summary>
    protected IStandardCRUDDataLayer<T> DataLayer { get; private init; }

    /// <summary>
    /// The name of the data object.
    /// </summary>
    protected string DataObjectTypeName { get; private init; } = typeof(T).Name;

    /// <summary>
    /// The property gets/sets the name of the error action.
    /// </summary>
    /// <remarks>
    /// The default is Error but if you need to change it then in the constructor of your child class,
    /// set this property to the name you need.
    /// </remarks>
    public string ErrorActionName { get; init; } = "Error";

    /// <summary>
    /// The property gets/sets the name of the error controller.
    /// </summary>
    /// <remarks>
    /// The default is Home but if you need to change it then in the constructor of your child class,
    /// set this property to the name you need.
    /// </remarks>
    public string ErrorControllerName { get; init; } = "Home";

    /// <summary>
    /// The property gets/sets if the controller redirects on a data conflict.
    /// </summary>
    /// <remarks>
    /// The default functionality is to do a redirect but if you need to return a 409 conflict with a user message
    /// then in the constructor of your child class, set this property to false.
    /// <br/>
    /// <br/>
    /// When data conflict occurs, the controller will redirect to the this controller for the ConflictView action with the id
    /// and a user message in the route. The id will be stored in the ViewBag.Id and the user message will be stored in the ViewBag.UserMessage;
    /// you can use them if you want. If a different controller and/or action is needed, set the ConflictActionName and ConflictControllerName 
    /// properties to what you need in the constructor of your child class.
    /// </remarks>
    public bool IsActionRedirectedOnConflict { get; init; } = true;

    /// <summary>
    /// The property gets/sets if the controller redirects on error.
    /// </summary>
    /// <remarks>
    /// The default functionality is to do a redirect but if you need to return a 500 internal server error with problem details 
    /// then in the constructor of your child class, set this property to false.
    /// <br/>
    /// <br/>
    /// When a general exception is thrown in an action, the controller will redirect to the Home controller for the Error action
    /// with a user message in the route. This allows you to accept the message as a string and display it if need be. If a different 
    /// controller and/or action is needed, set the ErrorActionName and ErrorControllerName properties to what you need in the constructor 
    /// of your child class.
    /// </remarks>
    public bool IsActionRedirectedOnError { get; init; } = true;

    /// <summary>
    /// The property gets/sets if the controller redirects a CUD (Create, Delete or Update) action on success to the Index view.
    /// </summary>
    /// <remarks>
    /// The default functionality is to do a redirect but if you need to return json then in the constructor of your child class, 
    /// set this property to false.
    /// </remarks>
    public bool IsCUDActionRedirectedOnSuccess { get; init; } = true;

    /// <summary>
    /// The logger the controller will interact with.
    /// </summary>
    protected ILogger Logger { get; private init; }

    /// <summary>
    /// The property gets/sets what the controller will do when the model fails server-side validation.
    /// </summary>
    /// <remarks>
    /// The default functionality is to return the view associated with the action but if you need a different action (partial view or
    /// 400 bad request with a model state dictionary) then in the constructor of your child class, set this property the action you need.
    /// </remarks>
    public ValidationFailedAction ValidationFailedAction { get; init; }

    /// <summary>
    /// The dependency injection constructor.
    /// </summary>
    /// <param name="dataLayer">The data layer the controller will interact with.</param>
    /// <param name="logger">The logger the controller will interact with.</param>
    /// <exception cref="ArgumentNullException">Thrown if the dataLayer or logger parameter is null.</exception>
    public StandardModelViewController(U dataLayer, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(dataLayer);
        ArgumentNullException.ThrowIfNull(logger);

        DataLayer = dataLayer;
        Logger = logger;
    }

    /// <summary>
    /// The method returns the add partial view.
    /// </summary>
    /// <returns>The add partial view or a negative status code.</returns>
    public virtual async Task<IActionResult> AddPartialViewAsync()
    {
        try
        {
            PartialViewResult partialViewResult = PartialView($"_{DataObjectTypeName}AddPartial");
            return await Task.FromResult(partialViewResult);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Add Partial View for the {Type}.", DataObjectTypeName);

            if (IsActionRedirectedOnError)
            {
                return RedirectToAction(ErrorActionName, ErrorControllerName, new { UserMessage = $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Add Partial View because of an error on the server" });
            }
            else
            {
                return Problem(detail: $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Add Partial View because of an error on the server.");
            }
        }
    }

    /// <summary>
    /// The method returns the add view.
    /// </summary>
    /// <returns>The add view or a negative status code.</returns>
    public virtual async Task<IActionResult> AddViewAsync()
    {
        try
        {
            ViewResult viewResult = View($"{DataObjectTypeName}Add");
            return await Task.FromResult(viewResult);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Add View for the {Type}.", DataObjectTypeName);

            if (IsActionRedirectedOnError)
            {
                return RedirectToAction(ErrorActionName, ErrorControllerName, new { UserMessage = $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Add View because of an error on the server." });
            }
            else
            {
                return Problem(detail: $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Add View because of an error on the server.");
            }
        }
    }

    /// <summary>
    /// The method creates a data object using the data layer.
    /// </summary>
    /// <param name="dataObject">The data object.</param>
    /// <returns>The created data object or a negative status code.</returns>
    [HttpPost]
    public virtual async Task<IActionResult> CreateAsync(T dataObject)
    {
        try
        {
            if (ModelState.IsValid)
            {
                await DataLayer.CreateAsync(dataObject);
                Logger.LogInformation("The {Type} was successfully created.", DataObjectTypeName);

                if (IsCUDActionRedirectedOnSuccess)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return Json(dataObject);
                }
            }
            else
            {
                Logger.LogWarning("Failed to create the {Type} because of a model validation error.", DataObjectTypeName);
                return ValidationFailedAction switch
                {
                    ValidationFailedAction.ReturnView => View($"{DataObjectTypeName}Add", dataObject),
                    ValidationFailedAction.ReturnPartialView => new PartialViewResult()
                    {
                        ViewData = new ViewDataDictionary<T>(ViewData, dataObject),
                        ViewName = $"_{DataObjectTypeName}AddPartial",
                    },
                    _ => ValidationProblem(ModelState)
                };
            }
        }
        catch (DataObjectValidationException ex)
        {
            Logger.LogWarning(ex, "Failed to create the {Type} because of a server-side validation error.", DataObjectTypeName);
            ex.CopyToModelState(ModelState);
            return ValidationFailedAction switch
            {
                ValidationFailedAction.ReturnView => View($"{DataObjectTypeName}Add", dataObject),
                ValidationFailedAction.ReturnPartialView => new PartialViewResult()
                {
                    ViewData = new ViewDataDictionary<T>(ViewData, dataObject),
                    ViewName = $"_{DataObjectTypeName}AddPartial",
                },
                _ => ValidationProblem(ModelState)
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create the {Type}.", DataObjectTypeName);

            if (IsActionRedirectedOnError)
            {
                return RedirectToAction(ErrorActionName, ErrorControllerName, new { UserMessage = $"Failed to create the {DataObjectTypeName.SpaceCamelCase()} record because of an error on the server." });
            }
            else
            {
                return Problem(detail: $"Failed to create the {DataObjectTypeName.SpaceCamelCase()} record because of an error on the server.");
            }
        }
    }

    /// <summary>
    /// The method returns the conflict partial view.
    /// </summary>
    /// <param name="id">Optionally, the id that had a conflict; stored in ViewBag.Id.</param>
    /// <param name="userMessage">Optionally, a user message; stored in ViewBag.UserMessage.</param>
    /// <returns>The view.</returns>
    [HttpGet("[controller]/ConflictPartialView/{id:long}")]
    public virtual async Task<IActionResult> ConflictPartialViewAsync(long? id, [FromQuery] string? userMessage)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.Integer64ID == id);

            if (dataObject is null)
            {
                return NotFound(new { UserMessage = $"The {DataObjectTypeName.SpaceCamelCase()} record was not found; please refresh the page because another user may have deleted it." });
            }

            ViewBag.UserMessage = userMessage;

            IActionResult actionResult = new PartialViewResult()
            {
                ViewData = new ViewDataDictionary<T>(ViewData, dataObject),
                ViewName = $"_{DataObjectTypeName}ConflictPartial",
            };
            return await Task.FromResult(actionResult);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Conflict Partial View for the {Type}.", DataObjectTypeName);

            if (IsActionRedirectedOnError)
            {
                return RedirectToAction(ErrorActionName, ErrorControllerName, new { UserMessage = $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Conflict Partial View because of an error on the server." });
            }
            else
            {
                return Problem(detail: $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Conflict Partial View because of an error on the server.");
            }
        }
    }

    /// <summary>
    /// The method returns the conflict partial view.
    /// </summary>
    /// <param name="id">Optionally, the id that had a conflict; stored in ViewBag.Id.</param>
    /// <param name="userMessage">Optionally, a user message; stored in ViewBag.UserMessage.</param>
    /// <returns>The view.</returns>
    [HttpGet("[controller]/ConflictPartialView/{id}")]
    public virtual async Task<IActionResult> ConflictPartialViewAsync(string? id, [FromQuery] string? userMessage)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.StringID == id);

            if (dataObject is null)
            {
                return NotFound(new { UserMessage = $"The {DataObjectTypeName.SpaceCamelCase()} record was not found; please refresh the page because another user may have deleted it." });
            }

            ViewBag.UserMessage = userMessage;

            IActionResult actionResult = new PartialViewResult()
            {
                ViewData = new ViewDataDictionary<T>(ViewData, dataObject),
                ViewName = $"_{DataObjectTypeName}ConflictPartial",
            };
            return await Task.FromResult(actionResult);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Conflict Partial View for the {Type}.", DataObjectTypeName);

            if (IsActionRedirectedOnError)
            {
                return RedirectToAction(ErrorActionName, ErrorControllerName, new { UserMessage = $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Conflict Partial View because of an error on the server." });
            }
            else
            {
                return Problem(detail: $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Conflict Partial View because of an error on the server.");
            }
        }
    }

    /// <summary>
    /// The method returns the conflict view.
    /// </summary>
    /// <param name="id">Optionally, the id that had a conflict; stored in ViewBag.Id.</param>
    /// <param name="userMessage">Optionally, a user message; stored in ViewBag.UserMessage.</param>
    /// <returns>The view.</returns>
    [HttpGet("[controller]/ConflictView/{id:long}")]
    public virtual async Task<IActionResult> ConflictViewAsync(long? id, [FromQuery] string? userMessage)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.Integer64ID == id);

            if (dataObject is null)
            {
                return NotFound(new { UserMessage = $"The {DataObjectTypeName.SpaceCamelCase()} record was not found; please refresh the page because another user may have deleted it." });
            }

            ViewBag.UserMessage = userMessage;
            IActionResult actionResult = View($"{DataObjectTypeName}Conflict", dataObject);
            return await Task.FromResult(actionResult);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Conflict View for the {Type}.", DataObjectTypeName);

            if (IsActionRedirectedOnError)
            {
                return RedirectToAction(ErrorActionName, ErrorControllerName, new { UserMessage = $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Conflict View because of an error on the server." });
            }
            else
            {
                return Problem(detail: $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Conflict View because of an error on the server.");
            }
        }
    }

    /// <summary>
    /// The method returns the conflict view.
    /// </summary>
    /// <param name="id">Optionally, the id that had a conflict; stored in ViewBag.Id.</param>
    /// <param name="userMessage">Optionally, a user message; stored in ViewBag.UserMessage.</param>
    /// <returns>The view.</returns>
    [HttpGet("[controller]/ConflictView/{id}")]
    public virtual async Task<IActionResult> ConflictViewAsync(string? id, [FromQuery] string? userMessage)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.StringID == id);

            if (dataObject is null)
            {
                return NotFound(new { UserMessage = $"The {DataObjectTypeName.SpaceCamelCase()} record was not found; please refresh the page because another user may have deleted it." });
            }

            ViewBag.UserMessage = userMessage;
            IActionResult actionResult = View($"{DataObjectTypeName}Conflict");
            return await Task.FromResult(actionResult);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Conflict View for the {Type}.", DataObjectTypeName);

            if (IsActionRedirectedOnError)
            {
                return RedirectToAction(ErrorActionName, ErrorControllerName, new { UserMessage = $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Conflict View because of an error on the server." });
            }
            else
            {
                return Problem(detail: $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Conflict View because of an error on the server.");
            }
        }
    }

    /// <summary>
    /// The method deletes a data object using the data layer.
    /// </summary>
    /// <param name="id">The id to delete.</param>
    /// <returns>The deleted data object or a negative status code.</returns>
    [HttpPost("[controller]/Delete/{id:long}")]
    public virtual async Task<IActionResult> DeleteAsync(long id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.Integer64ID == id);

            if (dataObject is null)
            {
                Logger.LogWarning("The {ID} for the {Type} was not found so no delete occurred.", id.ToString(), DataObjectTypeName);
                return NotFound(new { UserMessage = $"The {DataObjectTypeName.SpaceCamelCase()} record was not found; please refresh the page because another user may have deleted it." });
            }
            else
            {
                await DataLayer.DeleteAsync(dataObject);
                Logger.LogInformation("The {ID} for the {Type} was successfully deleted.", id.ToString(), DataObjectTypeName);

                if (IsCUDActionRedirectedOnSuccess)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return Json(dataObject);
                }
            }
        }
        catch (DataObjectDeleteConflictException ex)
        {
            Logger.LogError(ex, "Failed to delete the {ID} {Type} because of a data conflict.", id.ToString(), DataObjectTypeName);

            if (IsActionRedirectedOnConflict)
            {
                return RedirectToAction(ConflictActionName, ConflictControllerName, new { UserMessage = $"The {DataObjectTypeName.SpaceCamelCase()} record has a dependency that prevents it from being deleted; the dependency needs to be deleted first." });
            }
            else
            {
                return Conflict(new { UserMessage = $"The {DataObjectTypeName.SpaceCamelCase()} record has a dependency that prevents it from being deleted; the dependency needs to be deleted first." });
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete the {ID} {Type}.", id.ToString(), DataObjectTypeName);

            if (IsActionRedirectedOnError)
            {
                return RedirectToAction(ErrorActionName, ErrorControllerName, new { UserMessage = $"Failed to delete the {DataObjectTypeName.SpaceCamelCase()} record because of an error on the server." });
            }
            else
            {
                return Problem(detail: $"Failed to delete the {DataObjectTypeName.SpaceCamelCase()} record because of an error on the server.");
            }
        }
    }

    /// <summary>
    /// The method deletes a data object using the data layer.
    /// </summary>
    /// <param name="id">The id to delete.</param>
    /// <returns>The deleted data object or a negative status code.</returns>
    [HttpPost("[controller]/Delete/{id}")]
    public virtual async Task<IActionResult> DeleteAsync(string id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.StringID == id);

            if (dataObject is null)
            {
                Logger.LogWarning("The {ID} for the {Type} was not found so no delete occurred.", id, DataObjectTypeName);
                return NotFound(new { UserMessage = $"The {DataObjectTypeName.SpaceCamelCase()} record was not found; please refresh the page because another user may have deleted it." });
            }
            else
            {
                await DataLayer.DeleteAsync(dataObject);
                Logger.LogInformation("The {ID} for the {Type} was successfully deleted.", id, DataObjectTypeName);

                if (IsCUDActionRedirectedOnSuccess)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return Json(dataObject);
                }
            }
        }
        catch (DataObjectDeleteConflictException ex)
        {
            Logger.LogError(ex, "Failed to delete the {ID} {Type} because of a data conflict.", id.ToString(), DataObjectTypeName);

            if (IsActionRedirectedOnConflict)
            {
                return RedirectToAction(ConflictActionName, ConflictControllerName, new { UserMessage = $"The {DataObjectTypeName.SpaceCamelCase()} record has a dependency that prevents it from being deleted; the dependency needs to be deleted first." });
            }
            else
            {
                return Conflict(new { UserMessage = $"The {DataObjectTypeName.SpaceCamelCase()} record has a dependency that prevents it from being deleted; the dependency needs to be deleted first." });
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete the {ID} {Type}.", id.ToString(), DataObjectTypeName);

            if (IsActionRedirectedOnError)
            {
                return RedirectToAction(ErrorActionName, ErrorControllerName, new { UserMessage = $"Failed to delete the {DataObjectTypeName.SpaceCamelCase()} record because of an error on the server." });
            }
            else
            {
                return Problem(detail: $"Failed to delete the {DataObjectTypeName.SpaceCamelCase()} record because of an error on the server.");
            }
        }
    }

    /// <summary>
    /// The method returns the delete partial view.
    /// </summary>
    /// <param name="id">The id for the record.</param>
    /// <returns>The delete partial view or a negative status code.</returns>
    [HttpGet("[controller]/DeletePartialView/{id:long}")]
    public virtual async Task<IActionResult> DeletePartialViewAsync(long id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.Integer64ID == id);

            if (dataObject is null)
            {
                Logger.LogError("Failed to find the {ID} when fetching the Delete Partial View for the {Type}.", id, DataObjectTypeName);
                return NotFound(new { UserMessage = $"The {DataObjectTypeName.SpaceCamelCase()} record was not found; please refresh the page because another user may have deleted it." });
            }

            return new PartialViewResult()
            {
                ViewData = new ViewDataDictionary<T>(ViewData, dataObject),
                ViewName = $"_{DataObjectTypeName}DeletePartial",
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Delete Partial View for the {Type}.", DataObjectTypeName);

            if (IsActionRedirectedOnError)
            {
                return RedirectToAction(ErrorActionName, ErrorControllerName, new { UserMessage = $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Delete View because of an error on the server." });
            }
            else
            {
                return Problem(detail: $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Delete View because of an error on the server.");
            }
        }
    }

    /// <summary>
    /// The method returns the delete partial view.
    /// </summary>
    /// <param name="id">The id for the record.</param>
    /// <returns>The delete partial view or a negative status code.</returns>
    [HttpGet("[controller]/DeletePartialView/{id}")]
    public virtual async Task<IActionResult> DeletePartialViewAsync(string id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.StringID == id);

            if (dataObject is null)
            {
                Logger.LogError("Failed to find the {ID} when fetching the Delete Partial View for the {Type}.", id, DataObjectTypeName);
                return NotFound(new { UserMessage = $"The {DataObjectTypeName.SpaceCamelCase()} record was not found; please refresh the page because another user may have deleted it." });
            }

            return new PartialViewResult()
            {
                ViewData = new ViewDataDictionary<T>(ViewData, dataObject),
                ViewName = $"_{DataObjectTypeName}DeletePartial",
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Delete Partial View for the {Type}.", DataObjectTypeName);

            if (IsActionRedirectedOnError)
            {
                return RedirectToAction(ErrorActionName, ErrorControllerName, new { UserMessage = $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Delete View because of an error on the server." });
            }
            else
            {
                return Problem(detail: $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Delete View because of an error on the server.");
            }
        }
    }

    /// <summary>
    /// The method returns the delete view.
    /// </summary>
    /// <param name="id">The id for the record.</param>
    /// <returns>The delete view or a negative status code.</returns>
    [HttpGet("[controller]/DeleteView/{id:long}")]
    public virtual async Task<IActionResult> DeleteViewAsync(long id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.Integer64ID == id);

            if (dataObject is null)
            {
                Logger.LogError("Failed to find the {ID} when fetching the Delete View for the {Type}.", id, DataObjectTypeName);
                return NotFound(new { UserMessage = $"The {DataObjectTypeName.SpaceCamelCase()} record was not found; please refresh the page because another user may have deleted it." });
            }

            return View($"{DataObjectTypeName}Delete", dataObject);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Delete View for the {Type} using the {ID} ID.", DataObjectTypeName, id);

            if (IsActionRedirectedOnError)
            {
                return RedirectToAction(ErrorActionName, ErrorControllerName, new { UserMessage = $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Delete View because of an error on the server." });
            }
            else
            {
                return Problem(detail: $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Delete View because of an error on the server.");
            }
        }
    }

    /// <summary>
    /// The method returns the delete view.
    /// </summary>
    /// <param name="id">The id for the record.</param>
    /// <returns>The delete view or a negative status code.</returns>
    [HttpGet("[controller]/DeleteView/{id}")]
    public virtual async Task<IActionResult> DeleteViewAsync(string id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.StringID == id);

            if (dataObject == null)
            {
                Logger.LogError("Failed to find the {ID} when fetching the Delete View for the {Type}.", id, DataObjectTypeName);
                return NotFound(new { UserMessage = $"The {DataObjectTypeName.SpaceCamelCase()} record was not found; please refresh the page because another user may have deleted it." });
            }

            return View($"{DataObjectTypeName}Delete", dataObject);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Delete View for the {Type} using the {ID} ID.", DataObjectTypeName, id);

            if (IsActionRedirectedOnError)
            {
                return RedirectToAction(ErrorActionName, ErrorControllerName, new { UserMessage = $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Delete View because of an error on the server." });
            }
            else
            {
                return Problem(detail: $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Delete View because of an error on the server.");
            }
        }
    }

    /// <summary>
    /// The method returns the edit partial view.
    /// </summary>
    /// <param name="id">The id for the record.</param>
    /// <returns>The edit partial view or a negative status code.</returns>
    [HttpGet("[controller]/EditPartialView/{id:long}")]
    public virtual async Task<IActionResult> EditPartialViewAsync(long id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.Integer64ID == id);

            if (dataObject is null)
            {
                Logger.LogError("Failed to find the {ID} when fetching the Edit Partial View for the {Type}.", id, DataObjectTypeName);
                return NotFound(new { UserMessage = $"The {DataObjectTypeName.SpaceCamelCase()} record was not found; please refresh the page because another user may have deleted it." });
            }

            return new PartialViewResult()
            {
                ViewData = new ViewDataDictionary<T>(ViewData, dataObject),
                ViewName = $"_{DataObjectTypeName}EditPartial",
            };
        }
        catch (Exception ex) 
        {
            Logger.LogError(ex, "Failed to return the Edit Partial View for the {Type}.", DataObjectTypeName);

            if (IsActionRedirectedOnError)
            {
                return RedirectToAction(ErrorActionName, ErrorControllerName, new { UserMessage = $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Edit View because of an error on the server." });
            }
            else
            {
                return Problem(detail: $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Edit View because of an error on the server.");
            }
        }
    }

    /// <summary>
    /// The method returns the edit partial view.
    /// </summary>
    /// <param name="id">The id for the record.</param>
    /// <returns>The edit partial view or a negative status code.</returns>
    [HttpGet("[controller]/EditPartialView/{id}")]
    public virtual async Task<IActionResult> EditPartialViewAsync(string id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.StringID == id);

            if (dataObject is null)
            {
                Logger.LogError("Failed to find the {ID} when fetching the Edit Partial View for the {Type}.", id, DataObjectTypeName);
                return NotFound(new { UserMessage = $"The {DataObjectTypeName.SpaceCamelCase()} record was not found; please refresh the page because another user may have deleted it." });
            }

            return new PartialViewResult()
            {
                ViewData = new ViewDataDictionary<T>(ViewData, dataObject),
                ViewName = $"_{DataObjectTypeName}EditPartial",
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Edit Partial View for the {Type}.", DataObjectTypeName);

            if (IsActionRedirectedOnError)
            {
                return RedirectToAction(ErrorActionName, ErrorControllerName, new { UserMessage = $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Edit View because of an error on the server." });
            }
            else
            {
                return Problem(detail: $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Edit View because of an error on the server.");
            }
        }
    }

    /// <summary>
    /// The method returns the edit view.
    /// </summary>
    /// <param name="id">The id for the record.</param>
    /// <returns>The edit view or a negative status code.</returns>
    [HttpGet("[controller]/EditView/{id:long}")]
    public virtual async Task<IActionResult> EditViewAsync(long id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.Integer64ID == id);

            if (dataObject is null)
            {
                Logger.LogError("Failed to find the {ID} when fetching the Edit View for the {Type}.", id, DataObjectTypeName);
                return NotFound(new { UserMessage = $"The {DataObjectTypeName.SpaceCamelCase()} record was not found; please refresh the page because another user may have deleted it." });
            }

            return View($"{DataObjectTypeName}Edit", dataObject);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Edit View for the {Type} using the {ID} ID.", DataObjectTypeName, id);

            if (IsActionRedirectedOnError)
            {
                return RedirectToAction(ErrorActionName, ErrorControllerName, new { UserMessage = $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Edit View because of an error on the server." });
            }
            else
            {
                return Problem(detail: $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Edit View because of an error on the server.");
            }
        }
    }

    /// <summary>
    /// The method returns the edit view.
    /// </summary>
    /// <param name="id">The id for the record.</param>
    /// <returns>The edit view or a negative status code.</returns>
    [HttpGet("[controller]/EditView/{id}")]
    public virtual async Task<IActionResult> EditViewAsync(string id)
    {
        try
        {
            T? dataObject = await DataLayer.GetSingleAsync(obj => obj.StringID == id);

            if (dataObject == null)
            {
                Logger.LogError("Failed to find the {ID} when fetching the Edit View for the {Type}.", id, DataObjectTypeName);
                return NotFound(new { UserMessage = $"The {DataObjectTypeName.SpaceCamelCase()} record was not found; please refresh the page because another user may have deleted it." });
            }

            return View($"{DataObjectTypeName}Edit", dataObject);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Edit View for the {Type} using the {ID} ID.", DataObjectTypeName, id);

            if (IsActionRedirectedOnError)
            {
                return RedirectToAction(ErrorActionName, ErrorControllerName, new { UserMessage = $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Edit View because of an error on the server." });
            }
            else
            {
                return Problem(detail: $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Edit View because of an error on the server.");
            }
        }
    }

    /// <summary>
    /// The method returns the index view.
    /// </summary>
    /// <returns>The index view or a negative status code.</returns>
    public virtual async Task<IActionResult> IndexAsync()
    {
        try
        {
            List<T>? dataObjects = await DataLayer.GetAllAsync();
            return View($"{DataObjectTypeName}{nameof(Index)}", dataObjects);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to return the Index View for the {Type}.", DataObjectTypeName);

            if (IsActionRedirectedOnError)
            {
                return RedirectToAction(ErrorActionName, ErrorControllerName, new { UserMessage = $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Index View because of an error on the server." });
            }
            else
            {
                return Problem(detail: $"Failed to find the {DataObjectTypeName.SpaceCamelCase()} Index View because of an error on the server.");
            }
        }
    }

    /// <summary>
    /// The method updates a data object using the data layer.
    /// </summary>
    /// <param name="dataObject">The data object.</param>
    /// <returns>The updated data object or a negative status code.</returns>
    [HttpPost]
    public virtual async Task<IActionResult> UpdateAsync(T dataObject)
    {
        string id = string.Empty;

        try
        {
            id = string.IsNullOrEmpty(dataObject.StringID) ? dataObject.Integer64ID.ToString() : dataObject.StringID;

            if (ModelState.IsValid)
            {
                dataObject = await DataLayer.UpdateAsync(dataObject);
                Logger.LogInformation("The {ID} for the {Type} was successfully updated.", id, DataObjectTypeName);

                if (IsCUDActionRedirectedOnSuccess)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return Json(dataObject);
                }
            }
            else
            {
                Logger.LogWarning("Failed to update the {ID} {Type} because of a model validation error.", id, DataObjectTypeName);
                return ValidationFailedAction switch
                {
                    ValidationFailedAction.ReturnView => View($"{DataObjectTypeName}Edit", dataObject),
                    ValidationFailedAction.ReturnPartialView => new PartialViewResult()
                    {
                        ViewData = new ViewDataDictionary<T>(ViewData, dataObject),
                        ViewName = $"_{DataObjectTypeName}EditPartial",
                    },
                    _ => ValidationProblem(ModelState)
                };
            }
        }
        catch (DataObjectUpdateConflictException ex)
        {
            Logger.LogWarning(ex, "Failed to update {ID} {Type} because the data was considered old.", id, DataObjectTypeName);

            if (IsActionRedirectedOnConflict)
            {
                return RedirectToAction(ConflictActionName, ConflictControllerName, new { UserMessage = "The submitted data was detected to be out of date; please refresh the page and try again." });
            }
            else
            {
                return Conflict(new { UserMessage = "The submitted data was detected to be out of date; please refresh the page and try again." });
            }
        }
        catch (DataObjectValidationException ex)
        {
            Logger.LogWarning(ex, "Failed to update the {ID} {Type} because of a server-side validation error.", id, DataObjectTypeName);
            ex.CopyToModelState(ModelState);
            return ValidationFailedAction switch
            {
                ValidationFailedAction.ReturnView => View($"{DataObjectTypeName}Edit", dataObject),
                ValidationFailedAction.ReturnPartialView => new PartialViewResult()
                {
                    ViewData = new ViewDataDictionary<T>(ViewData, dataObject),
                    ViewName = $"_{DataObjectTypeName}EditPartial",
                },
                _ => ValidationProblem(ModelState)
            };
        }
        catch (IDNotFoundException ex)
        {
            Logger.LogWarning(ex, "Failed to update the {ID} {Type} because it was not found.", id, DataObjectTypeName);
            return NotFound(new { UserMessage = $"The {DataObjectTypeName.SpaceCamelCase()} record was not found; please refresh the page because another user may have deleted it." });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to update the {Type} for {ID}.", DataObjectTypeName, id);

            if (IsActionRedirectedOnError)
            {
                return RedirectToAction(ErrorActionName, ErrorControllerName, new { UserMessage = $"Failed to update the {DataObjectTypeName.SpaceCamelCase()} record because of an error on the server." });
            }
            else
            {
                return Problem(detail: $"Failed to update the {DataObjectTypeName.SpaceCamelCase()} record because of an error on the server.");
            }
        }
    }
}
