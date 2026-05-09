using JmesPathWpfDemo.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
                        Path = attrPath,
                        Parent = node
                    });
                }
            }

            // Group child elements by name to detect arrays
            var childElements = new List<XmlNode>();
            foreach (XmlNode child in element.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    childElements.Add(child);
                }
            }

            var grouped = childElements.GroupBy(c => c.Name).ToList();

            foreach (var group in grouped)
            {
                var items = group.ToList();
                if (items.Count > 1)
                {
                    // Create a virtual array grouping node
                    var arrayPath = $"{nodePath}/{group.Key}";
                    var arrayNode = new XmlTreeNode
                    {
                        Name = group.Key,
                        Path = arrayPath,
                        IsArrayNode = true,
                        IsExpanded = true,
                        Parent = node
                    };

                    for (int i = 0; i < items.Count; i++)
                    {
                        var itemPath = $"{arrayPath}[{i + 1}]";
                        var childNode = ProcessElement(items[i], "");
                        childNode.Name = $"[{i + 1}]";
                        childNode.Path = itemPath;
                        childNode.Parent = arrayNode;
                     RebuildNodePaths(childNode);
                        arrayNode.Children.Add(childNode);
                    }

                    node.Children.Add(arrayNode);
                }
                else
                {
                    var childNode = ProcessElement(items[0], nodePath);
                    childNode.Parent = node;
                    node.Children.Add(childNode);
                }
            }

            return node;
        }

        private void RebuildNodePaths(XmlTreeNode node)
        {
            if (node == null)
            {
                return;
            }

            foreach (var attribute in node.Attributes)
            {
                attribute.Parent = node;
                attribute.Path = $"{node.Path}/@{attribute.Name.TrimStart('@')}";
            }

            foreach (var child in node.Children)
            {
                child.Parent = node;
                if (child.IsArrayNode)
                {
                    child.Path = $"{node.Path}/{child.Name}";
                }
                else if (node.IsArrayNode && child.Name.StartsWith("[") && child.Name.EndsWith("]"))
                {
                    child.Path = $"{node.Path}{child.Name}";
                }
                else
                {
                    child.Path = $"{node.Path}/{child.Name}";
                }

                RebuildNodePaths(child);
            }
        }
    }
}