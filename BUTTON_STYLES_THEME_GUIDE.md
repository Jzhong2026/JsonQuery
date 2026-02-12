# Button Styles Theme - Usage Guide

## ?? Theme Location
`JmesPathWpfDemo/Themes/ButtonStyles.xaml`

## ?? Available Button Styles

### 1. **PrimaryButtonStyle** (Blue)
主要操作按钮，用于常规功能选项

```xaml
<Button Content="Select" 
        Style="{StaticResource PrimaryButtonStyle}"
        Click="OnSelect"/>
```

**Colors:**
- Normal: `#007BFF`
- Hover: `#0056B3`

**Use Cases:**
- 快速格式选择按钮
- 主要功能按钮
- 导航按钮

---

### 2. **SuccessButtonStyle** (Green)
成功/确认操作按钮

```xaml
<Button Content="OK" 
        Style="{StaticResource SuccessButtonStyle}"
        IsDefault="True"
        Click="OnOk"/>
```

**Colors:**
- Normal: `#28A745`
- Hover: `#218838`

**Use Cases:**
- OK / Confirm 按钮
- Save / Submit 按钮
- Apply 按钮

---

### 3. **DangerButtonStyle** (Red)
危险/删除操作按钮

```xaml
<Button Content="Clear" 
        Style="{StaticResource DangerButtonStyle}"
        Click="OnClear"/>
```

**Colors:**
- Normal: `#DC3545`
- Hover: `#C82333`

**Use Cases:**
- Clear / Delete 按钮
- Remove 按钮
- Reset 按钮

---

### 4. **SecondaryButtonStyle** (Gray)
次要操作按钮

```xaml
<Button Content="Cancel" 
        Style="{StaticResource SecondaryButtonStyle}"
        IsCancel="True"
        Click="OnCancel"/>
```

**Colors:**
- Normal: `#6C757D`
- Hover: `#5A6268`

**Use Cases:**
- Cancel 按钮
- Close 按钮
- Back 按钮

---

### 5. **InfoButtonStyle** (Light Blue)
信息/提示操作按钮

```xaml
<Button Content="Help" 
        Style="{StaticResource InfoButtonStyle}"
        Click="OnHelp"/>
```

**Colors:**
- Normal: `#17A2B8`
- Hover: `#138496`

**Use Cases:**
- Help / Info 按钮
- Details 按钮
- Preview 按钮

---

### 6. **WarningButtonStyle** (Yellow/Orange)
警告操作按钮

```xaml
<Button Content="Warning" 
        Style="{StaticResource WarningButtonStyle}"
        Click="OnWarning"/>
```

**Colors:**
- Normal: `#FFC107`
- Hover: `#E0A800`
- Text: `#212529` (Dark text for contrast)

**Use Cases:**
- Warning 按钮
- Caution 操作
- Review 按钮

---

### 7. **PurpleButtonStyle** (Purple)
特殊操作按钮

```xaml
<Button Content="Create Tab" 
        Style="{StaticResource PurpleButtonStyle}"
        Click="OnCreateTab"/>
```

**Colors:**
- Normal: `#6F42C1`
- Hover: `#5A32A3`

**Use Cases:**
- Create Tab from Result 按钮
- Special features
- Premium actions

---

## ?? How to Apply Styles

### In XAML
```xaml
<Button Content="My Button" 
        Style="{StaticResource PrimaryButtonStyle}"
        Width="100"
        Click="OnClick"/>
```

### Override Properties
You can override specific properties while keeping the style:

```xaml
<Button Content="Large Button" 
        Style="{StaticResource PrimaryButtonStyle}"
        Width="150"
        Height="40"
        FontSize="14"/>
```

---

## ?? Common Patterns

### Dialog Buttons
```xaml
<StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
    <Button Content="OK" 
            Style="{StaticResource SuccessButtonStyle}"
            Width="90"
            IsDefault="True"
            Click="OnOk"/>
    <Button Content="Cancel" 
            Style="{StaticResource SecondaryButtonStyle}"
            Width="90"
            Margin="10,0,0,0"
            IsCancel="True"
            Click="OnCancel"/>
</StackPanel>
```

