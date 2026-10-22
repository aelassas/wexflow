using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wexflow.Core;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class WorkflowVariableSubstitutionTests
    {
        [TestMethod]
        public void Parse_ReplacesOnlyExactGlobalLocalAndRestVariableNames()
        {
            var repoRoot = FindRepositoryRoot();
            var xsdPath = Path.Combine(repoRoot, "src", "netcore", "Wexflow.Core", "Workflow.xsd");
            var globalVariablesPath = Path.Combine(
                repoRoot,
                "tests",
                "netcore",
                "Wexflow.NetCore.Tests",
                "Fixtures",
                "GlobalVariables.VariableSubstitution.xml");
            var constructorXml = """
                <Workflow xmlns="urn:wexflow-schema" id="1" name="Synthetic" description="Parser-only">
                  <Settings>
                    <Setting name="launchType" value="Trigger" />
                    <Setting name="enabled" value="true" />
                  </Settings>
                  <Tasks />
                </Workflow>
                """;

            var workflow = new Workflow(
                null!,
                0,
                new ConcurrentDictionary<System.Guid, Workflow>(),
                "synthetic-empty-db",
                constructorXml,
                repoRoot,
                repoRoot,
                repoRoot,
                xsdPath,
                null!,
                globalVariablesPath);
            workflow.RestVariables.Add(new Variable { Key = "rest", Value = "shortRest" });
            workflow.RestVariables.Add(new Variable { Key = "restLong", Value = "longRest" });

            var sourceXml = """
                <Workflow xmlns="urn:wexflow-schema">
                  <LocalVariables>
                    <Variable name="local" value="shortLocal" />
                    <Variable name="localLong" value="longLocal" />
                  </LocalVariables>
                  <Values global="$nameLong" local="$localLong" rest="$restLong" literal="$cash" unknown="$nameLongSuffix" />
                </Workflow>
                """;
            var parse = typeof(Workflow).GetMethod("Parse", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(parse, "Workflow.Parse was not found.");
            var parsedXml = (string?)parse.Invoke(workflow, [sourceXml]);
            Assert.IsNotNull(parsedXml, "Workflow.Parse returned null.");

            var values = XDocument.Parse(parsedXml).Root?.Element(XName.Get("Values", "urn:wexflow-schema"));
            Assert.IsNotNull(values, "Parsed Values element was not found.");
            Assert.AreEqual("longGlobal", (string?)values.Attribute("global"));
            Assert.AreEqual("longLocal", (string?)values.Attribute("local"));
            Assert.AreEqual("longRest", (string?)values.Attribute("rest"));
            Assert.AreEqual("price=$1", (string?)values.Attribute("literal"));
            Assert.AreEqual("$nameLongSuffix", (string?)values.Attribute("unknown"));
        }

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "Wexflow.sln")))
                {
                    return directory.FullName;
                }

                var checkout = Path.Combine(directory.FullName, "checkout");
                if (File.Exists(Path.Combine(checkout, "Wexflow.sln")))
                {
                    return checkout;
                }

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("Wexflow.sln was not found.");
        }
    }
}
