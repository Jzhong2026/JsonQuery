# eq() 函数返回 null 问题排查指南

## 问题现象
执行 `eq(ID, 5)` 返回 `null` 而不是 `true` 或 `false`。

## 可能的原因

### 1. ID 属性不存在或为 null
如果 JSON 数据中没有 `ID` 字段，或者 `ID` 的值是 `null`，`eq()` 函数会返回 `false`，但整个表达式可能会返回 `null`。

**示例数据问题：**
```json
{
  "Name": "Test",
  "Value": "Something"
  // 缺少 ID 字段
}
```

**解决方案：** 检查你的 JSON 数据，确保 `ID` 字段存在。

### 2. ID 是嵌套属性
如果 `ID` 不是顶级属性，而是嵌套在对象内部，你需要使用完整路径。

**错误写法：**
```jmespath
eq(ID, 5)  // 假设 ID 在 Attributes 内部
```

**正确写法：**
```jmespath
eq(Attributes.ID, 5)
```

### 3. 在数组上下文中使用
如果你在数组项上执行查询，可能需要使用投影。

**场景：** 检查数组中是否有 ID 为 5 的项

**错误写法：**
```jmespath
eq(Items[*].ID, 5)  // 这会比较数组与数字，返回 false
```

**正确写法 1：** 使用过滤
```jmespath
Items[?ID == `5`]  // 使用标准 JMESPath 过滤器
```

**正确写法 2：** 在批量查询中使用
```jmespath
join(newline(), Items[*].(iff(eq(ID, `5`), concat(Name, ' provided'), Name)))
```

注意：在 JMESPath 中，数字字面量需要使用反引号 `` `5` `` 而不是 `5`。

### 4. 类型不匹配
`eq()` 函数使用 `JToken.DeepEquals`，这意味着类型必须完全匹配。

**类型匹配示例：**
```json
{
  "ID": 5,        // 数字类型
  "Name": "Test"
}
```

```jmespath
eq(ID, `5`)      // ? 正确：数字与数字比较 → true
eq(ID, '5')      // ? 错误：数字与字符串比较 → false
```

**如果 ID 是字符串：**
```json
{
  "ID": "5",      // 字符串类型
  "Name": "Test"
}
```

```jmespath
eq(ID, '5')      // ? 正确：字符串与字符串比较 → true
eq(ID, `5`)      // ? 错误：字符串与数字比较 → false
```

## 完整的排查步骤

### 步骤 1: 检查 JSON 数据
在应用中，切换到 **Tree View** 标签，展开你的数据结构，确认：
1. `ID` 字段是否存在
2. `ID` 的值是什么
3. `ID` 的类型是 Number 还是 String（在 TreeView 中会显示类型）

### 步骤 2: 验证简单查询
先测试一个简单的查询来获取 ID 值：

```jmespath
ID
```

如果这个返回 `null`，说明 `ID` 不在当前上下文中。你可能需要：
- 使用完整路径（如 `UserData.ID`）
- 或者在正确的节点上执行查询

### 步骤 3: 测试 eq() 函数
一旦你能成功获取 ID 值，测试 eq() 函数：

**如果 ID 是数字类型：**
```jmespath
eq(ID, `5`)
```

**如果 ID 是字符串类型：**
```jmespath
eq(ID, '5')
```

### 步骤 4: 在复杂查询构建器中使用

1. 点击 **Complex Query...** 按钮
2. 选择 `eq` 函数
3. 第一个参数：
   - 类型：**SavedQuery**
   - 选择或输入：`ID`（或完整路径如 `UserData.ID`）
4. 第二个参数：
   - 如果 ID 是数字：类型选 **Number**，输入 `5`
   - 如果 ID 是字符串：类型选 **StaticString**，输入 `5`
5. 查看预览结果

## 常见使用场景

### 场景 1: 直接比较（单个对象）
```json
{
  "ID": 5,
  "Name": "Item A"
}
```

查询：
```jmespath
eq(ID, `5`)
```

结果：
```json
true
```

### 场景 2: 条件选择（单个对象）
```json
{
  "ID": 5,
  "Name": "Item A"
}
```

查询：
```jmespath
iff(eq(ID, `5`), 'Found', 'Not Found')
```

结果：
```json
"Found"
```

