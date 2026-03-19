using Caliburn.Micro;
using JmesPathWpfDemo.Models;
using JmesPathWpfDemo.Services;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml;

namespace JmesPathWpfDemo.ViewModels
{
    public class XmlWorkspaceViewModel : Screen
    {
        private string _currentView = "XmlQuery";
        private string _xmlInput;
        private string _query;
        private string _result;
        private ObservableCollection<XmlTreeNode> _xmlTreeNodes;
        private readonly XmlTreeBuilder _treeBuilder = new XmlTreeBuilder();

        public XmlWorkspaceViewModel()
        {
            _xmlTreeNodes = new ObservableCollection<XmlTreeNode>();

            try
            {
                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
                var samplePath = Path.Combine(assemblyDirectory, "sample.json");

                if (!File.Exists(samplePath))
                {
                    var projectDir = Path.GetFullPath(Path.Combine(assemblyDirectory, @"..\..\.."));
                    var projectSamplePath = Path.Combine(projectDir, "sample.json");
                    if (File.Exists(projectSamplePath))
                    {
                        samplePath = projectSamplePath;
                    }
                }

                if (File.Exists(samplePath))
                {
                    var jsonStr = File.ReadAllText(samplePath);
                    // Add a root node because json usually is an object and DeserializeXmlNode requires one root
                    var doc = JsonConvert.DeserializeXmlNode(jsonStr, "Root");
                    using (var stringWriter = new StringWriter())
                    using (var xmlTextWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true }))
                    {
                        doc.WriteTo(xmlTextWriter);
                        xmlTextWriter.Flush();
                        _xmlInput = stringWriter.GetStringBuilder().ToString();
                    }
                }
                else
                {
                    _xmlInput = "<Root></Root>";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading sample.json and generating XML: {ex.Message}");
                _xmlInput = "<Root/>";
            }

            _query = "/Root";
            _result = "";
            RefreshXmlTree();
        }

        public string XmlInput
        {
            get => _xmlInput;
            set
            {
                if (_xmlInput != value)
                {
                    _xmlInput = value;
                    NotifyOfPropertyChange(() => XmlInput);
                }
            }
        }

        public string Query
        {
            get => _query;
            set
            {
                if (_query != value)
                {
                    _query = value;
                    NotifyOfPropertyChange(() => Query);
                }
            }
        }

        public string Result
        {
            get => _result;
            set
            {
                if (_result != value)
                {
                    _result = value;
                    NotifyOfPropertyChange(() => Result);
                }
            }
        }

        public ObservableCollection<XmlTreeNode> XmlTreeNodes
        {
            get => _xmlTreeNodes;
            set
            {
                _xmlTreeNodes = value;
                NotifyOfPropertyChange(() => XmlTreeNodes);
            }
        }

        public string CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                NotifyOfPropertyChange(() => CurrentView);
            }
        }

        public void OnNodeSelected(XmlTreeNode node)
        {
            if (node == null) return;
            Query = node.Path;
        }

        public void RefreshTree()
        {
            RefreshXmlTree();
        }

        private void RefreshXmlTree()
        {
            XmlTreeNodes = _treeBuilder.BuildTree(XmlInput);
        }

        public void Execute()
        {
            if (string.IsNullOrWhiteSpace(XmlInput) || string.IsNullOrWhiteSpace(Query))
            {
                Result = "Empty input or query.";
                return;
            }

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(XmlInput);

                var selectedNodes = doc.SelectNodes(Query);
                if (selectedNodes == null || selectedNodes.Count == 0)
                {
                    Result = "No elements found.";
                    return;
                }

                using (var stringWriter = new StringWriter())
                using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true }))
                {
                    xmlWriter.WriteStartElement("Results");
                    foreach (XmlNode node in selectedNodes)
                    {
                        node.WriteTo(xmlWriter);
                    }
                    xmlWriter.WriteEndElement();
                    xmlWriter.Flush();

                    Result = stringWriter.GetStringBuilder().ToString();
                }
            }
            catch (Exception ex)
            {
                Result = $"Error executing query:\n{ex.Message}";
            }
        }
    }
}