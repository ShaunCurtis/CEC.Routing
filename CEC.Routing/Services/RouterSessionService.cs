using System;
using CEC.Routing.Components;
using Microsoft.JSInterop;

#nullable disable warnings

namespace CEC.Routing.Services
{
    /// <summary>
    /// Service Class used by the Record Router to track routing operations for the current user session
    /// Needs to be loaded as a Scoped Service
    /// 
    /// </summary>
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
                var url = this.ActiveComponent?.RouteUrl ?? string.Empty;
                url = this.ActiveComponent?.RouteUrl ?? url;
                return url;
            }
        }

        /// <summary>
        /// Url of the previous Route
        /// </summary>
        public string ReturnRouteUrl { get; set; }

        /// <summary>
        /// Url of the Last Route
        /// </summary>
        public string LastRouteUrl { get; set; }

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

        private IJSRuntime _js;

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
        }

        /// <summary>
        /// Method to set or unset the browser onbeforeexit challenge
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public void SetPageExitCheck(bool show)
        {
            if (show != _ExitShowState && this._js != null)
                _js.InvokeAsync<bool>("cec_setEditorExitCheck", show);
            else
                show = false;
            _ExitShowState = show;
        }

        public bool CanRoute(string url, string locationAbsolute, out bool reNavigate)
        {
            var okToRoute = true;
            reNavigate = false;

            // Get the Route Uri minus any query string
            var routeurl = url.Contains("?") ? url.Substring(0, url.IndexOf("?")) : url;

            // Sets the LastRouteUrl to detect same route navigation i.e. "/Record/Editor?id=1" & "/Record/Editor?id=2"
            // and saves the previous route to ReturnRouteUrl for exit actions
            if (this.LastRouteUrl != null && this.LastRouteUrl.Equals(routeurl, StringComparison.CurrentCultureIgnoreCase)) this.TriggerSameComponentNavigation();
            else this.ReturnRouteUrl = this.LastRouteUrl;
            this.LastRouteUrl = routeurl;
            if (this.IsGoodToNavigate)
            {
                // Clear the Active Component - the next route will load itself if required
                this.ActiveComponent = null;
                this.NavigationCancelledUrl = null;
            }
            else
            {
                okToRoute = false;
                if (this.RouteUrl.Equals(locationAbsolute, StringComparison.CurrentCultureIgnoreCase))
                {
                    // Cancel routing
                    this.TriggerNavigationCancelledEvent();
                }
                else
                {
                    //  we're cancelling routing, but the Navigation Manager is current set to the aborted route
                    //  so we set the navigation cancelled url so the route can navigate to it if necessary
                    //  and tell the router to do a reset trip through the Navigation Manager again to set this back to the original route
                    this.NavigationCancelledUrl = url;
                    reNavigate = true;
                }
            }
            return okToRoute;
        }
    }
}