### 场景 3: 数组批量查询
```json
{
  "Items": [
    {"ID": 5, "Name": "Item A"},
    {"ID": 6, "Name": "Item B"},
    {"ID": 5, "Name": "Item C"}
  ]
}
```

查询（在 Items 数组节点上右键 → Apply Query to Items）：
```
Query for each item: iff(eq(ID, `5`), concat(Name, ' ?'), Name)
Separator: \n
```

生成的 JMESPath：
```jmespath
join(newline(), Items[*].(iff(eq(ID, `5`), concat(Name, ' ?'), Name)))
```

结果：
```
Item A ?
Item B
Item C ?
```

### 场景 4: 过滤数组（推荐方式）
如果你只是想过滤出 ID 为 5 的项，使用标准 JMESPath 过滤器更简单：

```jmespath
Items[?ID == `5`]
```

结果：
```json
[
  {"ID": 5, "Name": "Item A"},
  {"ID": 5, "Name": "Item C"}
]
```

## JMESPath 字面量语法提醒

在 JMESPath 中：
- **数字字面量**：使用反引号 `` `5` ``, `` `3.14` ``, `` `-10` ``
- **字符串字面量**：使用单引号 `'hello'`, `'world'`
- **布尔字面量**：`` `true` ``, `` `false` ``
- **null 字面量**：`` `null` ``

## 复杂查询构建器的参数类型

在复杂查询构建器中：

| 参数类型 | 用途 | 生成的代码 | 何时使用 |
|---------|------|-----------|---------|
| **Number** | 数字字面量 | `5`, `3.14`, `-10` | 比较数字类型的字段 |
| **StaticString** | 字符串字面量 | `'hello'`, `'5'` | 比较字符串类型的字段 |
| **SavedQuery** | 引用字段/表达式 | `ID`, `User.Name` | 引用 JSON 数据中的字段 |
| **NewLine** | 换行符 | `newline()` | 在 concat 中使用 |
| **Space** | 空格 | `' '` | 在 concat 中使用 |

**重要提示**：复杂查询构建器会自动处理数字字面量的反引号，你只需：
- 选择 **Number** 类型
- 输入 `5`（不需要手动加反引号）
- 生成的查询会是 `5`（在 JMESPath.NET 库中数字字面量不需要反引号）

## DevLab.JmesPath 库的特性

该应用使用的是 `DevLab.JmesPath` 库，它有一些特殊行为：
- 数字字面量可以直接写成 `5`，不一定需要反引号
- 字符串必须用单引号 `'text'`
- 自定义函数（如 `eq`, `iff`, `concat`）已注册并可用

## 调试建议

1. **使用 TreeView 确认数据结构**
   - 展开所有节点
   - 查看字段类型（Number、String、Boolean 等）

2. **逐步构建查询**
   - 先测试简单的字段访问：`ID`
   - 再测试函数：`eq(ID, 5)`
   - 最后嵌套：`iff(eq(ID, 5), ...)`

3. **使用复杂查询构建器**
   - 避免手动输入错误
   - 自动生成正确的语法

4. **查看执行结果**
   - 如果返回 `null`，检查路径是否正确
   - 如果返回 `false`，检查类型是否匹配

## 示例：完整的调试流程

假设你有这样的 JSON：
```json
{
  "UserDefinedFields": [
    {"ID": 5, "Name": "Field1"},
    {"ID": 6, "Name": "Field2"}
  ]
}
```

**目标**：找出 ID 为 5 的字段名称

**步骤：**

1. **定位正确的上下文**
   - 在 TreeView 中右键点击 `UserDefinedFields`
   - 选择 "Create New Tab"（这会创建以数组为根的新 Tab）

2. **在新 Tab 中测试**
   - 测试：`[0].ID` → 应该返回 `5`
   - 测试：`[0].Name` → 应该返回 `"Field1"`

3. **构建批量查询**
   - 右键点击 TreeView 根节点（数组）
   - 选择 "Apply Query to Items..."
   - Query: `iff(eq(ID, 5), Name, '')`
   - Separator: `\n`
   - 执行后应该得到：
     ```
     Field1
     
     ```

如果你还是遇到问题，请提供：
1. 你的完整 JSON 数据结构
2. 你在哪个节点上执行的查询
3. TreeView 中显示的数据类型
