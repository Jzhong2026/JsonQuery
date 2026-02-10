# TreeView 排序显示功能说明

## ?? 新功能：TreeView 显示排序后的数据

现在，当您配置数组排序后，TreeView 会**实际显示排序后的元素顺序**！

## 功能概述

### 之前的行为
- TreeView 始终显示原始 JSON 顺序
- `[Sorted by age ↑]` 只是一个配置标记
- 只有在执行查询时才应用排序

### 现在的行为 ?
- ? TreeView **实时显示**排序后的顺序
- ? 数组索引自动更新（[0], [1], [2]...）
- ? 可以清除排序，恢复原始顺序
- ? 查询自动使用排序后的索引

## 完整使用流程

### 步骤 1：配置排序

1. **启用选择**（可选，但推荐）
   - 点击 "Enable Selection" 按钮

2. **配置数组排序**
   - 右键点击 `people` 数组节点
   - 选择 "Configure Array Sort..."
   - 选择排序字段：`age`
   - 选择排序方向：`Ascending`（升序）
   - 点击 OK

3. **立即看到效果** ?
   ```
   排序前：
   people: Array
     [0]: Jason (30)
     [1]: Alice (35)
     [2]: Bob (28)
     [3]: Diana (42)
     [4]: Tom (25)

   排序后（按 age 升序）：
   people: Array [Sorted by age ↑]
     [0]: Tom (25)      ← 现在 [0] 是最年轻的
     [1]: Bob (28)
     [2]: Jason (30)
     [3]: Alice (35)
     [4]: Diana (42)
   ```

### 步骤 2：使用排序后的数据

#### 示例 A：选择排序后的第一个元素
```
1. 点击 [0]: Object（现在是 Tom）
2. Query 自动生成：people | sort_by(@, &age)[0]
3. 点击 Execute
4. Result 显示：Tom 的完整对象
```

#### 示例 B：选择排序后的特定属性
```
1. 点击 [0] → profile → firstName
2. Query 自动生成：people | sort_by(@, &age)[0].profile.firstName
3. 点击 Execute
4. Result 显示：Tom
```

#### 示例 C：查看降序排序
```
1. 右键 people → "Configure Array Sort..."
2. 选择 Descending
3. TreeView 立即更新：
   [0]: Diana (42)  ← 最年长的
   [1]: Alice (35)
   [2]: Jason (30)
   [3]: Bob (28)
   [4]: Tom (25)
```

### 步骤 3：清除排序

```
1. 右键已排序的 people 数组
2. 选择 "Clear Sort"
3. TreeView 恢复原始顺序：
   [0]: Jason (30)   ← 恢复原始第一个
   [1]: Alice (35)
   [2]: Bob (28)
   [3]: Diana (42)
   [4]: Tom (25)
```

## 关键特性

### 1. 自动索引更新
- 排序后，数组索引 `[0]`, `[1]`, `[2]`... 会自动更新
- TreeView 中的 `[0]` 始终表示排序后的第一个元素
- Query 中的 `[0]` 也指向排序后的第一个元素

### 2. 原始顺序保护
- 第一次配置排序前，系统会保存原始顺序
- 清除排序时，会恢复到原始顺序
- 多次排序不会丢失原始数据

### 3. 实时视觉反馈
```
配置排序 → TreeView 立即重新排列 → 标记显示 [Sorted by age ↑]
```

### 4. Query 同步
- 选择任何节点时，Query 自动包含排序表达式
- TreeView 的索引与 Query 的索引**完全一致**

## 实际测试用例

### 测试 1：升序排序
```
操作：
1. 右键 people → "Configure Array Sort..."
2. 选择 age, Ascending

期望结果：
TreeView 显示：
[0] Tom (25)
[1] Bob (28)
[2] Jason (30)
[3] Alice (35)
[4] Diana (42)

验证：
- 点击 [0] → age
- Query: people | sort_by(@, &age)[0].age
- Result: 25 ?
```

