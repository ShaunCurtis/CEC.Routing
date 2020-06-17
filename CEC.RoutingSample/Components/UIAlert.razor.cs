using Microsoft.AspNetCore.Components;

namespace CEC.RoutingSample.Components
{
    public partial class UIAlert
    {
        [Parameter]
        public Alert Alert { get; set; } = new Alert();

        [Parameter]
        public bool Boxing { get; set; } = true;

        [Parameter]
        public bool Small { get; set; }

        protected bool IsAlert => this.Alert != null && this.Alert.IsAlert;

        protected string Css => this.Small ? "alert alert-sm" : "alert";

    }
}
