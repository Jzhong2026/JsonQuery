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
            string nodePathPart = element.Name;
            if (element.ParentNode != null)
            {
                int sameNameCount = 0;
                int myIndex = 0;
                foreach (XmlNode sibling in element.ParentNode.ChildNodes)
                {
                    if (sibling.NodeType == XmlNodeType.Element && sibling.Name == element.Name)
                    {
                        sameNameCount++;
                        if (sibling == element)
                        {
                            myIndex = sameNameCount;
                        }
                    }
                }

                if (sameNameCount > 1)
                {
                    nodePathPart = $"{element.Name}[{myIndex}]";
                }
            }

            var nodePath = string.IsNullOrEmpty(currentPath) ? $"/{nodePathPart}" : $"{currentPath}/{nodePathPart}";
            
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
                        Path = attrPath,
                        Parent = node
                    });
                }
            }

            foreach (XmlNode child in element.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    var childNode = ProcessElement(child, nodePath);
                    childNode.Parent = node;
                    node.Children.Add(childNode);
                }
            }

            return node;
        }
    }
}