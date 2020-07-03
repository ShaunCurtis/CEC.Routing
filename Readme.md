# CEC.Routing
Controlled Routing Library

This is an enhanced version of the standard Blazor Router.  It adds functionality to control routing on a editor page with unsaved changes.

The package is available on NuGet: CEC.Routing.

The sample project uses the basic Blazor Weather app, implementing a WeatherForecast object editor with navigation control when a Weather Station object is unsaved.  Review the code for a more detailed code example of an implementation of the router.

There is an additional project and library at https://github.com/ShaunCurtis/CEC.FormControls that implements more advanced router control using a set of enhanced form controls.

To implement the router you need to:

Add the Service elements to your startup.cs:


        using CEC.Routing;
        ....
        public void ConfigureServices(IServiceCollection services)
        {
        ...
            services.AddCECRouting();
        }

Change your App.razor to use the new router:

    <RecordRouter AppAssembly="@typeof(Program).Assembly">
    ....
    </RecordRouter>

Add the following code below the blazor.server.js script block to the _Host.html file:

    <script src="_content/CEC.Routing/cec.routing.js"></script>

At this point you are up and running.  The router acts and behaves like the standard Blazor router.  The router interacts with a RouterSessionService and only controls routing when certain properties are set, but by default it's vanilla Blazor routing.

To implement controlled routing, you need to implement the IRecordRouterComponent interface on your editor component and register your component with the RouterSessionService.

In the example project there's a EditorComponentBase class that does most of the boiler plating.  The key activities are in the OnInitialized event:

            this.PageUrl = this.NavManager.Uri;
            this.RouterSessionService.ActiveComponent = this;
            this.RouterSessionService.NavigationCancelled += OnNavigationCancelled;

The current component is registered with the RouterSessionService, the current page url  is set, and a local handler is registered with the navigation cancelled event on the service. Note that while the event lives on the service, it's triggered by the router.

The OnNavigationCancelled event handler in the sample project looks like this:

        protected void OnNavigationCancelled(object sender, EventArgs e)
        {
            this.ExitAttempt = true;
            this.Alert.SetAlert("<b>RECORD ISN'T SAVED</b>. Either Cancel or Exit Without Saving.", Alert.AlertDanger);
            this.StateHasChanged();
        }

What ties everything together is the IsClean property on the IRecordRouterComponent interface implemented by the component.  Set this to true and the router routes, set this to false (when there are unsaved edits) and the router stops routing, and instead raises the NavigationCancelled event on the RouterSessionService.

How you implement control of the IsClean property and what you do on the NavigationCancelled event is up to you.  The example project demonstrates a simple implementation.

Updates:
1.0.1 - https://github.com/ShaunCurtis/CEC.Routing/wiki/1.0.1-Updates
