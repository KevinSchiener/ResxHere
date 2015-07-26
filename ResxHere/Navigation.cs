using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ResxHere
{
    class FileNavigation
    {
        public static FileInfo GetSelectedFilePath()
        {
            IVsHierarchy hierarchy = null;

            uint itemid = VSConstants.VSITEMID_NIL;

            if (!IsSingleProjectItemSelection(out hierarchy, out itemid)) return null;

            // Get the file path
            string itemFullPath = null;
            ((IVsProject)hierarchy).GetMkDocument(itemid, out itemFullPath);
            return new FileInfo(itemFullPath);
        }

        public static bool IsSingleProjectItemSelection(out IVsHierarchy hierarchy, out uint itemid)
        {
            hierarchy = null;
            itemid = VSConstants.VSITEMID_NIL;
            int hr = VSConstants.S_OK;

            var monitorSelection = Package.GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            var solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            if (monitorSelection == null || solution == null)
            {
                return false;
            }

            IVsMultiItemSelect multiItemSelect = null;
            IntPtr hierarchyPtr = IntPtr.Zero;
            IntPtr selectionContainerPtr = IntPtr.Zero;

            try
            {
                hr = monitorSelection.GetCurrentSelection(out hierarchyPtr, out itemid, out multiItemSelect, out selectionContainerPtr);

                if (ErrorHandler.Failed(hr) || hierarchyPtr == IntPtr.Zero || itemid == VSConstants.VSITEMID_NIL)
                {
                    // there is no selection
                    return false;
                }

                // multiple items are selected
                if (multiItemSelect != null) return false;

                // there is a hierarchy root node selected, thus it is not a single item inside a project

                if (itemid == VSConstants.VSITEMID_ROOT) return false;

                hierarchy = Marshal.GetObjectForIUnknown(hierarchyPtr) as IVsHierarchy;
                if (hierarchy == null) return false;

                Guid guidProjectID = Guid.Empty;

                if (ErrorHandler.Failed(solution.GetGuidOfProject(hierarchy, out guidProjectID)))
                {
                    return false; // hierarchy is not a project inside the Solution if it does not have a ProjectID Guid
                }

                // if we got this far then there is a single project item selected
                return true;
            }
            finally
            {
                if (selectionContainerPtr != IntPtr.Zero)
                {
                    Marshal.Release(selectionContainerPtr);
                }

                if (hierarchyPtr != IntPtr.Zero)
                {
                    Marshal.Release(hierarchyPtr);
                }
            }
        }

        public static void AddFileToActiveProject(string fileName, DTE2 dte)
        {
            Project project = GetActiveProject(dte);

            if (project == null || project.Kind == "{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}") // ASP.NET 5 projects
                return;

            string projectFilePath = GetProjectRoot(project).Value.ToString();
            string projectDirPath = Path.GetDirectoryName(projectFilePath);

            if (!fileName.StartsWith(projectDirPath, StringComparison.OrdinalIgnoreCase))
                return;

            project.ProjectItems.AddFromFile(fileName);

            var solution = (Solution2) dte.Solution;
        }

        public static void AddItemToActiveProject(string fileName, string templateName, string language, DTE2 dte)
        {
            Project project = GetActiveProject(dte);

            if (project == null || project.Kind == "{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}") // ASP.NET 5 projects
                return;

            string projectFilePath = GetProjectRoot(project).Value.ToString();
            string projectDirPath = Path.GetDirectoryName(projectFilePath);

            if (!fileName.StartsWith(projectDirPath, StringComparison.OrdinalIgnoreCase))
                return;

            var solution = (Solution2)dte.Solution;
            var template = solution.GetProjectItemTemplate(templateName, language);
            project.ProjectItems.AddFromTemplate(template, fileName);
        }

        public static Project GetActiveProject( DTE2 dte )
        {
            try
            {
                Array activeSolutionProjects = dte.ActiveSolutionProjects as Array;

                if (activeSolutionProjects != null && activeSolutionProjects.Length > 0)
                    return activeSolutionProjects.GetValue(0) as Project;
            }
            catch (Exception)
            {
                // Pass through and return null
            }

            return null;
        }

        public static Property GetProjectRoot(Project project)
        {
            Property prop;

            try
            {
                prop = project.Properties.Item("FullPath");
            }
            catch (ArgumentException)
            {
                try
                {
                    // MFC projects don't have FullPath, and there seems to be no way to query existence
                    prop = project.Properties.Item("ProjectDirectory");
                }
                catch (ArgumentException)
                {
                    // Installer projects have a ProjectPath.
                    prop = project.Properties.Item("ProjectPath");
                }
            }

            return prop;
        }
    }
}
