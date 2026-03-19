using JmesPathWpfDemo.Models;
using System;
using System.Collections.ObjectModel;
using System.Xml;

namespace JmesPathWpfDemo.Services
{
    public class XmlTreeBuilder
    {
        public ObservableCollection<XmlTreeNode> BuildTree(string xmlString)
        {
            var nodes = new ObservableCollection<XmlTreeNode>();
            if (string.IsNullOrWhiteSpace(xmlString))
            {
                return nodes;
            }

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xmlString);

                if (doc.DocumentElement != null)
                {
                    nodes.Add(ProcessElement(doc.DocumentElement, ""));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to build XML tree: {ex.Message}");
            }

            return nodes;
        }

        private XmlTreeNode ProcessElement(XmlNode element, string currentPath)
        {
            var nodePath = string.IsNullOrEmpty(currentPath) ? $"/{element.Name}" : $"{currentPath}/{element.Name}";
            
            var node = new XmlTreeNode
            {
                Name = element.Name,
                Path = nodePath,
                Value = element.ChildNodes.Count == 1 && element.FirstChild.NodeType == XmlNodeType.Text ? element.InnerText : string.Empty
            };

            if (element.Attributes != null)
            {
                foreach (XmlAttribute attr in element.Attributes)
                {
                    var attrPath = $"{nodePath}/@{attr.Name}";
                    node.Attributes.Add(new XmlTreeNode
                    {
                        Name = $"@{attr.Name}",
                        Value = attr.Value,
                        Path = attrPath
                    });
                }
            }

            foreach (XmlNode child in element.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    node.Children.Add(ProcessElement(child, nodePath));
                }
            }

            return node;
        }
    }
}