using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Wexflow.Core.Task
{
    public abstract class Task
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        private XElement _xElement;

        public Task(XElement xe) 
        {
            this._xElement = xe;
            this.Id = int.Parse(xe.Attribute("id").Value);
            this.Name = xe.Attribute("name").Value;
            this.Description = xe.Attribute("description").Value;
            this.LoadSettings();
        }

        public abstract void LoadSettings();
        public abstract void Run();

        public string GetSetting(string name)
        {
            return this._xElement.XPathSelectElement(string.Format("Setting[@name='{0}']", name)).Attribute("value").Value;
        }
    }
}