### Action Bar
```xaml
<StackPanel Orientation="Horizontal">
    <Button Content="Add" 
            Style="{StaticResource PrimaryButtonStyle}"
            Width="80"/>
    <Button Content="Edit" 
            Style="{StaticResource InfoButtonStyle}"
            Width="80"
            Margin="5,0,0,0"/>
    <Button Content="Delete" 
            Style="{StaticResource DangerButtonStyle}"
            Width="80"
            Margin="5,0,0,0"/>
</StackPanel>
```

### Quick Options Grid
```xaml
<WrapPanel>
    <Button Content="Option 1" 
            Style="{StaticResource PrimaryButtonStyle}"
            Width="100"
            Margin="0,0,5,5"/>
    <Button Content="Option 2" 
            Style="{StaticResource PrimaryButtonStyle}"
            Width="100"
            Margin="0,0,5,5"/>
    <Button Content="Option 3" 
            Style="{StaticResource PrimaryButtonStyle}"
            Width="100"
            Margin="0,0,5,5"/>
</WrapPanel>
```

---

## ?? Style Features

### All Styles Include:
? **Rounded Corners** - 3px border radius
? **Hover Effects** - Color darkens on mouse over
? **Hand Cursor** - Shows pointer on hover
? **Disabled State** - Grayed out when disabled
? **Consistent Sizing** - Default heights (32px or 35px)
? **Smooth Transitions** - Visual feedback

### Template Features:
- Custom `ControlTemplate` for consistent appearance
- Centered content alignment
- Padding support
- Border customization

---

## ?? Files Modified

1. **Created**: `JmesPathWpfDemo/Themes/ButtonStyles.xaml`
2. **Updated**: `JmesPathWpfDemo/App.xaml` - Added theme reference
3. **Updated**: `JmesPathWpfDemo/Views/DateFormatDialog.xaml` - Applied styles

---

## ?? Applying to Other Dialogs

### Example: Update existing dialogs

**Before:**
```xaml
<Button Content="Execute" 
        Width="100" 
        Height="32"
        Foreground="White"
        Background="#0078D4"
        BorderThickness="0"
        Click="Execute"/>
```

**After:**
```xaml
<Button Content="Execute" 
        Width="100"
        Style="{StaticResource PrimaryButtonStyle}"
        Click="Execute"/>
```

---

## ?? Best Practices

1. **Use Semantic Colors**
   - Primary (Blue) for main actions
   - Success (Green) for confirmations
   - Danger (Red) for destructive actions
   - Secondary (Gray) for cancellations

2. **Consistent Sizing**
   - Dialog buttons: `Width="90"` `Height="35"`
   - Quick options: `Width="100-130"` `Height="32"`
   - Toolbar buttons: `Width="80"` `Height="28"`

3. **Button Groups**
   - Use consistent margins: `Margin="0,0,10,0"` or `Margin="5,0,0,0"`
   - Align related buttons: `HorizontalAlignment="Right"`

4. **Accessibility**
   - Set `IsDefault="True"` for OK buttons
   - Set `IsCancel="True"` for Cancel buttons
   - Add meaningful `ToolTip` values

---

## ?? Migration Checklist

To apply these styles across your application:

- [ ] Replace inline button styles with theme styles
- [ ] Update `ShellView.xaml` toolbar buttons
- [ ] Update `ComplexQueryBuilderView.xaml` buttons
- [ ] Update `JsonQueryTabView.xaml` action buttons
- [ ] Update other dialog windows
- [ ] Test all button hover effects
- [ ] Verify disabled states
- [ ] Check keyboard navigation

---

## ?? Color Reference

| Style | Normal | Hover | Usage |
|-------|--------|-------|-------|
| Primary | #007BFF | #0056B3 | Main actions |
| Success | #28A745 | #218838 | Confirmations |
| Danger | #DC3545 | #C82333 | Destructive |
| Secondary | #6C757D | #5A6268 | Cancel/Close |
| Info | #17A2B8 | #138496 | Information |
| Warning | #FFC107 | #E0A800 | Warnings |
| Purple | #6F42C1 | #5A32A3 | Special |

All colors are from Bootstrap color palette for consistency and familiarity.
