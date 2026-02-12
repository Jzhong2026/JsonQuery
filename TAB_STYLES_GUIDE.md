# Tab Styles Guide - 美化 Tab 界面

## ?? 新增的 Tab 样式

### 1. **ModernTabControlStyle**
现代化的 TabControl 容器样式

```xaml
<TabControl Style="{StaticResource ModernTabControlStyle}"
            ItemsSource="{Binding Items}">
</TabControl>
```

**特性:**
- 白色背景
- 浅灰色边框 (#DEE2E6)
- 顶部边框突出显示
- 无内边距，更紧凑

---

### 2. **ModernTabItemStyle**
用于内部 TabItem，如 "Text Input" 和 "Tree View"

```xaml
<TabControl Style="{StaticResource ModernTabControlStyle}">
    <TabControl.ItemContainerStyle>
        <Style TargetType="TabItem" BasedOn="{StaticResource ModernTabItemStyle}"/>
    </TabControl.ItemContainerStyle>
    <TabItem Header="Tab 1">
        <!-- Content -->
    </TabItem>
</TabControl>
```

**视觉效果:**
- **未选中**: 透明背景，深灰色文字 (#495057)
- **悬停**: 浅灰色背景 (#F8F9FA)
- **选中**: 
  - 白色背景
  - 蓝色文字 (#007BFF)
  - 底部蓝色下划线（3px）
  - 加粗字体

**特性:**
- 圆角顶部 (5px,5px,0,0)
- 适中的内边距 (16,10)
- 平滑的悬停过渡

---

### 3. **QueryTabStyle**
用于主查询 Tab，支持关闭按钮

```xaml
<TabControl.ItemContainerStyle>
    <Style TargetType="TabItem" BasedOn="{StaticResource QueryTabStyle}"/>
</TabControl.ItemContainerStyle>
```

**视觉效果:**
- **未选中**: 透明背景
- **悬停**: 浅灰色背景 (#E9ECEF)
- **选中**: 
  - 白色背景
  - 深色文字 (#212529)
  - 半粗体
  - 边框环绕 (除底部)

**特性:**
- 更大的圆角 (8px,8px,0,0)
- 适合包含关闭按钮的 Tab
- 卡片式设计

---

## ?? 应用位置

### 主 Shell 界面
**文件**: `ShellView.xaml`

```xaml
<!-- 主查询 Tab 控件 -->
<TabControl ItemsSource="{Binding JsonQueryTabs}"
            Style="{StaticResource ModernTabControlStyle}"
            Background="#F8F9FA">
    <TabControl.ItemContainerStyle>
        <Style TargetType="TabItem" BasedOn="{StaticResource QueryTabStyle}"/>
    </TabControl.ItemContainerStyle>
    <!-- Tab 项包含关闭按钮 -->
</TabControl>
```

### JSON Query Tab 视图
**文件**: `JsonQueryTabView.xaml`

```xaml
<!-- Text Input / Tree View Tab -->
<TabControl Style="{StaticResource ModernTabControlStyle}">
    <TabControl.ItemContainerStyle>
        <Style TargetType="TabItem" BasedOn="{StaticResource ModernTabItemStyle}"/>
    </TabControl.ItemContainerStyle>
    <TabItem Header="Text Input">...</TabItem>
    <TabItem Header="Tree View">...</TabItem>
</TabControl>
```

---

## ?? 关闭按钮美化

### 之前
```xaml
<Button Content="&#x00D7;"/>
```

### 之后
```xaml
<Button Width="18" Height="18">
    <Button.Content>
        <TextBlock Text="?" FontSize="12"/>
    </Button.Content>
    <Button.Style>
        <Style TargetType="Button">
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Foreground" Value="#6C757D"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="Button">
                        <Border Background="{TemplateBinding Background}"
                               CornerRadius="9">
                            <ContentPresenter/>
                        </Border>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
            <Style.Triggers>
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Background" Value="#DC3545"/>
                    <Setter Property="Foreground" Value="White"/>
                </Trigger>
            </Style.Triggers>
        </Style>
    </Button.Style>
</Button>
```

**改进:**
- ? 圆形按钮 (CornerRadius="9")
- ? 悬停时变为红色圆圈
- ? 白色 X 标记在红色背景上
- ? 平滑的视觉反馈

---

## ?? 配色方案

### TabControl 容器
| 元素 | 颜色 | 说明 |
|------|------|------|
| 背景 | `#F8F9FA` | 浅灰色背景 |
| 边框 | `#DEE2E6` | 分隔线颜色 |

### ModernTabItemStyle
| 状态 | 背景 | 文字 | 边框 |
|------|------|------|------|
| 正常 | Transparent | `#495057` | 无 |
| 悬停 | `#F8F9FA` | `#495057` | 无 |
| 选中 | White | `#007BFF` | `#007BFF` (底部3px) |

### QueryTabStyle
| 状态 | 背景 | 文字 | 边框 |
|------|------|------|------|
| 正常 | Transparent | `#495057` | 无 |
| 悬停 | `#E9ECEF` | `#495057` | 无 |
| 选中 | White | `#212529` | `#DEE2E6` (环绕) |

### 关闭按钮
| 状态 | 背景 | 文字 |
|------|------|------|
| 正常 | Transparent | `#6C757D` |
| 悬停 | `#DC3545` (红色) | White |

---

## ?? 使用示例

### 基本 Tab
```xaml
<TabControl Style="{StaticResource ModernTabControlStyle}">
    <TabControl.ItemContainerStyle>
        <Style TargetType="TabItem" BasedOn="{StaticResource ModernTabItemStyle}"/>
    </TabControl.ItemContainerStyle>
    <TabItem Header="First Tab">
        <TextBlock Text="Content 1"/>
    </TabItem>
    <TabItem Header="Second Tab">
        <TextBlock Text="Content 2"/>
    </TabItem>
</TabControl>
```

### 带关闭按钮的 Tab
```xaml
<TabControl Style="{StaticResource ModernTabControlStyle}"
            ItemsSource="{Binding Tabs}">
    <TabControl.ItemContainerStyle>
        <Style TargetType="TabItem" BasedOn="{StaticResource QueryTabStyle}"/>
    </TabControl.ItemContainerStyle>
    <TabControl.ItemTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal">
                <TextBlock Text="{Binding Title}"/>
                <Button Content="?" 
                        Visibility="{Binding CanClose, Converter={StaticResource BooleanToVisibilityConverter}}"
                        Command="{Binding CloseCommand}"/>
            </StackPanel>
        </DataTemplate>
    </TabControl.ItemTemplate>
</TabControl>
```

---

## ?? 迁移步骤

### 1. 确保主题已引用
检查 `App.xaml`:
```xaml
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="Themes/ButtonStyles.xaml"/>
</ResourceDictionary.MergedDictionaries>
```

### 2. 更新现有 TabControl
**之前:**
```xaml
<TabControl ItemsSource="{Binding Items}">
```

**之后:**
```xaml
<TabControl ItemsSource="{Binding Items}"
            Style="{StaticResource ModernTabControlStyle}">
    <TabControl.ItemContainerStyle>
        <Style TargetType="TabItem" BasedOn="{StaticResource ModernTabItemStyle}"/>
    </TabControl.ItemContainerStyle>
```

### 3. 美化关闭按钮
参考上面的关闭按钮样式代码

---

## ? 视觉对比

### 之前
- ? 默认灰色 Tab
- ? 方形边角
- ? 简单的文字关闭按钮
- ? 无悬停效果

### 之后
- ? 现代化配色
- ? 圆角设计
- ? 圆形红色关闭按钮
- ? 平滑的悬停过渡
- ? 选中状态明显
- ? 蓝色下划线/边框高亮

---

## ?? 完整示例

### ShellView.xaml 主 Tab
```xaml
<TabControl ItemsSource="{Binding JsonQueryTabs}"
            SelectedItem="{Binding SelectedJsonQueryTab}"
            Style="{StaticResource ModernTabControlStyle}"
            Background="#F8F9FA"
            BorderBrush="#DEE2E6"
            BorderThickness="0,2,0,0">
    <TabControl.ItemContainerStyle>
        <Style TargetType="TabItem" BasedOn="{StaticResource QueryTabStyle}"/>
    </TabControl.ItemContainerStyle>
    <TabControl.ItemTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal">
                <TextBlock Text="{Binding TabTitle}" 
                           Margin="0,0,8,0"
                           VerticalAlignment="Center"/>
                <Button Width="18" Height="18"
                        Visibility="{Binding CanClose, Converter={StaticResource BooleanToVisibilityConverter}}">
                    <Button.Content>
                        <TextBlock Text="?" FontSize="12"/>
                    </Button.Content>
                    <!-- Close button style here -->
                </Button>
            </StackPanel>
        </DataTemplate>
    </TabControl.ItemTemplate>
</TabControl>
```

---

## ?? 最佳实践

1. **主 Tab 使用 QueryTabStyle**
   - 用于顶层导航
   - 支持关闭按钮
   - 卡片式外观

2. **内部 Tab 使用 ModernTabItemStyle**
   - 用于内容区域的 Tab
   - 底部下划线高亮
   - 更简洁的设计

3. **保持一致性**
   - 所有 TabControl 都使用 ModernTabControlStyle
   - 根据用途选择合适的 TabItem 样式

4. **关闭按钮**
   - 只在可关闭的 Tab 上显示
   - 使用圆形红色悬停样式
   - 保持 18x18 的尺寸

---

## ?? 已应用的界面

? **ShellView.xaml** - 主查询 Tab 容器
? **JsonQueryTabView.xaml** - Text Input / Tree View Tab

### 建议应用到:
- [ ] QueryStoreView.xaml (如果有 Tab)
- [ ] FunctionReferenceView.xaml (如果有 Tab)
- [ ] 其他自定义对话框中的 Tab

---

现在你的界面拥有：
- ?? 现代化的 Tab 设计
- ?? 蓝色主题一致性
- ? 平滑的动画效果
- ?? 清晰的视觉层次
- ?? 优秀的用户体验
