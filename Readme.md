# CEC.Routing

> **Note**.  There is a more radical solution to the whole routing issue covered in an article [An Alternative to Routing in Blazor](https://github.com/ShaunCurtis/CEC.Blazor.Examples/blob/master/Articles/Replacing%20Blazor%20Routing%20with%20ViewManager.md), a Github Repo [CEC.Blazor.Examples](https://github.com/ShaunCurtis/CEC.Blazor.Examples) and a [demo site](https://cec-blazor-examples.azurewebsites.net/).

Controlled Routing Library

> Note Version 1.2 is released.  There are some import **Obselete** marked changes.

This is an enhanced version of the standard Blazor Router.  It adds functionality to control routing on a editor page with unsaved changes.

The package is available on NuGet: CEC.Routing.

The sample project uses the basic Blazor Weather app, implementing a WeatherForecast object editor with navigation control when a Weather Station object is unsaved.  Review the code for a more detailed code example of an implementation of the router.

There is an additional project and library at https://github.com/ShaunCurtis/CEC.FormControls that implements more advanced router control using a set of enhanced form controls.

To implement the router you need to:

Add the Service elements to your startup.cs in Blazor Server:

```c#
using CEC.Routing;
....
public void ConfigureServices(IServiceCollection services)
{
...
    services.AddCECRouting();
}
```

Or, Add the Service elements to your program.cs in Blazor WASM Client:

```c#
     builder.Services.AddCECRouting();
```

Change your App.razor to use the new router:

```html
<RecordRouter AppAssembly="@typeof(Program).Assembly">
....
</RecordRouter>
```

Add the following code below the blazor.server.js script block to the _Host.cshtml file in Blazor Server:

```html
<script src="_content/CEC.Routing/cec.routing.js"></script>
```

Or, Add the following code below the blazor.server.js script block to the index.html file in Blazor WASM:

```html
<script src="_content/CEC.Routing/cec.routing.js"></script>
```

At this point you are up and running.  The router acts and behaves like the standard Blazor router.  The router interacts with a RouterSessionService and only controls routing when certain properties are set, but by default it's vanilla Blazor routing.

To implement controlled routing, you need to implement the IRecordRouterComponent interface on your editor component and register your component with the RouterSessionService.

In the example project there's a EditorComponentBase class that does most of the boiler plating.  The key activities are in the OnInitialized event:

```c#
this.RouteUrl = this.NavManager.Uri;
this.RouterSessionService.ActiveComponent = this;
this.RouterSessionService.NavigationCancelled += OnNavigationCancelled;
```

The current component is registered with the RouterSessionService, the current page url  is set, and a local handler is registered with the navigation cancelled event on the service. Note that while the event lives on the service, it's triggered by the router.

The OnNavigationCancelled event handler in the sample project looks like this:

```c#
protected void OnNavigationCancelled(object sender, EventArgs e)
{
this.ExitAttempt = true;
this.Alert.SetAlert("<b>RECORD ISN'T SAVED</b>. Either Cancel or Exit Without Saving.", Alert.AlertDanger);
this.StateHasChanged();
}
```

What ties everything together is the IsClean property on the IRecordRouterComponent interface implemented by the component.  Set this to true and the router routes, set this to false (when there are unsaved edits) and the router stops routing, and instead raises the NavigationCancelled event on the RouterSessionService.

How you implement control of the IsClean property and what you do on the NavigationCancelled event is up to you.  The example project demonstrates a simple implementation.

### Updates:
https://github.com/ShaunCurtis/CEC.Routing/wiki/Updates
