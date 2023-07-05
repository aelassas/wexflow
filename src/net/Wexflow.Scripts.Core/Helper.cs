using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Wexflow.Core.Db;

namespace Wexflow.Scripts.Core
{
    public class Helper
    {
        public static void InsertWorkflowsAndUser(Db db)
        {
            try
            {
                var workflowFiles = Directory.GetFiles(ConfigurationManager.AppSettings["workflowsFolder"]);

                db.Init();

                Console.WriteLine("Creating workflows...");
                foreach (var workflowFile in workflowFiles)
                {
                    XNamespace xn = "urn:wexflow-schema";
                    var xdoc1 = XDocument.Load(workflowFile);
                    var workflowIdFromFile = int.Parse(xdoc1.Element(xn + "Workflow")?.Attribute("id")?.Value ?? throw new InvalidOperationException());

                    var found = false;
                    var workflows = db.GetWorkflows().ToList();
                    foreach (var workflow in workflows)
                    {
                        var xdoc2 = XDocument.Parse(workflow.Xml);
                        var workflowId = int.Parse(xdoc2.Element(xn + "Workflow")?.Attribute("id")?.Value ?? throw new InvalidOperationException());
                        if (workflowIdFromFile == workflowId)
                        {
                            found = true;
                            Console.WriteLine($"Workflow {workflowIdFromFile} already in database.");
                            break;
                        }
                    }

                    if (!found)
                    {
                        try
                        {
                            _ = db.InsertWorkflow(new Workflow { Xml = xdoc1.ToString() });
                            Console.WriteLine($"Workflow {workflowIdFromFile} inserted.");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("An error occured: {0}", e);
                        }
                    }
                }
                Console.WriteLine($"{workflowFiles.Length} Workflows created.");
                Console.WriteLine();

                Console.WriteLine("Creating wexflow user...");
                var user = db.GetUser("wexflow");
                if (user == null)
                {
                    db.InsertUser(new User
                    {
                        CreatedOn = DateTime.Now,
                        Username = "wexflow",
                        Password = Db.GetMd5("wexflow2018"),
                        UserProfile = UserProfile.Administrator
                    });
                    user = db.GetUser("wexflow");

                    var workflows = db.GetWorkflows().ToList();
                    foreach (var workflow in workflows)
                    {
                        XNamespace xn = "urn:wexflow-schema";
                        var xdoc = XDocument.Parse(workflow.Xml);
                        var workflowId = int.Parse(xdoc.Element(xn + "Workflow")?.Attribute("id")?.Value ?? throw new InvalidOperationException());

                        if (workflowId != 146 && workflowId != 147 && workflowId != 148 && workflowId != 149 && workflowId != 150)
                        {
                            db.InsertUserWorkflowRelation(new UserWorkflow
                            {
                                UserId = user.GetDbId(),
                                WorkflowId = workflow.GetDbId()
                            });
                            Console.WriteLine("UserWorkflowRelation ({0}, {1}) created.", user.GetDbId(), workflow.GetDbId());
                        }
                    }
                    Console.WriteLine("wexflow user created with success.");
                }
                else
                {
                    Console.WriteLine("The user wexflow already exists.");
                }
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured: {0}", e);
            }
        }

        public static void InsertRecords(Db db, string dbFolderName)
        {
            Console.WriteLine("Inserting records...");

            var records = db.GetRecords(string.Empty).ToList();

            // Insert document
            if (records.All(r => r.Name != "Document"))
            {
                InsertRecord(db
                    , "Document"
                    , "Time card"
                    , "This document needs to be completed."
                    , "Please fill the document."
                    , true
                    , "documentFile"
                    , dbFolderName);
            }

            // Insert invoice
            if (records.All(r => r.Name != "Invoice"))
            {
                InsertRecord(db
                , "Invoice"
                , "Invoice Payments Report by Agency - July 2013 to June 2014"
                , "This document needs to be reviewed."
                , "Please complete the document."
                , true
                , "invoiceFile"
                , dbFolderName);
            }

            // Insert timesheet
            if (records.All(r => r.Name != "Timesheet"))
            {
                InsertRecord(db
                , "Timesheet"
                , "Time Sheet"
                , "This document needs to be completed."
                , "Please fill the document."
                , true
                , "timesheetFile"
                , dbFolderName);
            }

            // Insert vacation request
            if (records.All(r => r.Name != "Vacations"))
            {
                InsertRecord(db
                , "Vacations"
                , "Vacations request"
                , string.Empty
                , string.Empty
                , false
                , string.Empty
                , dbFolderName);
            }
        }

        private static void InsertRecord(Db db, string name, string desc, string comments, string managerComments, bool hasFile, string configId, string dbFolderName)
        {
            try
            {
                var recordsFolder = ConfigurationManager.AppSettings["recordsFolder"];
                var admin = db.GetUser("admin");
                var wexflow = db.GetUser("wexflow");

                var record = new Record
                {
                    Name = name,
                    Description = desc,
                    Approved = false,
                    AssignedOn = DateTime.Now,
                    AssignedTo = wexflow.GetDbId(),
                    Comments = comments,
                    ManagerComments = managerComments,
                    CreatedBy = admin.GetDbId(),
                    StartDate = DateTime.Now.AddDays(10),
                    EndDate = DateTime.Now.AddDays(30)
                };

                var recordId = db.InsertRecord(record);

                if (hasFile)
                {
                    var recordVersion = new Wexflow.Core.Db.Version
                    {
                        RecordId = recordId
                    };
                    var recordVersionId = db.InsertVersion(recordVersion);

                    var recordSrc = ConfigurationManager.AppSettings[configId];
                    var recordFileName = Path.GetFileName(recordSrc);
                    var recordFolder = Path.Combine(recordsFolder, dbFolderName, recordId, recordVersionId);
                    var recordFilePath = Path.Combine(recordFolder, recordFileName);
                    if (!Directory.Exists(recordFolder))
                    {
                        _ = Directory.CreateDirectory(recordFolder);
                    }
                    if (File.Exists(recordFilePath))
                    {
                        File.Delete(recordFilePath);
                    }
                    File.Copy(recordSrc, recordFilePath, true);
                    recordVersion.FilePath = recordFilePath;
                    db.UpdateVersion(recordVersionId, recordVersion);
                }

                Console.WriteLine($"Record {name} inserted: {recordId}");
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured while creating the record {0}: {1}", name, e);
            }
        }
    }
}
