﻿using CEC.Routing.Components;
using CEC.Routing.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.RoutingSample.Components
{
    public class EditorComponentBase : ComponentBase, IRecordRoutingComponent, IDisposable
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
        /// IRecordRoutingComponent implementation
        /// </summary>
        public string RouteUrl { get; set; }

        /// <summary>
        /// IRecordRoutingComponent implementation
        /// </summary>
        public bool IsClean { get; set; } = true;

        /// <summary>
        /// Boolean property set when the user attempts to exit a dirty component
        /// </summary>
        protected bool ExitAttempt { get; set; }

        /// <summary>
        /// Form Edit Context
        /// </summary>
        public EditContext EditContext { get; set; }

        /// <summary>
        /// Alert object used in UI by UI Alert
        /// </summary>
        public Alert Alert { get; set; } = new Alert();

        protected override Task OnInitializedAsync()
        {
            this.RouteUrl = this.NavManager.Uri;
            this.RouterSessionService.ActiveComponent = this;
            this.RouterSessionService.NavigationCancelled += OnNavigationCancelled;
            return base.OnInitializedAsync();
        }

        /// <summary>
        /// Event Handler for the Navigation Cancelled event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnNavigationCancelled(object sender, EventArgs e)
        {
            this.ExitAttempt = true;
            this.Alert.SetAlert("<b>RECORD ISN'T SAVED</b>. Either Cancel or Exit Without Saving.", Alert.AlertDanger);
            this.StateHasChanged();
        }

        public void Dispose()
        {
            this.RouterSessionService.NavigationCancelled -= OnNavigationCancelled;
        }
    }
}
