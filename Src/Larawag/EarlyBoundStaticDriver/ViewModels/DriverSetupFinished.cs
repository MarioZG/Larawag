using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Larawag.EarlyBoundStaticDriver.ViewModels
{
    class DriverSetupFinished : EventArgs
    {
        public bool Confirmed { get; set; }

        public DriverSetupFinished(bool confirmed)
        {
            Confirmed = confirmed;
        }
    }
}
