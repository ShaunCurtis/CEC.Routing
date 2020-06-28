using System;
using CEC.Routing.Components;

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
        public bool IsGoodToNavigate { get => this.ActiveComponent is null || (this.ActiveComponent != null && this.ActiveComponent.IsClean); }

        /// <summary>
        /// Url of Current Page being navigated from
        /// </summary>
        public string PageUrl { get => this.ActiveComponent is null ? string.Empty : this.ActiveComponent.PageUrl; }

        /// <summary>
        /// Url of the previous page
        /// </summary>
        public string LastPageUrl { get; set; }

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
        public event EventHandler IntraPageNavigation;

        /// <summary>
        /// Method to trigger the NavigationCancelled Event
        /// </summary>
        public void TriggerNavigationCancelledEvent()
        {
            this.NavigationCancelled?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Method to trigger the IntraPageNavigation Event
        /// </summary>
        public void TriggerIntraPageNavigation()
        {
            this.IntraPageNavigation?.Invoke(this, EventArgs.Empty);
        }

    }
}
