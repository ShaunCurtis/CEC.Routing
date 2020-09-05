
# CEC.Routing

Single Page Applications have several issues running as applications in a web browser. One such is navigation: the user can navigate away from the page in a variety of ways where the application have little control over what happens. Data loss often occurs in a less than satisfactory experience for the user.

This library seeks to address the following navigation issues:

1. Intercepting and potentially cancelling intra-application routing when the page is dirty.

1. Intercepting and warning on a navigation event away from the application - there's no browser mechanism to prevent this. Entering a new URL in the URL bar, clicking on a favourite, ...

The router can be used in Server and WASM projects.

# Library and Example Repositories

**CEC.Routing** is an implementation of the standard Blazor router with functionality needed to control intra-application routing and the onbeforeunload browser event behaviour. It's released and available as a Nuget Package. The source code is available at [https://github.com/ShaunCurtis/CEC.Routing](https://github.com/ShaunCurtis/CEC.Routing).

All the source code is available under the MIT license.

## Intra-Application Routing

Intercept routing on a dirty i.e. unsaved data page isn't possible with the out-of-the-box Blazor navigator/router. In fact we have no way to get to the internal routing decision making, so need start from scratch.

A quick digression on the basics of Blazor navigation/routing. 

DOM navigation events such as anchors, etc are captured by the Blazor JavaScript Interop code. They surface in the C# Blazor world through the NavigationManager. The user clicks on an HTML link or navlink in the browser, the NavigationManager service instance gets populated with the relevant URL data and the **NavigationManager**._LocationChanged_ event is fired. That's it for the NavigationManager. The heavy lifting is done by the Router. It gets initialized through app.razor on a page load, and wires itself into the **NavigationManager**._LocationChanged_ event. The developer has no access to its internal workings, so can't cancel anything.

Fortunately, we can clone the standard router and add the necessary functionality. The new router is called RecordRouter. The key changes to the out-of-the-box router are as follows:

### RouterSessionService

Create a new scoped Service called RouterSessionService for controlling and interacting with the RecordRouter.
```c#
public class RouterSessionService
{
    /// <summary>
    /// Property containing the currently loaded component if set
    /// </summary>
    public IRecordRoutingComponent ActiveComponent { get; set; }

    /// <summary>
    /// Boolean to check if the Router Should Navigate
    /// </summary>
    public bool IsGoodToNavigate => this.ActiveComponent?.IsClean ?? true;

    /// <summary>
    /// Url of Current Route being navigated from
    /// </summary>
    public string RouteUrl { get
        {
            var url = this.ActiveComponent?.PageUrl ?? string.Empty;
            url = this.ActiveComponent?.RouteUrl ?? url;
            return url;
        }
    }

    /// <summary>
    /// Url of Current Page being navigated from
    /// This Property is depreciated after version 1.1
    /// Use RouteURL
    /// </summary>
    [Obsolete]
    public string PageUrl => RouteUrl;

    /// <summary>
    /// Url of the previous Route
    /// </summary>
    public string ReturnRouteUrl { get; set; }

    /// <summary>
    /// Url of the Last Route
    /// </summary>
    public string LastRouteUrl { get; set; }

    /// <summary>
    /// Url of the Last Route
    /// This Property is depreciated after version 1.1
    /// Use LastRouteURL
    /// </summary>
    [Obsolete]
    public string LastPageUrl => LastRouteUrl;

    /// <summary>
    /// Url of the navigation cancelled page
    /// </summary>
    public string NavigationCancelledUrl { get; set; }

    /// <summary>
    /// Event to notify Navigation Cancellation
    /// </summary>
    public event EventHandler NavigationCancelled;

    /// <summary>
    /// Event to notify that Intra Page Navigation has taken place
    /// useful when using Querystring controlled pages
    /// </summary>
    public event EventHandler SameComponentNavigation;

    /// <summary>
    /// Event to notify that Intra Page Navigation has taken place
    /// useful when using Querystring controlled pages
    /// This Event Handler is depreciated after version 1.1
    /// use SameComponentNavigation
    /// </summary>
    [Obsolete]
    public event EventHandler IntraPageNavigation;

    private readonly IJSRuntime _js;

    private bool _ExitShowState { get; set; }

    public RouterSessionService(IJSRuntime js)
    {
        _js = js;
    }

    /// <summary>
    /// Method to trigger the NavigationCancelled Event
    /// </summary>
    public void TriggerNavigationCancelledEvent() => this.NavigationCancelled?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Method to trigger the IntraPageNavigation Event
    /// </summary>
    public void TriggerSameComponentNavigation() 
    {
        this.SameComponentNavigation?.Invoke(this, EventArgs.Empty);
        this.IntraPageNavigation?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Method to trigger the IntraPageNavigation Event
    /// This Event Trigger is depreciated after version 1.1
    /// use TriggerSameComponentNavigation
    /// </summary>
    [Obsolete]
    public void TriggerIntraPageNavigation()
    {
        this.SameComponentNavigation?.Invoke(this, EventArgs.Empty);
        this.IntraPageNavigation?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Method to set or unset the browser onbeforeexit challenge
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public void SetPageExitCheck(bool show)
    {
        if (show != _ExitShowState) _js.InvokeAsync<bool>("cec_setEditorExitCheck", show);
        _ExitShowState = show;
    }

}
```
### RecordRouter

