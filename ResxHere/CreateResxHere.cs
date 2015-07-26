//------------------------------------------------------------------------------
// <copyright file="CreateResxHere.cs" company="Kevin Erler">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.IO;
using System.Runtime.InteropServices;
using EnvDTE80;
using EnvDTE;

namespace ResxHere
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CreateResxHere
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("d51faa80-3297-4b45-9930-708e16497ff1");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        private static DTE2 _Dte;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateResxHere"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private CreateResxHere(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            _Dte = ServiceProvider.GetService(typeof(DTE)) as DTE2;

            OleMenuCommandService commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                //var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);

                var menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
                menuItem.BeforeQueryStatus += MenuCommand_BeforeQueryStatus;

                commandService.AddCommand(menuItem);
            }
        }

        void MenuCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            // get the menu that fired the event
            var menuCommand = sender as OleMenuCommand;
            if (menuCommand != null)
            {
                // start by assuming that the menu will not be shown
                menuCommand.Visible = false;
                menuCommand.Enabled = false;

                IVsHierarchy hierarchy = null;

                uint itemid = VSConstants.VSITEMID_NIL;

                if (!FileNavigation.IsSingleProjectItemSelection(out hierarchy, out itemid)) return;
                // Get the file path
                string itemFullPath = null;
                ((IVsProject)hierarchy).GetMkDocument(itemid, out itemFullPath);
                var transformFileInfo = new FileInfo(itemFullPath);

                bool isCodeFile = transformFileInfo.Extension.EndsWith(".cs", true, CultureInfo.InvariantCulture);

                // if not leave the menu hidden
                if (!isCodeFile) return;

                menuCommand.Visible = true;
                menuCommand.Enabled = true;
            }
        }

        

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CreateResxHere Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new CreateResxHere(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var selectedFile = FileNavigation.GetSelectedFilePath();

            var cultures = ((CreateResxHerePackage)package).Cultures;
            
            foreach (var culture in cultures )
            {
                var info = CultureInfo.GetCultureInfo(culture);
                AddResxFileForCulture(selectedFile, info);
            }
        }

        private void AddResxFileForCulture(FileInfo file, CultureInfo culture)
        {
            var cultureExtension = culture.Name.Replace("-", ".");
            var resxFile = Path.ChangeExtension(file.FullName, $"{cultureExtension}.resx");

            try
            {
                if (File.Exists(resxFile)) return;
                FileNavigation.AddItemToActiveProject(resxFile, "Resource", "CSharp", _Dte);
            }
            catch
            {
                //Ignore
            }
        }
    }
}
