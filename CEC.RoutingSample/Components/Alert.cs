using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.RoutingSample.Components
{
    public class Alert
    {
        public const string AlertPrimary = "alert-primary";
        public const string AlertSecondary = "alert-secondary";
        public const string AlertSuccess = "alert-success";
        public const string AlertDanger = "alert-danger";
        public const string AlertWarning = "alert-warning";
        public const string AlertInfo = "alert-info";
        public const string AlertLight = "alert-light";
        public const string AlertDark = "alert-dark";

        public string Message { get; set; } = string.Empty;

        public string CSS { get; set; } = string.Empty;

        public bool IsAlert { get; set; } = false;


        public void ClearAlert()
        {
            this.Message = string.Empty;
            this.CSS = "alert-info";
            this.IsAlert = false;
        }
        public void SetAlert(string message, string Css )
        {
            this.Message = message;
            this.CSS = Css;
            this.IsAlert = true;
        }

    }
}
