# 排序功能测试指南

## 问题诊断

根据截图，您遇到的问题是：
1. ? 树显示了 `[Sorted by age ↑]` - 说明排序信息已保存
2. ? 但 Query 没有包含 sort_by 表达式 - 这是因为配置排序时节点没有被选中

## 修复内容

我已经修改了 `ConfigureArraySort` 方法，现在它会：

### 1. 自动选中节点并更新查询
```csharp
// 配置排序后，如果启用了选择，自动选中节点并更新查询
if (IsNodeSelectionEnabled)
{
    node.IsSelected = true;
    _currentSelectedNode = node;
    UpdateQueryWithNodeSort(node);  // 自动生成带排序的查询
}
```

### 2. 提供用户提示
如果选择功能未启用，会显示提示告诉用户如何使用：
```
Sort configured successfully!

To use the sorted query:
1. Click 'Enable Selection'
2. Click on the array node
3. The query will include the sort expression
```

### 3. 增强调试输出
添加了详细的调试信息，可以在 Visual Studio 的 Output 窗口查看：
```
Before: SortKey=age, SortAscending=True
After: SortKey=age, SortAscending=False
DisplayText: people: Array [Sorted by age ↓]
```

## 测试步骤

### 方法 1：停止并重新运行应用
1. **停止当前运行的应用程序** (按 Shift+F5 或点击停止按钮)
2. **清理项目**:
   ```powershell
   cd "C:\Users\jason\source\repos\JsonQuery"
   Remove-Item -Recurse -Force "JmesPathWpfDemo\obj", "JmesPathWpfDemo\bin"
   ```
3. **重新构建**: 在 Visual Studio 中点击 Build → Rebuild Solution
4. **运行应用程序** (F5)

### 方法 2：使用 Hot Reload (如果支持)
1. 保存所有文件 (Ctrl+Shift+S)
2. 等待 Hot Reload 自动应用更改
3. 如果没有自动应用，点击 Hot Reload 按钮

## 正确的使用流程

### 场景 A：启用选择后配置排序
1. 点击 **"Enable Selection"** 按钮
2. 右键点击 `people` 数组节点
3. 选择 **"Configure Array Sort..."**
4. 选择排序字段（如 `age`），选择方向（Ascending/Descending）
5. 点击 **OK**
6. **期望结果**：
   - ? 节点自动被选中（黄色背景）
   - ? Query 自动更新为：`people | sort_by(@, &age)` (升序) 或 `people | sort_by(@, &age) | reverse(@)` (降序)
   - ? 节点显示：`people: Array [Sorted by age ↑]` 或 `↓`

### 场景 B：未启用选择时配置排序
1. 右键点击 `people` 数组节点
2. 选择 **"Configure Array Sort..."**
3. 配置排序
4. 点击 **OK**
5. **期望结果**：
   - ? 显示提示对话框，说明如何使用
   - ? 节点显示排序信息：`people: Array [Sorted by age ↑]`
   - ? 按照提示操作后，Query 会更新

### 场景 C：修改已有的排序
1. 右键点击已排序的数组节点（显示 `[Sorted by age ↑]`）
2. 选择 **"Configure Array Sort..."**
3. 修改排序方向为 **Descending**
4. 点击 **OK**
5. **期望结果**：
   - ? 节点显示更新为：`people: Array [Sorted by age ↓]`
   - ? Query 更新为：`people | sort_by(@, &age) | reverse(@)`

### 场景 D：清除排序
1. 右键点击已排序的数组节点
2. 选择 **"Clear Sort"**
3. **期望结果**：
   - ? 节点显示恢复为：`people: Array`
   - ? Query 恢复为：`people`（如果节点被选中）

## 验证排序是否生效

### 查看 Output 窗口
在 Visual Studio 中：
1. 打开 **View → Output** (或 Ctrl+Alt+O)
2. 在下拉列表中选择 **"Debug"**
3. 配置排序时应该看到：
   ```
   Before: SortKey=, SortAscending=True
   SortKey set to: age for node: people
   After: SortKey=age, SortAscending=True
   DisplayText: people: Array [Sorted by age ↑]
   ```

### 执行查询验证
1. 配置排序后，Query 应该显示：`people | sort_by(@, &age)`
2. 点击 **Execute** 按钮
3. 查看 **Result** - 应该显示排序后的结果

## 故障排除

### 问题：配置排序后 UI 没有更新
**解决方案**：
- 检查 Output 窗口的调试信息
- 确认 `SortKey set to: ...` 消息出现
- 尝试点击 "Refresh Tree" 按钮

### 问题：Query 没有包含 sort_by
**原因**：配置排序时选择功能未启用，且节点未被选中

**解决方案**：
1. 点击 **"Enable Selection"**
2. 点击已配置排序的数组节点
3. Query 应该自动更新

或者：
1. 重新右键配置排序
2. 在启用选择的情况下点击 OK
3. 会自动选中并更新 Query

### 问题：构建失败 "XML document must contain a root level element"
**解决方案**：
1. 完全停止应用程序
2. 删除 obj 和 bin 文件夹
3. 重新构建
4. 如果仍然失败，重启 Visual Studio

## 代码修改总结

### ShellViewModel.cs - ConfigureArraySort 方法
```csharp
// 关键改进
if (dialog.ShowDialog() == true)
{
    // 更新排序属性
    node.SortKey = dialog.SelectedSortKey;
    node.SortAscending = dialog.SortAscending;
    
    // 自动选中并更新查询（如果启用了选择）
    if (IsNodeSelectionEnabled)
    {
        node.IsSelected = true;
        _currentSelectedNode = node;
        UpdateQueryWithNodeSort(node);  // ← 这里会生成带排序的查询
    }
}
```

### JsonTreeNode.cs - SortKey/SortAscending 属性
```csharp
public string SortKey
{
    set
    {
        _sortKey = value;
        // 总是触发 PropertyChanged，即使值相同
        OnPropertyChanged();
        OnPropertyChanged(nameof(DisplayText));  // 刷新显示
        OnPropertyChanged(nameof(HasSortApplied));
    }
}
```

## 预期行为对比

### 修复前
```
1. 右键配置排序 → OK
2. 节点可能显示排序信息（缓存）
3. Query 不变：people[0].profile.firstName ?
4. 需要手动选中节点才能更新 Query
```

### 修复后
```
1. 启用选择
2. 右键配置排序 → OK
3. 节点自动选中，显示：people: Array [Sorted by age ↑] ?
4. Query 自动更新：people | sort_by(@, &age) ?
5. 可以直接点击 Execute 看结果
```

