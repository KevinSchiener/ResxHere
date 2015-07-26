using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ResxHere
{
    [Guid("96B1BB97-64D2-4B85-8AAD-98F9EAC1B46A")]
    public class CultureOptions : DialogPage
    {
        private List<string> cultures = new List<string> { "de", "" };

        public List<string> Cultures
        {
            get { return cultures; }
            set { cultures = value; }
        }

        protected override IWin32Window Window
        {
            get
            {
                var page = new OptionsControl();
                page.optionPage = this;
                page.Initialize();
                return page;
            }
        }
    }
}
