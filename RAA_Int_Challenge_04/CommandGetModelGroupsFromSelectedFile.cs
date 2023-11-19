#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using Forms = System.Windows.Forms;

#endregion

namespace RAA_Int_Challenge_04
{
    [Transaction(TransactionMode.Manual)]
    public class CommandGetModelGroupsFromSelectedFile : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            //Get spaces from current file
            FilteredElementCollector spaceCollector = new FilteredElementCollector(doc)
                .OfClass(typeof(SpatialElement));

            TaskDialog.Show("Number of spaces", $"There are {spaceCollector.Count()} spaces in the document");

            // Create open file dialog
            Forms.OpenFileDialog dialog = new Forms.OpenFileDialog();
            dialog.Title = "Select Revit file";
            dialog.InitialDirectory = @"C:\";
            dialog.RestoreDirectory = true;
            dialog.Multiselect = false;
            dialog.Filter = ("Revit files (*.rvt)|*.rvt");

            // Check to see if user cancelled out of dialog
            if (dialog.ShowDialog() != Forms.DialogResult.OK) 
            {
                return Result.Failed;
            }

            string revitFilePath = dialog.FileName;

            // open selected file in background
            UIDocument closedUIDoc = uiapp.OpenAndActivateDocument(revitFilePath);
            Document closedDoc = closedUIDoc.Document;

            // collect groups from selected file
            FilteredElementCollector groupsCollector = new FilteredElementCollector(closedDoc)
                .OfCategory(BuiltInCategory.OST_IOSModelGroups)
                .WhereElementIsElementType();
            List<ElementId> groupIdList = groupsCollector.Select(x => x.Id).ToList();

            TaskDialog.Show("Number of groups", $"{groupsCollector.Count()} have been copied to the current document");


            using (Transaction t = new Transaction(doc))
            {
                t.Start("Copy model groups into selected file");

                // Create Groups
                ElementTransformUtils.CopyElements(closedDoc, groupIdList, doc, null, new CopyPasteOptions());
                t.Commit();
            }


            // Make current document active and close other document
            uiapp.OpenAndActivateDocument(doc.PathName);
            closedDoc.Close(false);

            FilteredElementCollector copiedGroupsCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_IOSModelGroups)
                .WhereElementIsElementType();

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create ");
                //Loop through spaces and insert groups
                foreach (SpatialElement space in spaceCollector)
                {
                    string groupName = space.LookupParameter("Comments").AsString();
                    LocationPoint insertionPoint = space.Location as LocationPoint;
                    foreach (GroupType groupType in copiedGroupsCollector)
                    {
                        if (groupName == groupType.Name)
                            doc.Create.PlaceGroup(insertionPoint.Point, groupType);
                    }
                }
                t.Commit();
            }

            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand2";
            string buttonTitle = "Button 2";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 2");

            return myButtonData1.Data;
        }
    }
}
