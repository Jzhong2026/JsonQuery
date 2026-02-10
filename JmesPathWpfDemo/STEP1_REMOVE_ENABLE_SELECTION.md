# 步骤 1: 移除 Enable Selection 按钮

## XAML 修改

### 在 ShellView.xaml 中找到这段代码（大约在第 157-177 行）：

```xaml
<StackPanel Grid.Column="2" 
            Orientation="Horizontal" 
            HorizontalAlignment="Right">
    <Button Content="Refresh Tree" 
            Width="100" 
            Height="28"
            Margin="0,0,10,0"
            Foreground="White"
            FontWeight="Bold"
            BorderThickness="0"
            Cursor="Hand"
            cal:Message.Attach="RefreshTree">
        <Button.Style>
            <Style TargetType="Button">
                <Setter Property="Background" Value="#28A745"/>
                <Style.Triggers>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter Property="Background" Value="#218838"/>
                    </Trigger>
                </Style.Triggers>
            </Style>
        </Button.Style>
    </Button>
    <Button Content="{Binding NodeSelectionButtonText}"    ← 删除这个按钮
            Width="140" 
            Height="28"
            Foreground="White"
            FontWeight="Bold"
            BorderThickness="0"
            Cursor="Hand"
            cal:Message.Attach="ToggleNodeSelection">
        <Button.Style>
            <Style TargetType="Button">
                <Setter Property="Background" Value="#6C757D"/>
                <Style.Triggers>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter Property="Background" Value="#5A6268"/>
                    </Trigger>
                </Style.Triggers>
            </Style>
        </Button.Style>
    </Button>
</StackPanel>
```

### 替换为：

```xaml
<StackPanel Grid.Column="2" 
            Orientation="Horizontal" 
            HorizontalAlignment="Right">
    <Button Content="Refresh Tree" 
            Width="100" 
            Height="28"
            Foreground="White"
            FontWeight="Bold"
            BorderThickness="0"
            Cursor="Hand"
            cal:Message.Attach="RefreshTree">
        <Button.Style>
            <Style TargetType="Button">
                <Setter Property="Background" Value="#28A745"/>
                <Style.Triggers>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter Property="Background" Value="#218838"/>
                    </Trigger>
                </Style.Triggers>
            </Style>
        </Button.Style>
    </Button>
</StackPanel>
```

**主要变化**：
1. ? 删除整个 "Enable Selection" 按钮
2. ? 删除 Refresh Tree 按钮的 `Margin="0,0,10,0"`（因为右边没有按钮了）

---

## ViewModel 修改

### 在 ShellViewModel.cs 中

#### 1. 移除字段
找到并**删除**：
```csharp
private bool _isNodeSelectionEnabled;
```

#### 2. 移除属性
找到并**删除**：
```csharp
public bool IsNodeSelectionEnabled
{
    get => _isNodeSelectionEnabled;
    set
    {
        _isNodeSelectionEnabled = value;
        NotifyOfPropertyChange(() => IsNodeSelectionEnabled);
        NotifyOfPropertyChange(() => NodeSelectionButtonText);
        
        // Clear selection when disabling
        if (!value && _currentSelectedNode != null)
        {
            _currentSelectedNode.IsSelected = false;
            _currentSelectedNode = null;
        }
    }
}

public string NodeSelectionButtonText => IsNodeSelectionEnabled ? "Disable Selection" : "Enable Selection";
```

#### 3. 移除方法
找到并**删除**：
```csharp
public void ToggleNodeSelection()
{
    IsNodeSelectionEnabled = !IsNodeSelectionEnabled;
}
```

#### 4. 更新 ConfigureArraySort 方法
找到这个方法中的代码：
```csharp
// Auto-select the node and update query if selection is enabled
if (IsNodeSelectionEnabled)
{
    // Deselect previous node
    if (_currentSelectedNode != null && _currentSelectedNode != node)
    {
        _currentSelectedNode.IsSelected = false;
    }
    
    // Select this node
    node.IsSelected = true;
    _currentSelectedNode = node;
    
    // Update query with sort
    UpdateQueryWithNodeSort(node);
}
else
{
    // If selection is not enabled, show message to user
    MessageBox.Show(...);
}
```

替换为：
```csharp
// Auto-select the node and update query (selection is always enabled)
// Deselect previous node
if (_currentSelectedNode != null && _currentSelectedNode != node)
{
    _currentSelectedNode.IsSelected = false;
}

// Select this node
node.IsSelected = true;
_currentSelectedNode = node;

// Update query with sort
UpdateQueryWithNodeSort(node);
```

#### 5. 更新 OnNodeSelected 方法
找到这个方法开头的代码：
```csharp
public void OnNodeSelected(JsonTreeNode node)
{
    if (!IsNodeSelectionEnabled || node == null)
        return;
    
    // ... 其余代码
}
```

替换为：
```csharp
public void OnNodeSelected(JsonTreeNode node)
{
    if (node == null)
        return;
    
    // ... 其余代码
}
```

---

## 修改总结

### XAML 更改（ShellView.xaml）
- ? 删除 "Enable Selection" 按钮
- ? 保留 "Refresh Tree" 按钮

### ViewModel 更改（ShellViewModel.cs）
- ? 删除 `_isNodeSelectionEnabled` 字段
- ? 删除 `IsNodeSelectionEnabled` 属性
- ? 删除 `NodeSelectionButtonText` 属性
- ? 删除 `ToggleNodeSelection()` 方法
- ? 更新 `ConfigureArraySort()` 方法 - 移除 `if (IsNodeSelectionEnabled)` 检查
- ? 更新 `OnNodeSelected()` 方法 - 移除 `!IsNodeSelectionEnabled` 检查

### 结果
- ? 节点选择功能**始终启用**
- ? 点击任何节点都会立即更新 Query
- ? 不需要额外的启用/禁用操作
- ? 界面更简洁

---

## 测试清单

完成修改后，测试以下功能：

### 测试 1: 直接点击节点
```
1. 启动应用
2. 切换到 Tree View
3. 直接点击任意节点（例如 UserDefinedFields[0]）
4. ? 验证：Query 自动更新
5. ? 验证：节点高亮显示（黄色背景）
```

### 测试 2: 数组排序
```
1. 右键点击数组节点
2. "Configure Array Sort..."
3. 配置排序
4. ? 验证：节点自动选中
5. ? 验证：Query 包含排序表达式
```

### 测试 3: 切换节点
```
1. 点击节点 A
2. ? 验证：节点 A 高亮
3. 点击节点 B
4. ? 验证：节点 A 取消高亮
5. ? 验证：节点 B 高亮
6. ? 验证：Query 更新为节点 B 的路径
```

### 测试 4: 取消选择
```
1. 点击某个节点
2. 再次点击同一个节点
3. ? 验证：节点取消高亮
4. ? 验证：Query 清空
```

---

## 编译和运行

1. **保存所有文件**
2. **重新编译项目**
   ```
   dotnet build
   ```
3. **运行应用**
   ```
   dotnet run
   ```

---

## 如果遇到问题

### 编译错误
- 确保删除了所有 `IsNodeSelectionEnabled` 的引用
- 确保删除了 `NodeSelectionButtonText` 的引用
- 确保删除了 `ToggleNodeSelection` 的引用

### 运行时错误
- 如果点击节点没有反应，检查 `OnNodeSelected` 方法
- 如果排序后没有自动选中，检查 `ConfigureArraySort` 方法

---

## 完成！

完成这些修改后：
- ? "Enable Selection" 按钮已移除
- ? 选择功能始终启用
- ? 界面更简洁
- ? 用户体验更流畅