This is a straight clone of the shipped router. The only changes are in the _OnLocationChanged_ event handler. It now looks like this:

```c#
private async void OnLocationChanged(object sender, LocationChangedEventArgs args)
{
    // Get the Route Uri minus any query string
    var routeurl = this.NavigationManager.Uri.Contains("?") ? this.NavigationManager.Uri.Substring(0, this.NavigationManager.Uri.IndexOf("?")): this.NavigationManager.Uri ;

    // Sets the LastRouteUrl to detect same route navigation i.e. "/Record/Editor?id=1" & "/Record/Editor?id=2"
    // and saves the previous route to ReturnRouteUrl for exit actions
    if (RouterSessionService.LastRouteUrl != null && RouterSessionService.LastRouteUrl.Equals(routeurl, StringComparison.CurrentCultureIgnoreCase)) RouterSessionService.TriggerSameComponentNavigation();
    else RouterSessionService.ReturnRouteUrl = RouterSessionService.LastRouteUrl;
    RouterSessionService.LastRouteUrl = routeurl;

    _locationAbsolute = args.Location;
    // SCC ADDED - SessionState Check for Unsaved Component
    if (_renderHandle.IsInitialized && Routes != null && this.RouterSessionService.IsGoodToNavigate)
    {
        // Clear the Active Component - the next route will load itself if required
        this.RouterSessionService.ActiveComponent = null;
        this.RouterSessionService.NavigationCancelledUrl = null;
        Refresh(args.IsNavigationIntercepted);
    }
    else
    {
        // SCC ADDED - Trigger a Navigation Cancelled Event on the SessionStateService
        if (this.RouterSessionService.RouteUrl.Equals(_locationAbsolute, StringComparison.CurrentCultureIgnoreCase))
        {
            // Cancel routing
            this.RouterSessionService.TriggerNavigationCancelledEvent();
        }
        else
        {
            //  we're cancelling routing, but the Navigation Manager is current set to the aborted route
            //  so we set the navigation cancelled url so the route can navigate to it if necessary
            //  and do a reset trip through the Navigation Manager again to set this back to the original route
            //  We need to do this though an async method as at the moment in WASM we are blocking the only thread: other
            //  OnLocationChanged events handlers below us in the list haven't yet been called.
            //  If we just initiate another navigation event calling NavigationManager.NavigateTo(), components such as NavLinks
            //  will receive notification of the second event before the first (and thus highlight the wrong link!)
            //  So we make this method async, wrap NavigationManager.NavigateTo() in an async method with a small yielded delay, and await the method.
            //  The default delay can be overridden through the IRecordRoutingComponent interface on any component by setting the RouterDelay property.  Default is 50ms.
            this.RouterSessionService.NavigationCancelledUrl = this.NavigationManager.Uri;
            await ReNavigate();
        }
    }
}

/// <summary>
/// Async Method for the dummy run through the Navigator if we have cancelled navigation to set the URL back to the original.
/// Allows yielding so that other OnLocationChanged Events can be run - and stops OnlocationChanged Events arriving in the wrong order
/// NavLinks being one!
/// </summary>
/// <returns></returns>
private async Task ReNavigate()
{
    var delay = this.RouterSessionService?.ActiveComponent?.RouterDelay ?? 50;
    await Task.Delay(delay);
    this.NavigationManager.NavigateTo(this.RouterSessionService.RouteUrl);
}

```
On a **NavigationManager**._LocationChanged_ event, the method:

