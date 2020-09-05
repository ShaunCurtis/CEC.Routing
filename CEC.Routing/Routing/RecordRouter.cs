using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CEC.Routing.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Logging;

namespace CEC.Routing.Router
{
    /*=======================================================================
     * This is a direct copy of the Blazor Router with the following changes:
     *  -  Injection of SessionStateService
     *  
     *  - OnLocationChanged Event Receiver checks for an Unsaved Component
     *      and if so cancels navigation and triggers the NavigationCancelled Event of the SessionStateService
     *  
     *  Also had to copy various other classes as they are declared internal to 
     *  Microsoft.AspNetCore.Components.Routing and therefore can't be referenced:
     *   - HashCodeCombine.cs
     *   - OptionalTypeRouteConstraint.cs
     *   - RouteConstraint.cs
     *   - RouteContext.cs
     *   - RouteEntry.cs
     *   - RouteTable.cs
     *   - RouteTableFactory.cs
     *   - RouteTemplate.cs
     *   - TermplateParser.cs
     *   - TemplateSegment.cs
     *   - TypeRouteConstraint.cs
     *   
      ======================================================================*/

    /// <summary>
    /// A customized Router to handle routing away from an unsaved Component
    /// </summary>
    public class RecordRouter : IComponent, IHandleAfterRender, IDisposable
    {
        static readonly char[] _queryOrHashStartChar = new[] { '?', '#' };
        static readonly ReadOnlyDictionary<string, object> _emptyParametersDictionary
            = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());

        RenderHandle _renderHandle;
        string _baseUri;
        string _locationAbsolute;
        bool _navigationInterceptionEnabled;
        ILogger<RecordRouter> _logger;

        [Inject] private NavigationManager NavigationManager { get; set; }

        [Inject] private RouterSessionService RouterSessionService { get; set; }

        [Inject] private INavigationInterception NavigationInterception { get; set; }

        [Inject] private ILoggerFactory LoggerFactory { get; set; }

        /// <summary>
        /// Gets or sets the assembly that should be searched for components matching the URI.
        /// </summary>
        [Parameter] public Assembly AppAssembly { get; set; }

        /// <summary>
        /// Gets or sets a collection of additional assemblies that should be searched for components
        /// that can match URIs.
        /// </summary>
        [Parameter] public IEnumerable<Assembly> AdditionalAssemblies { get; set; }

        /// <summary>
        /// Gets or sets the content to display when no match is found for the requested route.
        /// </summary>
        [Parameter] public RenderFragment NotFound { get; set; }

        /// <summary>
        /// Gets or sets the content to display when a match is found for the requested route.
        /// </summary>
        [Parameter] public RenderFragment<RouteData> Found { get; set; }

        private RouteTable Routes { get; set; }

        /// <inheritdoc />
        public void Attach(RenderHandle renderHandle)
        {
            _logger = LoggerFactory.CreateLogger<RecordRouter>();
            _renderHandle = renderHandle;
            _baseUri = NavigationManager.BaseUri;
            _locationAbsolute = NavigationManager.Uri;
            NavigationManager.LocationChanged += this.OnLocationChanged;
        }

        /// <inheritdoc />
        public Task SetParametersAsync(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);

            if (AppAssembly == null)
            {
                throw new InvalidOperationException($"The {nameof(Router)} component requires a value for the parameter {nameof(AppAssembly)}.");
            }

            // Found content is mandatory, because even though we could use something like <RouteView ...> as a
            // reasonable default, if it's not declared explicitly in the template then people will have no way
            // to discover how to customize this (e.g., to add authorization).
            if (Found == null)
            {
                throw new InvalidOperationException($"The {nameof(Router)} component requires a value for the parameter {nameof(Found)}.");
            }

            // NotFound content is mandatory, because even though we could display a default message like "Not found",
            // it has to be specified explicitly so that it can also be wrapped in a specific layout
            if (NotFound == null)
            {
                throw new InvalidOperationException($"The {nameof(Router)} component requires a value for the parameter {nameof(NotFound)}.");
            }

