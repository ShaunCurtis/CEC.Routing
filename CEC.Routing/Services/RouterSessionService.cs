using System;
using CEC.Routing.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.Configuration;

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
}
