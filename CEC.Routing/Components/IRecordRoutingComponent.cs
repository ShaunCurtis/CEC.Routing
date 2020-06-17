using CEC.Routing.Services;
using Microsoft.AspNetCore.Components;

namespace CEC.Routing.Components
{
    /// <summary>
    /// Defines the properties used during Routing to check the current page state
    /// The current page/component is registered in the User Session Service as a IRecordRoutingComponent object
    /// </summary>
    public interface IRecordRoutingComponent
    {
        /// <summary>
        /// Injected Navigation Manager
        /// </summary>
        [Inject]
        public NavigationManager NavManager { get; set; }

        /// <summary>
        /// Injected User Session Object
        /// </summary>
        [Inject]
        public RouterSessionService RouterSessionService { get; set; }

        /// <summary>
        /// Property to hold the current page Url
        /// We need this as the name of the component probably won't match the route
        /// </summary>
        public string PageUrl { get; set; }

        /// <summary>
        /// Property to reflect the save state of the component
        /// I'm probably old school here(Clean/Dirty) - set to true if saved
        /// Checked by the router to see if we should cancel routing
        /// </summary>
        public bool IsClean { get; }

    }
}
