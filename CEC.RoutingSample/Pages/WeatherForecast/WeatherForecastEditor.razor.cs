using CEC.RoutingSample.Components;
using CEC.RoutingSample.Data;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Threading.Tasks;

namespace CEC.RoutingSample.Pages
{
    public partial class WeatherForecastEditor : EditorComponentBase
    {
        public WeatherForecast Record { get; set; } = new WeatherForecast();

        public WeatherForecast ShadowRecord { get; set; }

        protected override Task OnInitializedAsync()
        {
            // Set up the Edit Context
            this.EditContext = new EditContext(this.Record);

            // Register with the Edit Context OnFieldChanged Event
            this.EditContext.OnFieldChanged += OnFieldChanged;

            // Make a copy of the existing record - in this case it's always new but in the real world that won't be the case
            this.ShadowRecord = this.Record.Copy();

            // Get the actual page Url from the Navigation Manager
            this.PageUrl = this.NavManager.Uri;

            return base.OnInitializedAsync();
        }

        /// <summary>
        /// Event handler for when a edit form field change takes place
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnFieldChanged(object sender, EventArgs e)
        {
            this.ExitAttempt = false;
            this.CheckForChanges();
            this.CheckClean();
        }

        /// <summary>
        /// Quick and dirty implmentation of a Method to check the record against the shadow copy to see if there are any changes
        /// handles such events as user changing a value and then changing it back to the original
        /// </summary>
        protected void CheckForChanges()
        {
            this.IsClean = true;
            this.IsClean = this.Record.Date.Date.Equals(this.ShadowRecord.Date.Date) ? this.IsClean : false;
            this.IsClean = this.Record.TemperatureC.Equals(this.ShadowRecord.TemperatureC) ? this.IsClean : false;
            if (string.IsNullOrEmpty(this.Record.Summary) && !string.IsNullOrEmpty(this.ShadowRecord.Summary)) this.IsClean = false;
            else if (string.IsNullOrEmpty(this.ShadowRecord.Summary) && !string.IsNullOrEmpty(this.Record.Summary)) this.IsClean = false;
            else if (!this.Record.Summary.Equals(this.ShadowRecord.Summary)) this.IsClean = false;
        }

        protected void CheckClean(bool setclean = false)
        {
            if (setclean) this.IsClean = true;
            if (this.IsClean)
            {
                this.Alert.ClearAlert();
                this.RouterSessionService.SetPageExitCheck(false);
            }
            else
            {
                this.Alert.SetAlert("Forecast Changed", Alert.AlertWarning);
                this.RouterSessionService.SetPageExitCheck(true);
            }
        }

        /// <summary>
        /// Save Method called from the Button
        /// </summary>
        protected void Save()
        {
            this.ShadowRecord = this.Record.Copy();
            this.CheckClean(true);
            this.Alert.SetAlert("Forecast Saved", Alert.AlertSuccess);
            this.StateHasChanged();
        }

        /// <summary>
        /// Cancel Method called from the Button
        /// </summary>
        protected void Cancel()
        {
            this.ExitAttempt = false;
            this.CheckClean();
            this.StateHasChanged();
        }

        /// <summary>
        /// Confirm Exit Method called from the Button
        /// </summary>
        protected void ConfirmExit()
        {
            // To Escape with a dirty component override the component to clean
            // and get the page the user eas trying to navigate to
            this.CheckClean(true);
            if (!string.IsNullOrEmpty(this.RouterSessionService.NavigationCancelledUrl)) this.NavManager.NavigateTo(this.RouterSessionService.NavigationCancelledUrl);
            else if (!string.IsNullOrEmpty(this.RouterSessionService.LastPageUrl)) this.NavManager.NavigateTo(this.RouterSessionService.LastPageUrl);
            else this.NavManager.NavigateTo("/");
        }

    }
}
