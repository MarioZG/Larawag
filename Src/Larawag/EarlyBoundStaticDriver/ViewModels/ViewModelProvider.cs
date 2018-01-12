using Larawag.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Larawag.EarlyBoundStaticDriver.ViewModels
{
    class ViewModelProvider
    {
        public LibrarySelectorViewModel LibrarySelectorViewModel
        {
            get
            {
                return new LibrarySelectorViewModel(new OrganizationServiceContextGenerator(), new CompilerService());
            }
        }
    }
}
