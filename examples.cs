
///Example N1

//noncompliant
public class UserController : Controller
{
    public ActionResult UserDetails(int id)
    {
        
        User newUser = GetUserById(id);

        return View(newUser);
    }
}

//compliant
[RoutePrefix("users")]
public class UserController : Controller
{
    [Route("details/{id}")]
    public ActionResult UserDetails(int id)
    {
        // Logic to retrieve user details based on the provided ID
        User user = GetUserById(id);

        return View(user);
    }
}

//The Route Prefix attribute is used at the controller level to define a prefix for all routes associated with user actions, in this case "users".Also, RoutePrefix attribute is used to specify the common route prefix at the controller level to eliminate the need to repeat the common route prefix on each and every controller action.
// The Route attribute uses at the level of the action method to define a specific route for the UserDetails action, which includes the {id} parameter for the user ID.
// By using explicit routing attributes, a developer can create more straightforward and user-friendly URLs, such as "/users/details/123", instead of relying solely on routes based on default conventions.



///Example N2

//noncompliant
public class UserController : Controller
{
    public ActionResult UserDetails(int id)
    {
        User user = GetUserById(id);

   
        ViewBag.UserName = user.Name;
        ViewBag.UserEmail = user.Email;
        ViewBag.UserRole = GetUserRole(user);


        EmailService.SendWelcomeEmail(user.Email);

 
        LogService.LogActivity(user.Id, "User details viewed");

        return View();
    }

    // Other actions and methods for user-related functionality
}



// In this noncompliant code example, the UserController violates the Single Responsibility Principle by having multiple responsibilities within the UserDetails action:
// Displaying user details: The action retrieves the user details and sets ViewBag properties to be used in the view for displaying user information.
// Sending a welcome email: The action contains logic for sending a welcome email to the user, which is not directly related to displaying user details.
// Logging user activity: The action includes code for logging user activity, which is another separate concern.

//compliant
public class UserController : Controller
{
    private readonly IUserService userService;
    private readonly IEmailService emailService;
    private readonly ILogService logService;

    public UserController(IUserService userService, IEmailService emailService, ILogService logService)
    {
        this.userService = userService;
        this.emailService = emailService;
        this.logService = logService;
    }

    public ActionResult UserDetails(int id)
    {
        User user = userService.GetUserById(id);

        return View(user);
    }

    // Other actions and methods for user-related functionality
}
// In this compliant code example:
// The UserController follows the Single Responsibility Principle by delegating specific responsibilities to separate services (IUserService, IEmailService, ILogService).
// The UserDetails action focuses solely on retrieving the user details from the IUserService and passing them to the view for display.
// The responsibilities of sending emails and logging user activity are moved to separate services (IEmailService and ILogService) that are injected into the controller's constructor.


///Example N3

//noncompliant

public class UserController : Controller
{
    public ActionResult UserDetails(int id)
    {
        User user = GetUserById(id);

        if (user.IsActive && user.Age >= 18)
        {
            ViewBag.Message = "User is eligible.";
        }
        else
        {
            ViewBag.Message = "User is not eligible.";
        }

        return View(user);
    }
}

//In this noncompliant code example, this typical mistake is performing business logic directly in the controller. The controller checks if the user is active and over 18 years old and sets a message in the ViewBag accordingly. This violates the principle of separation of concerns and can make the controller bloated with business logic.

//compliant

public class UserController : Controller
{
    private readonly IUserService userService;

    public UserController(IUserService userService)
    {
        this.userService = userService;
    }

    public ActionResult UserDetails(int id)
    {
        User user = userService.GetUserById(id);

        string eligibilityMessage = userService.GetEligibilityMessage(user);
        ViewBag.Message = eligibilityMessage;

        return View(user);
    }
}
//In this compliant code example:
// The controller delegates the business logic to a separate service (IUserService), which is injected into the controller's constructor.
// The IUserService encapsulates the logic for determining the eligibility message based on the user's properties.
// The controller retrieves the eligibility message from the IUserService and sets it in the ViewBag for use in the view.
// By separating the business logic into a dedicated service, we adhere to the principle of separation of concerns. This improves code maintainability, testability, and allows for better flexibility in modifying or extending the business logic without impacting the controller.

// It's important to keep in mind that the controller's responsibility is to coordinate the flow of data and interaction between the model and the view, while business logic should be encapsulated in separate services or classes.



///Example N4

//noncompliant
public class UserController : Controller
{
    public ActionResult GetUser(int id)
    {
        User user = UserRepository.GetUserById(id);

        // Noncompliant: Performing complex data transformations in the controller
        string formattedName = FormatUserName(user.Name);
        ViewBag.FormattedName = formattedName;

        return View(user);
    }

    private string FormatUserName(string name)
    {
        // Noncompliant: Complex data transformation logic in the controller
        string formattedName = name.ToUpper() + " [Formatted]";
        return formattedName;
    }
}

//In this noncompliant code example, the mistake is performing complex data transformations within the controller. The controller is responsible for handling user interactions and coordinating data flow, but it shouldn't perform complex data transformations or apply business logic directly.


//compliant

public class UserController : Controller
{
    private readonly IUserRepository userRepository;

