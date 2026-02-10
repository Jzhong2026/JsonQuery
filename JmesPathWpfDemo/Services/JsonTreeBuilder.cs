using JmesPathWpfDemo.Models;
using System;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace JmesPathWpfDemo.Services
{
	public class JsonTreeBuilder
	{
		public ObservableCollection<JsonTreeNode> BuildTree(string jsonText)
		{
			var nodes = new ObservableCollection<JsonTreeNode>();

			if (string.IsNullOrWhiteSpace(jsonText))
				return nodes;

			try
			{
				using var doc = JsonDocument.Parse(jsonText);
				var root = doc.RootElement;

				ProcessElement(root, nodes, "");
			}
			catch
			{
				// If parsing fails, return empty collection
			}

			return nodes;
		}

		private void ProcessElement(JsonElement element, ObservableCollection<JsonTreeNode> parentCollection, string currentPath)
		{
			switch (element.ValueKind)
			{
				case JsonValueKind.Object:
					foreach (var property in element.EnumerateObject())
					{
						var node = new JsonTreeNode
						{
							Key = property.Name,
							Path = string.IsNullOrEmpty(currentPath) ? property.Name : $"{currentPath}.{property.Name}",
							Type = GetTypeString(property.Value.ValueKind)
						};

						if (property.Value.ValueKind == JsonValueKind.Object || property.Value.ValueKind == JsonValueKind.Array)
						{
							ProcessElement(property.Value, node.Children, node.Path);
						}
						else
						{
							node.Value = GetValueString(property.Value);
						}

						parentCollection.Add(node);
					}
					break;

				case JsonValueKind.Array:
					int index = 0;
					foreach (var item in element.EnumerateArray())
					{
						var node = new JsonTreeNode
						{
							Key = $"[{index}]",
							Path = $"{currentPath}[{index}]",
							Type = GetTypeString(item.ValueKind)
						};

						if (item.ValueKind == JsonValueKind.Object || item.ValueKind == JsonValueKind.Array)
						{
							ProcessElement(item, node.Children, node.Path);
						}
						else
						{
							node.Value = GetValueString(item);
						}

						parentCollection.Add(node);
						index++;
					}
					break;

				default:
					var valueNode = new JsonTreeNode
					{
						Value = GetValueString(element),
						Path = currentPath,
						Type = GetTypeString(element.ValueKind)
					};
					parentCollection.Add(valueNode);
					break;
			}
		}

		private string GetTypeString(JsonValueKind kind)
		{
			return kind switch
			{
				JsonValueKind.Object => "Object",
				JsonValueKind.Array => "Array",
				JsonValueKind.String => "String",
				JsonValueKind.Number => "Number",
				JsonValueKind.True => "Boolean",
				JsonValueKind.False => "Boolean",
				JsonValueKind.Null => "Null",
				_ => "Unknown"
			};
		}

		private string GetValueString(JsonElement element)
		{
			return element.ValueKind switch
			{
				JsonValueKind.String => element.GetString(),
				JsonValueKind.Number => element.GetRawText(),
				JsonValueKind.True => "true",
				JsonValueKind.False => "false",
				JsonValueKind.Null => "null",
				_ => element.GetRawText()
			};
		}
	}
}
