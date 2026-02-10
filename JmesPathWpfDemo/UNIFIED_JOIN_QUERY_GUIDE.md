# 功能更新：统一 Join Query

## ?? 更新说明

根据您的反馈，我们进行了以下关键改进：

1.  **统一入口**：
    - 移除了 "Apply Query to Items..." 菜单。
    - 升级了 **"Generate join Query..."** 功能。
    - 现在点击 "Generate join Query..." 会直接打开新的高级批量查询对话框。

2.  **修复括号问题**：
    - 生成的 JMESPath 不再强制在表达式外层添加括号。
    - 例如：`join(..., Attributes[*].Name)` 而不是 `join(..., Attributes[*].(Name))`。
    - 这解决了简单属性访问的语法问题。

3.  **支持 `newline()` 函数**：
    - 分隔符选择器现在支持 `newline()` 函数。
    - 如果选择或输入 `newline()`（或 `\n`），生成的 JMESPath 将使用 `newline()` 函数而不是字符串字面量。
    - 结果：`join(newline(), ...)`，这会生成真正的换行符，而不是文本 `\n`。

---

## ?? 新的使用流程

1.  右键点击任意数组节点。
2.  选择 **"Generate join Query..."**。
3.  在弹出的对话框中：
    - **Query for each item**: 输入 `Name` 或使用 **Builder...** 构建复杂逻辑。
    - **Join Separator**: 选择 **"Newline (newline())"**。
4.  点击 **Apply**。

生成的查询示例：
```
join(newline(), UserDefinedFields[0].Attributes[*].Name)
```

或者使用复杂逻辑：
```
join(newline(), UserDefinedFields[0].Attributes[*].iff(eq(to_string(ID), '5'), concat(Name, ' ', 'provided'), Name))
```

所有功能现在都集中在一个统一、强大且正确的工具中。

