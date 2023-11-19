#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Linq;


#endregion

namespace RAA_Int_Challenge_04
{
    [Transaction(TransactionMode.Manual)]
    public class CommandCopyWallsAndGenericModels : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // Create document variable
            Document openDoc = null;

            // Loop through open documents and look for match
            foreach(Document curDoc in uiapp.Application.Documents)
            {
                if (curDoc.PathName.Contains("Sample 03"))
                {
                    openDoc = curDoc;
                }
            }

            // Create level from current view
            Level activeLevel = doc.ActiveView.GenLevel;

            //Collect walls and generic models from open file
            List<BuiltInCategory> categoryList = new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_Walls,
                BuiltInCategory.OST_GenericModel
            };

            ElementMulticategoryFilter categoryFilter = new ElementMulticategoryFilter(categoryList);

            FilteredElementCollector openDocCollector = new FilteredElementCollector(openDoc)
                .WherePasses(categoryFilter)
                .WhereElementIsNotElementType();

            List<ElementId> elementIdList = openDocCollector.Select(el => el.Id).ToList();

            TaskDialog.Show("Elements copied", $"{openDocCollector.Count()} elements have been copied from the sample 03 file!");

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create areas");
                ElementTransformUtils.CopyElements(openDoc, elementIdList, doc, null, new CopyPasteOptions());
                t.Commit();
            }


            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 1");

            return myButtonData1.Data;
        }
    }
}
