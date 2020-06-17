# CEC.Routing
Controlled Routing Library

This is a enhanced version of the standard Blazor router with functionality added to control routing on a edit page when there are unsaved changes.

The Package is available via NuGet as: CEC.Routing.

The sample project uses the basic Blazor Weather app, implementing a WeatherForecast object editor with navigation control when a Weather Station object is unsaved.  Review the code for a more detailed code example of an implemenatation of the router.

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

    <script>
        window.onbeforeunload = function () {
            return "Do you really want to leave?";
        };
    </script>

This warns the user when navigating to another site through the URL bar of the browser or an external link. The browser doesn't give you the control to stop this, just warn the user.  I wish the browser window had a page APP setting to control this!  The message that appears is browser specific, so you won't necessarily get the message you ask for.

At this point you are up and running.  The router acts and behaves like the standard Blazor router.  There's a RouterSessionService running that controls the router when certain properties are set, but by default it's vanilla Blazor routing.

To implement controlled routing, you need to implement the IRecordRouterComponent interface on your editor component and register your component with the RouterSessionService.

In the example project there's a EditorComponentBase class that does most of the boiler plating.  The key activities are in the OnInitialized event:

            this.PageUrl = this.NavManager.Uri;
            this.RouterSessionService.ActiveComponent = this;
            this.RouterSessionService.NavigationCancelled += OnNavigationCancelled;

The current component is registered with the RouterSessionSerive, the current page url  is set, and a local handler is registered with the navigation cancelled event on the service. Note that this event is triggered by the router.

The OnNavigationCancelled event handler in the sampl;e project looks like this:

        protected void OnNavigationCancelled(object sender, EventArgs e)
        {
            this.ExitAttempt = true;
            this.Alert.SetAlert("<b>RECORD ISN'T SAVED</b>. Either Cancel or Exit Without Saving.", Alert.AlertDanger);
            this.StateHasChanged();
        }

What ties everything together is the IsClean property on the IRecordRouterComponent interface implemented by the component.  Set this to true and the router routes, set this to false (when there are unsaved edits) and the router stops routing and instead raises the NavigationCancelled event on the RouterSessionService.

How you implement control of the IsClean property and what you do on the NavigationCancelled event is up to you.  The example project demonstrates a simple implementation.