1. Gets the page URL minus any query string, and checks if we are good to route. (I like to use a combination of routing and query strings - more flexible than all routing in many instances).

    **Yes** - clears out the relevant fields on the **RouterSessionService** and routes through the _Refresh_ method.

    **No** - triggers a _NavigationCancelled_ event. We solve the displayed URL issue by making a second dummy run through navigation to reset the displayed URL. Anyone know a better way?

2. Check for Intra-Page Navigation and trigger the _IntraPageNavigation_ event if needed. Useful where only the query string changes.

The routing is controlled by the _IsGoodToNavigate_ property on the **RouterSessionService**.
```
public bool IsGoodToNavigate => this.ActiveComponent?.IsClean ?? true;
```
This is only false when we cancel routing - i.e. the _ActiveComponent_ exists and is dirty. A lot of coding/refactoring to make a binary check!

**Note**: *OnLocationChanged* has changed to being an async event handler and *ReNavigate* has been added as an awaited async method to handle registered *NavigationManager.LocationChanged()* event handlers being blocked in WASM apppications.

## Setting up you site to use the Router

Install the Nuget Package

#### Startup.cs

Add the CEC.Routing services
```
using CEC.Routing;
 
public void ConfigureServices(IServiceCollection services)
{
....
services.AddCECRouting();
....
}
```
#### _Imports.razor

Add the following namespace references
```
@using CEC.Routing
@using CEC.Routing.Services
@using CEC.Routing.Router
```
#### App.razor

Change the name of the Router to RecordRouter

```
<RecordRouter AppAssembly="@typeof(Program).Assembly">
......
</RecordRouter>
```
### Implementing the Router

There's a sample site on the Github repository demonstrating the use of the library on a WeatherForecast editor.

NOTE - Record routing only kicks in if you set up a page component to use it. Normal pages will route as normal: you don't need to configure them not to.

You interact with the router through the **RouterSessionService**. To configure a page to use the extra routing functionality:

1. Inject the service into any edit page.

2. Implement the **_IRecordRoutingComponent_** Interface on the page

Next you need to add an event handler for the navigation cancelled event. This should contain code to tell the user that navigation was cancelled and potentially ask them if they really want to leave the page.
```
protected virtual void OnNavigationCancelled(object sender, EventArgs e)
{
  this.NavigationCancelled = true;
  this.ShowExitConfirmation = true;
  this.AlertMessage.SetAlert("<b>THIS RECORD ISN'T SAVED</b>. Either <i>Save</i> or <i>Exit Without Saving</i>.", Alert.AlertDanger);
  InvokeAsync(this.StateHasChanged);
}
```
This one (from the **EditRecordComponentBase** boilerplate in the project):

1. Sets a couple of local properties used in controlling which buttons display when.

2. Sets an alert box to display.

3. Calls _StateHasChanged_ to refresh the UI.

