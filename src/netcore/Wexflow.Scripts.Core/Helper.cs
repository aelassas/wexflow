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
                string[] workflowFiles = Directory.GetFiles(workfowsFolder);

                db.Init();

                Console.WriteLine("Creating workflows...");
                foreach (string workflowFile in workflowFiles)
                {
                    XNamespace xn = "urn:wexflow-schema";
                    XDocument xdoc1 = XDocument.Load(workflowFile);
                    int workflowIdFromFile = int.Parse(xdoc1.Element(xn + "Workflow").Attribute("id").Value);

                    bool found = false;
                    System.Collections.Generic.List<Workflow> workflows = db.GetWorkflows().ToList();
                    foreach (Workflow workflow in workflows)
                    {
                        XDocument xdoc2 = XDocument.Parse(workflow.Xml);
                        int workflowId = int.Parse(xdoc2.Element(xn + "Workflow").Attribute("id").Value);
                        if (workflowIdFromFile == workflowId)
                        {
                            found = true;
                            Console.WriteLine("Workflow " + workflowIdFromFile + " already in database.");
                            break;
                        }
                    }

                    if (!found)
                    {
                        try
                        {
                            db.InsertWorkflow(new Workflow { Xml = xdoc1.ToString() });
                            Console.WriteLine("Workflow " + workflowIdFromFile + " inserted.");
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
                User user = db.GetUser("wexflow");
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

                    System.Collections.Generic.List<Workflow> workflows = db.GetWorkflows().ToList();
                    foreach (Workflow workflow in workflows)
                    {
                        XNamespace xn = "urn:wexflow-schema";
                        XDocument xdoc = XDocument.Parse(workflow.Xml);
                        int workflowId = int.Parse(xdoc.Element(xn + "Workflow").Attribute("id").Value);

                        if (workflowId != 170 && workflowId != 171 && workflowId != 172 && workflowId != 173 && workflowId != 174)
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

            System.Collections.Generic.List<Record> records = db.GetRecords(string.Empty).ToList();

            // Insert document
            if (!records.Any(r => r.Name == "Document"))
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
            if (!records.Any(r => r.Name == "Invoice"))
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
            if (!records.Any(r => r.Name == "Timesheet"))
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
            if (!records.Any(r => r.Name == "Vacations"))
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
                User admin = db.GetUser("admin");
                User wexflow = db.GetUser("wexflow");

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

                string recordId = db.InsertRecord(record);

                if (hasFile)
                {
                    Wexflow.Core.Db.Version recordVersion = new()
                    {
                        RecordId = recordId
                    };
                    string recordVersionId = db.InsertVersion(recordVersion);

                    string recordFileName = Path.GetFileName(recordSrc);
                    string recordFolder = Path.Combine(recordsFolder, dbFolderName, recordId, recordVersionId);
                    string recordFilePath = Path.Combine(recordFolder, recordFileName);
                    if (!Directory.Exists(recordFolder))
                    {
                        Directory.CreateDirectory(recordFolder);
                    }
                    if (File.Exists(recordFilePath))
                    {
                        File.Delete(recordFilePath);
                    }
                    File.Copy(recordSrc, recordFilePath, true);
                    if (isUnix)
                    {
                        recordVersion.FilePath = recordsFolder + "/" + dbFolderName + "/" + recordId + "/" + recordVersionId + "/" + recordFileName;
                    }
                    else
                    {
                        recordVersion.FilePath = recordFilePath;
                    }
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