### 测试 2：降序排序
```
操作：
1. 右键 people → "Configure Array Sort..."
2. 选择 age, Descending

期望结果：
TreeView 显示：
[0] Diana (42)
[1] Alice (35)
[2] Jason (30)
[3] Bob (28)
[4] Tom (25)

验证：
- 点击 [0] → age
- Query: people | sort_by(@, &age) | reverse(@)[0].age
- Result: 42 ?
```

### 测试 3：按字符串排序
```
操作：
1. 右键 people → "Configure Array Sort..."
2. 选择 profile.firstName（如果可用），或创建示例
3. Ascending

期望结果：
按名字字母顺序排序
[0] Alice
[1] Bob
[2] Diana
[3] Jason
[4] Tom
```

### 测试 4：清除排序
```
操作：
1. 在已排序的数组上右键
2. 选择 "Clear Sort"

期望结果：
TreeView 恢复原始顺序：
[0] Jason (30)
[1] Alice (35)
[2] Bob (28)
[3] Diana (42)
[4] Tom (25)

标记消失：people: Array
```

## 对比表格

| 场景 | TreeView [0] | Query [0] | Result |
|------|--------------|-----------|--------|
| **无排序** | Jason (30) | Jason (30) | Jason |
| **升序排序** | Tom (25) | Tom (25) | Tom |
| **降序排序** | Diana (42) | Diana (42) | Diana |
| **清除排序** | Jason (30) | Jason (30) | Jason |

## 技术细节

### 数据流程
```
1. 用户配置排序
   ↓
2. SaveOriginalOrder() - 保存原始顺序
   ↓
3. ApplySortToChildren() - 对 Children 集合排序
   ↓
4. 更新索引：[0], [1], [2]...
   ↓
5. 更新路径：people[0], people[1]...
   ↓
6. Children.Clear() + Children.Add()
   ↓
7. TreeView 自动刷新显示
   ↓
8. 用户看到排序后的结果 ?
```

### 清除排序流程
```
1. 用户点击 "Clear Sort"
   ↓
2. SortKey = null
   ↓
3. RestoreOriginalOrder()
   ↓
4. 从 _originalChildrenOrder 恢复
   ↓
5. TreeView 显示原始顺序
```

## 常见问题

### Q: TreeView 中看到的顺序和 Result 不一致？
**A**: 不应该出现这种情况。如果出现：
1. 检查是否正确配置了排序
2. 检查 Query 是否包含 `sort_by`
3. 尝试清除排序后重新配置

### Q: 清除排序后，原始顺序错误？
**A**: 确保在第一次配置排序前，数据没有被修改。如果需要，点击 "Refresh Tree" 重新加载。

### Q: 排序后索引从哪里开始？
**A**: 始终从 `[0]` 开始，与 JMESPath 的索引一致。

### Q: 可以按多个字段排序吗？
**A**: 当前版本只支持按一个字段排序。如需多字段排序，需要手动编写 JMESPath 查询。

### Q: 排序会影响原始 JSON 数据吗？
**A**: 不会！排序只影响 TreeView 的显示和查询生成。原始 JSON 数据完全不变。

## 调试提示

### 查看调试输出
在 Visual Studio 的 Output 窗口中，您会看到：
```
SortKey set to: age for node: people
Saved original order for people: 5 items
Reindexed: [0] -> [0]
Reindexed: [4] -> [1]
Reindexed: [2] -> [2]
Reindexed: [1] -> [3]
Reindexed: [3] -> [4]
Applied sort to people: 5 items sorted by age ASC
```

这显示了：
- 排序键设置
- 原始顺序保存
- 索引重新分配
- 排序完成

## 总结

### 主要优势
1. ? **直观**：直接在 TreeView 中看到排序结果
2. ? **一致**：TreeView 索引与 Query 索引匹配
3. ? **可逆**：可以随时恢复原始顺序
4. ? **实时**：配置后立即生效

### 推荐工作流
1. 浏览原始数据（无排序）
2. 配置需要的排序
3. 在 TreeView 中查看排序结果
4. 选择需要的元素
5. 执行查询获取数据
6. 如需，清除排序查看原始数据

现在您可以更直观地使用数组排序功能了！??