Add the following code to the component _OnInitialized_ or _OnInitializedAsync_ event
```
  this.PageUrl = this.NavManager.Uri;
  this.RouterSessionService.ActiveComponent = this;
  this.RouterSessionService.NavigationCancelled += this.OnNavigationCancelled;
```
This:

1. Sets the **_PageURL_** property to the current URL (pages names/directories and routing URLs are now very different).

2. Sets the **RouterSessionService** _ActiveComponent_ reference to the component.

3. Attaches the above event hander to the **RouterSessionService** _NavigationCancelled_ Event.

The final bit of the jigsaw is connecting the **IRecordRoutingComponent**._IsClean_ property. Its important to get this right. The router uses this property to route/cancel routing.

In my projects it's wired directly to the _IsClean_ property on the specific data service associated with the record. It gets set when the record in the service changes.

In the CEC.Routing sample project it's set and unset in the _CheckForChanges_ method which is called whenever an edit control is changed.

The following code shows how to override the cancel routing event - such as when the users wants to exit regardless.
```
        protected void ConfirmExit()
        {
            this.IsClean = true;
            if (!string.IsNullOrEmpty(this.RouterSessionService.NavigationCancelledUrl)) this.NavManager.NavigateTo(this.RouterSessionService.NavigationCancelledUrl);
            else if (!string.IsNullOrEmpty(this.RouterSessionService.LastPageUrl)) this.NavManager.NavigateTo(this.RouterSessionService.LastPageUrl);
            else this.NavManager.NavigateTo("/");
        }
```
Note that the **RouterSessionService** holds the cancelled URL.

## Intercepting/Warning on external Navigation

You can't lock down the browser window to stop this - I wish we could. The only control browsers offer is the _onbeforeunload_ event. When a function is registered on this event, the browser displays a popup warning dialog, giving the user the option to cancel navigation. The degree of control, what appears in the box, and what you need the attached function to do differs across browsers.

The sledgehammer approach is to add the following to your **_Host.html** file:
```
<script>
  window.onbeforeunload = function () {
    return "Do you really want to leave?";
  };
</script>
```
It forces a popup exit box whenever the users tries to leave the application, like using a wrecking ball to crack a nut. There are many instances where you want to leave the application - authentication, print pages to name a couple. Having the exit popup box coming up every time is a pain.

CEC.Routing implements a more nuanced and focused alternative. It still uses the _onbeforeunload_ event, but dynamically registers and unregisters with the event as needed. i.e. only when a form is dirty.

#### CEC.Routing.js

The client side Javascript files looks like this (pretty self explanatory):
```
window.cec_setEditorExitCheck = function (show) {
  if (show) {
    window.addEventListener("beforeunload", cec_showExitDialog);
  }
  else {
    window.removeEventListener("beforeunload", cec_showExitDialog);
  }
}

window.cec_showExitDialog = function (event) {
  event.preventDefault();
  event.returnValue = "There are unsaved changes on this page. Do you want to leave?";
}
```
The JSInterop code is implemented as a method in **RouteSessionService**.
```
private bool _ExitShowState { get; set; }
public void SetPageExitCheck(bool show)
  {
    if (show != _ExitShowState) _js.InvokeAsync<bool>("cec_setEditorExitCheck", show);
   _ExitShowState = show;
  }
```
Add the following script reference to the _Host.html next to the blazor.server.js reference.
```
<script src="_content/CEC.Routing/cec.routing.js"></script>
```
In the CEC.Routing sample **WeatherForcastEditor**:
```
protected void CheckClean(bool setclean = false)
{
    if (setclean) this.IsClean = true;
    if (this.IsClean)
    {
      this.Alert.ClearAlert();
      this.RouterSessionService.SetPageExitCheck(false);
    }
    else
    {
      this.Alert.SetAlert("Forecast Changed", Alert.AlertWarning);
      this.RouterSessionService.SetPageExitCheck(true);
    }
}
```
This method is called by the _OnFieldChanged_ event handler, and the _Save_ and _ConfirmExit_ methods.
