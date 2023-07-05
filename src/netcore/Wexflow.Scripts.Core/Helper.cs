using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Wexflow.Core.Db;

namespace Wexflow.Scripts.Core
{
    public class Helper
    {
        public static void InsertWorkflowsAndUser(Db db, string workfowsFolder)
        {
            try
            {
                var workflowFiles = Directory.GetFiles(workfowsFolder);

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
                Console.WriteLine("Workflows created.");
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

                        if (workflowId is not 170 and not 171 and not 172 and not 173 and not 174)
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

        public static void InsertRecords(Db db, string dbFolderName, string recordsFolder, string documentFile, string invoiceFile, string timesheetFile, bool isUnix = false)
        {
            Console.WriteLine("Inserting records...");

            var records = db.GetRecords(string.Empty).ToList();

            // Insert document
            if (records.All(r => r.Name != "Document"))
            {
                InsertRecord(db
                , recordsFolder
                , documentFile
                , "Document"
                , "Time card"
                , "This document needs to be completed."
                , "Please fill the document."
                , true
                , dbFolderName
                , isUnix);
            }

            // Insert invoice
            if (records.All(r => r.Name != "Invoice"))
            {
                InsertRecord(db
                , recordsFolder
                , invoiceFile
                , "Invoice"
                , "Invoice Payments Report by Agency - July 2013 to June 2014"
                , "This document needs to be reviewed."
                , "Please complete the document."
                , true
                , dbFolderName
                , isUnix);
            }

            // Insert timesheet
            if (records.All(r => r.Name != "Timesheet"))
            {
                InsertRecord(db
                , recordsFolder
                , timesheetFile
                , "Timesheet"
                , "Time Sheet"
                , "This document needs to be completed."
                , "Please fill the document."
                , true
                , dbFolderName
                , isUnix);
            }

            // Insert vacation request
            if (records.All(r => r.Name != "Vacations"))
            {
                InsertRecord(db
                , recordsFolder
                , string.Empty
                , "Vacations"
                , "Vacations request"
                , string.Empty
                , string.Empty
                , false
                , dbFolderName
                , isUnix);
            }
        }

        private static void InsertRecord(Db db, string recordsFolder, string recordSrc, string name, string desc, string comments, string managerComments, bool hasFile, string dbFolderName, bool isUnix)
        {
            try
            {
                var admin = db.GetUser("admin");
                var wexflow = db.GetUser("wexflow");

                Record record = new()
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
                    Wexflow.Core.Db.Version recordVersion = new()
                    {
                        RecordId = recordId
                    };
                    var recordVersionId = db.InsertVersion(recordVersion);

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
                    recordVersion.FilePath = isUnix ? $"{recordsFolder}/{dbFolderName}/{recordId}/{recordVersionId}/{recordFileName}" : recordFilePath;
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
