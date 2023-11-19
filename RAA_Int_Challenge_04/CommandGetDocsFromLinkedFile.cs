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
    public class CommandGetDocFromLinkedFile : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // Get current level
            Level currentLevel = doc.ActiveView.GenLevel;

            // Get all links
            FilteredElementCollector revitLinkTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(RevitLinkType));

            // loop through links and get doc if loaded
            Document linkedDoc = null;
            //RevitLinkInstance link = null;

            foreach(RevitLinkType linkType in revitLinkTypes) 
            {
                if (linkType.GetLinkedFileStatus() == LinkedFileStatus.Loaded)
                {
                    RevitLinkInstance link = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_RvtLinks)
                        .OfClass(typeof(RevitLinkInstance))
                        .Where(x => x.GetTypeId() == linkType.Id).First() as RevitLinkInstance;

                    linkedDoc = link.GetLinkDocument();
                }
            }

            // Collect rooms from linked doc
            FilteredElementCollector roomCollector = new FilteredElementCollector(linkedDoc)
                .OfCategory(BuiltInCategory.OST_Rooms);

            TaskDialog.Show("Room Count", $"There are {roomCollector.Count()} rooms in the linked document: {linkedDoc.Title}");


            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create areas");
                foreach(Room room in roomCollector) 
                {
                    // get room data
                    string roomNumber = room.Number;
                    string roomName = room.Name;
                    string roomComments = room.LookupParameter("Comments").AsString();

                    // get room location
                    LocationPoint roomPoint = room.Location as LocationPoint;

                    // Create spaces and transfer properties
                    SpatialElement newSpace = doc.Create.NewSpace(currentLevel, new UV(roomPoint.Point.X, roomPoint.Point.Y));
                    newSpace.Name = roomName;
                    newSpace.Number = roomNumber;
                    newSpace.LookupParameter("Comments").Set(roomComments);
                }
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
