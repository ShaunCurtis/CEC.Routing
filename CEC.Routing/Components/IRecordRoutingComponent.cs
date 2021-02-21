using CEC.Routing.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Security.Cryptography.X509Certificates;

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
        /// Property to hold the current route Url
        /// We need this as the name of the component probably won't match the route
        /// </summary>
        public string RouteUrl { get; set; }

        /// <summary>
        /// Property to reflect the save state of the component
        /// I'm probably old school here(Clean/Dirty) - set to true if saved
        /// Checked by the router to see if we should cancel routing
        /// </summary>
        public bool IsClean { get; }

        /// <summary>
        /// Property to define the delay period before reloading
        /// Needed for WASM Apps as single threaded and blocking
        /// ms delay for task before doing dummy run through the Navigation Manager
        /// </summary>
        public int RouterDelay { get => 50; set { var x = value; } }

    }
}