            RouterSessionService.LastRouteUrl = this.NavigationManager.Uri.Contains("?") ? this.NavigationManager.Uri.Substring(0, this.NavigationManager.Uri.IndexOf("?")) : this.NavigationManager.Uri;


            var assemblies = AdditionalAssemblies == null ? new[] { AppAssembly } : new[] { AppAssembly }.Concat(AdditionalAssemblies);
            Routes = RouteTableFactory.Create(assemblies);
            Refresh(isNavigationIntercepted: false);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            NavigationManager.LocationChanged -= OnLocationChanged;
        }

        private static string StringUntilAny(string str, char[] chars)
        {
            var firstIndex = str.IndexOfAny(chars);
            return firstIndex < 0
                ? str
                : str.Substring(0, firstIndex);
        }

        private void Refresh(bool isNavigationIntercepted)
        {
            var locationPath = NavigationManager.ToBaseRelativePath(_locationAbsolute);
            locationPath = StringUntilAny(locationPath, _queryOrHashStartChar);
            var context = new RouteContext(locationPath);
            Routes.Route(context);

            if (context.Handler != null)
            {
                if (!typeof(IComponent).IsAssignableFrom(context.Handler))
                {
                    throw new InvalidOperationException($"The type {context.Handler.FullName} " +
                        $"does not implement {typeof(IComponent).FullName}.");
                }

                Log.NavigatingToComponent(_logger, context.Handler, locationPath, _baseUri);

                var routeData = new RouteData(
                    context.Handler,
                    context.Parameters ?? _emptyParametersDictionary);
                _renderHandle.Render(Found(routeData));
            }
            else
            {
                if (!isNavigationIntercepted)
                {
                    Log.DisplayingNotFound(_logger, locationPath, _baseUri);

                    // We did not find a Component that matches the route.
                    // Only show the NotFound content if the application developer programatically got us here i.e we did not
                    // intercept the navigation. In all other cases, force a browser navigation since this could be non-Blazor content.
                    _renderHandle.Render(NotFound);
                }
                else
                {
                    Log.NavigatingToExternalUri(_logger, _locationAbsolute, locationPath, _baseUri);
                    NavigationManager.NavigateTo(_locationAbsolute, forceLoad: true);
                }
            }
        }

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

        Task IHandleAfterRender.OnAfterRenderAsync()
        {
            if (!_navigationInterceptionEnabled)
            {
                _navigationInterceptionEnabled = true;
                return NavigationInterception.EnableNavigationInterceptionAsync();
            }

            return Task.CompletedTask;
        }

        private static class Log
        {
            private static readonly Action<ILogger, string, string, Exception> _displayingNotFound =
                LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(1, "DisplayingNotFound"), $"Displaying {nameof(NotFound)} because path '{{Path}}' with base URI '{{BaseUri}}' does not match any component route");

            private static readonly Action<ILogger, Type, string, string, Exception> _navigatingToComponent =
                LoggerMessage.Define<Type, string, string>(LogLevel.Debug, new EventId(2, "NavigatingToComponent"), "Navigating to component {ComponentType} in response to path '{Path}' with base URI '{BaseUri}'");

            private static readonly Action<ILogger, string, string, string, Exception> _navigatingToExternalUri =
                LoggerMessage.Define<string, string, string>(LogLevel.Debug, new EventId(3, "NavigatingToExternalUri"), "Navigating to non-component URI '{ExternalUri}' in response to path '{Path}' with base URI '{BaseUri}'");

            internal static void DisplayingNotFound(ILogger logger, string path, string baseUri)
            {
                _displayingNotFound(logger, path, baseUri, null);
            }

            internal static void NavigatingToComponent(ILogger logger, Type componentType, string path, string baseUri)
            {
                _navigatingToComponent(logger, componentType, path, baseUri, null);
            }

            internal static void NavigatingToExternalUri(ILogger logger, string externalUri, string path, string baseUri)
            {
                _navigatingToExternalUri(logger, externalUri, path, baseUri, null);
            }
        }
    }
}