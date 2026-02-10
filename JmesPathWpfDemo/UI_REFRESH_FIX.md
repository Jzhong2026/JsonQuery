# UI 刷新修复说明

## 问题描述
配置数组排序后，TreeView 中的节点显示文本没有立即更新，需要手动刷新才能看到排序信息。

## 根本原因
虽然 `JsonTreeNode` 的 `SortKey` 和 `SortAscending` 属性在 setter 中会触发 `PropertyChanged` 事件，但是在 ViewModel 中通过对话框设置这些属性后，UI 绑定没有正确响应更新。

## 解决方案

### 1. 公开 `OnPropertyChanged` 方法
**文件**: `JmesPathWpfDemo\Models\JsonTreeNode.cs`

将 `OnPropertyChanged` 方法从 `protected` 改为 `public`：

```csharp
public void OnPropertyChanged([CallerMemberName] string propertyName = null)
{
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
```

**原因**: 允许 ViewModel 在必要时手动触发属性更新通知。

### 2. 增强 `ConfigureArraySort` 方法
**文件**: `JmesPathWpfDemo\ViewModels\ShellViewModel.cs`

在配置排序后添加以下代码：

```csharp
if (dialog.ShowDialog() == true)
{
    node.SortKey = dialog.SelectedSortKey;
    node.SortAscending = dialog.SortAscending;
    
    // 手动触发 UI 更新
    node.OnPropertyChanged(nameof(node.SortKey));
    node.OnPropertyChanged(nameof(node.SortAscending));
    node.OnPropertyChanged(nameof(node.DisplayText));
    node.OnPropertyChanged(nameof(node.HasSortApplied));
    
    // 如果节点被选中，更新查询
    if (node.IsSelected)
    {
        UpdateQueryWithNodeSort(node);
    }
}
```

**改进**:
- ? 添加对话框的 `Owner` 设置，使对话框居中显示在主窗口上
- ? 显式触发所有相关属性的 `PropertyChanged` 事件
- ? 仅在节点被选中时更新查询，避免不必要的查询更新

### 3. 自动属性更新机制
**文件**: `JmesPathWpfDemo\Models\JsonTreeNode.cs`

`SortKey` 和 `SortAscending` 的 setter 已经实现了级联更新：

```csharp
public string SortKey
{
    get => _sortKey;
    set
    {
        if (_sortKey != value)
        {
            _sortKey = value;
            OnPropertyChanged();                      // 通知 SortKey 改变
            OnPropertyChanged(nameof(DisplayText));   // 通知 DisplayText 改变
            OnPropertyChanged(nameof(HasSortApplied)); // 通知 HasSortApplied 改变
        }
    }
}
```

## UI 更新流程

### 配置排序时的更新流程：
1. 用户右键点击数组节点 → "Configure Array Sort..."
2. 打开 `ArraySortDialog` 对话框
3. 用户选择排序字段和方向 → 点击 OK
4. ViewModel 更新节点的 `SortKey` 和 `SortAscending` 属性
5. **手动触发** `PropertyChanged` 事件（确保 UI 更新）
6. TreeView 接收到通知，重新渲染节点的 `DisplayText`
7. 节点显示更新为：`people: Array [Sorted by age ↑]`

### 清除排序时的更新流程：
1. 用户右键点击已排序的数组节点 → "Clear Sort"
2. 直接设置 `node.SortKey = null` 和 `node.SortAscending = true`
3. 属性的 setter **自动触发** `PropertyChanged` 事件
4. TreeView 接收到通知，重新渲染节点
5. 节点显示恢复为：`people: Array`

## 测试验证

### 测试场景 1：配置数组排序
1. 启动应用程序
2. 切换到 "Tree View" 标签页
3. 右键点击 `people` 数组节点
4. 选择 "Configure Array Sort..."
5. 选择 `age` 字段，Ascending 方向
6. 点击 OK
7. **期望**: 节点立即显示 `people: Array [Sorted by age ↑]`

### 测试场景 2：清除排序
1. 在已排序的 `people` 节点上右键
2. 选择 "Clear Sort"
3. **期望**: 节点立即恢复为 `people: Array`

### 测试场景 3：排序并选中
1. 配置 `people` 数组按 `age` 降序排序
2. 启用节点选择（点击 "Enable Selection"）
3. 点击 `people` 节点
4. **期望**: 
   - 节点显示 `people: Array [Sorted by age ↓]`
   - Query 显示 `people | sort_by(@, &age) | reverse(@)`

## 关键改进总结

| 改进项 | 说明 | 影响 |
|--------|------|------|
| 公开 `OnPropertyChanged` | 允许外部手动触发属性通知 | ViewModel 可以确保 UI 更新 |
| 对话框 Owner 设置 | 对话框居中于主窗口 | 更好的用户体验 |
| 显式属性通知 | 配置排序后手动触发所有相关属性更新 | 确保 TreeView 立即刷新显示 |
| 条件查询更新 | 仅在节点被选中时更新查询 | 避免不必要的查询更改 |

## 技术要点

### WPF 数据绑定刷新机制
- **自动更新**: 属性 setter 中的 `OnPropertyChanged()` 会自动通知 UI
- **手动更新**: 在某些复杂场景下，需要显式调用 `OnPropertyChanged(propertyName)` 确保更新
- **级联更新**: 一个属性改变可能影响多个计算属性（如 `DisplayText`），需要通知所有相关属性

### INotifyPropertyChanged 最佳实践
1. 在属性 setter 中实现自动通知
2. 对于计算属性，在依赖属性改变时通知
3. 必要时提供公开的 `OnPropertyChanged` 方法供外部调用