    public UserController(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public ActionResult GetUser(int id)
    {
        User user = userRepository.GetUserById(id);
        string formattedName = UserFormatter.FormatUserName(user.Name);
        ViewBag.FormattedName = formattedName;
        return View(user);
    }
}

public static class UserFormatter
{
    public static string FormatUserName(string name)
    {
        string formattedName = name.ToUpper() + " [Formatted]";
        return formattedName;
    }
}

// In this compliant code example:
// The complex data transformation logic is moved to a separate static class UserFormatter, which is responsible for formatting user names.
// The UserFormatter class encapsulates the logic for transforming the user name.
// The controller uses the UserFormatter class to format the user name and set it in the ViewBag for use in the view.
// By separating the complex data transformation logic into a dedicated class (UserFormatter), we brake to the principle of separation of concerns. The controller focuses on its responsibility of handling user interactions, while the formatting logic is delegated to a separate class.
// Separating concerns in this manner improves code maintainability, reusability, and testability. It allows for easier modification or extension of the data transformation logic without impacting the controller's functionality.



///Example N5

//noncompliant

public class UserController : Controller
{
    private readonly ApplicationDbContext dbContext;

    public UserController(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public ActionResult GetUser(int id)
    {
        User user = dbContext.Users.FirstOrDefault(u => u.Id == id);

        // Noncompliant: Not checking for null before accessing properties
        string userName = user.Name; // Potential NullReferenceException

        return View();
    }
}


//In this noncompliant code example, the mistake is not checking for null before accessing properties of the User object obtained from FirstOrDefault. If no user is found with the given ID, the FirstOrDefault method will return null. Accessing properties of a null object will result in a NullReferenceException at runtime.

//compliant
public class UserController : Controller
{
    private readonly ApplicationDbContext dbContext;

    public UserController(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public ActionResult GetUser(int id)
    {
        User user = dbContext.Users.FirstOrDefault(u => u.Id == id);

        if (user != null)
        {
            string userName = user.Name;
            return View();
        }

        // Handle the case where no user is found
        return RedirectToAction("UserNotFound");
    }

    public ActionResult UserNotFound()
    {
        return View();
    }
}

// After retrieving the user with FirstOrDefault, we explicitly check if the user object is null before accessing its properties.
// If a user is found, we safely access its properties without risking a NullReferenceException.
// If no user is found, we handle the scenario by redirecting to a "UserNotFound" view or taking any other appropriate action.
// By checking for null before accessing properties, we prevent potential runtime exceptions and handle the case when no user is found. This ensures the application behaves as expected and provides a better user experience.

///Example N6


//noncompliant
Controller: HomeController.cs
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
    
    [HttpPost]
    public IActionResult SubmitForm(string data)
    {
        // Process form submission without validating AntiForgeryToken
        // ...
        
        return RedirectToAction("Success");
    }
    
    public IActionResult Success()
    {
        return View();
    }
}

Model: MyModel.cs
public class MyModel
{
    public string Data { get; set; }
}

View: Index.cshtml
@model MyModel

@{
    ViewData["Title"] = "Home Page";
}

<h2>Welcome to the Home Page!</h2>

<form method="post" action="/Home/SubmitForm">
    <div class="form-group">
        <label for="data">Data:</label>
        <input type="text" name="data" id="data" class="form-control" required />
    </div>
    
    <button type="submit" class="btn btn-primary">Submit</button>
</form>


View: Success.cshtml
@{
    ViewData["Title"] = "Success";
}

<h2>Form submitted successfully!</h2>

<p>Thank you for submitting the form.</p>

//compliant

Controller: HomeController.cs
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var model = new MyModel();
        return View(model);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitForm(MyModel model)
    {
        // Validate AntiForgeryToken
        if (!ValidateAntiForgeryToken())
        {
            return BadRequest("Invalid AntiForgeryToken");
        }
        
        // Process form submission
        // ...
        
        return RedirectToAction("Success");
    }
    
    public IActionResult Success()
    {
        return View();
    }
    
    private bool ValidateAntiForgeryToken()
    {
        try
        {
            HttpContext.RequestServices.GetService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>()
                .ValidateRequestAsync(HttpContext).GetAwaiter().GetResult();
            
            return true;
        }
        catch
        {
            return false;
        }
    }
}

Model: MyModel.cs

public class MyModel
{
    public string Data { get; set; }
}

View: Index.cshtml
@model MyModel

@{
    ViewData["Title"] = "Home Page";
}

<h2>Welcome to the Home Page!</h2>

<form method="post" action="/Home/SubmitForm">
    @Html.AntiForgeryToken()
    
    <div class="form-group">
        <label for="data">Data:</label>
        <input type="text" name="Data" id="data" class="form-control" required />
    </div>
    
    <button type="submit" class="btn btn-primary">Submit</button>
</form>

View: Success.cshtml

@{
    ViewData["Title"] = "Success";
}

<h2>Form submitted successfully!</h2>

<p>Thank you for submitting the form.</p>

// In the noncompliant version, the SubmitForm action skips the AntiForgeryToken validation. This poses a security risk as it allows potential CSRF (Cross-Site Request Forgery) attacks. In the noncompliant version, the SubmitForm action doesn't include a model parameter, and the form data is retrieved directly from the request parameters. The compliant version includes a model parameter MyModel model in the SubmitForm action, allowing the form data to be bound to the model automatically.
// In the compliant version, the SubmitForm action includes the [ValidateAntiForgeryToken] attribute, and it validates the AntiForgeryToken using the ValidateAntiForgeryToken method before processing the form submission. This ensures that the form submission is protected against CSRF attacks by validating the AntiForgeryToken.

// It's important to include AntiForgeryToken validation to prevent unauthorized form submissions and ensure the security of your application.
