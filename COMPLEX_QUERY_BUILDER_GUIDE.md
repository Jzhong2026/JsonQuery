# Complex Query Builder - 改进说明

## 新增功能

### 1. **Current Tab Field（当前 Tab 字段）**
- 自动从当前 Tab 的 JSON 数据中提取字段路径
- 可以直接选择字段，无需手动输入路径
- 支持嵌套字段（例如：`user.profile.name`）
- 支持数组字段（例如：`items[*].id`）

### 2. **Expression Builder（表达式构建器）**
快速构建条件判断表达式，特别适合 `iff` 函数

#### 支持的操作符：
- **Equal（相等）**: `eq(field, value)`
- **NotEqual（不相等）**: `!eq(field, value)`
- **GreaterThan（大于）**: `field > value`
- **LessThan（小于）**: `field < value`
- **GreaterThanOrEqual（大于等于）**: `field >= value`
- **LessThanOrEqual（小于等于）**: `field <= value`
- **Contains（包含）**: `contains(field, value)`
- **StartsWith（开头匹配）**: `starts_with(field, value)`
- **EndsWith（结尾匹配）**: `ends_with(field, value)`

#### 值类型：
- **Static（静态值）**: 输入固定的字符串或数字
- **Field（字段引用）**: 引用另一个字段的值进行比较

## 使用示例

### 示例 1：简单的 if 条件判断
**需求**: 如果 Id 等于 5，返回 "Match"，否则返回 "No Match"

**步骤**:
1. 选择函数：`iff`
2. 参数 1（条件）:
   - 类型：`ExpressionBuilder`
   - Field: `Id`
   - Operator: `Equal`
   - Value: `5` (Static)
3. 参数 2（真值）:
   - 类型：`StaticString`
   - 值：`Match`
4. 参数 3（假值）:
   - 类型：`StaticString`
   - 值：`No Match`

**生成查询**: `iff(eq(Id, `5`), 'Match', 'No Match')`

### 示例 2：字段比较
**需求**: 如果 price 大于 100，返回 "Expensive"，否则返回 "Affordable"

**步骤**:
1. 选择函数：`iff`
2. 参数 1（条件）:
   - 类型：`ExpressionBuilder`
   - Field: `price`
   - Operator: `GreaterThan`
   - Value: `100` (Static)
3. 参数 2: `Expensive`
4. 参数 3: `Affordable`

**生成查询**: `iff(price > `100`, 'Expensive', 'Affordable')`

### 示例 3：使用字段引用
**需求**: 如果 quantity 小于 minStock，返回 "Reorder"，否则返回 "OK"

**步骤**:
1. 选择函数：`iff`
2. 参数 1（条件）:
   - 类型：`ExpressionBuilder`
   - Field: `quantity`
   - Operator: `LessThan`
   - Value: `minStock` (切换到 Field 模式)
3. 参数 2: `Reorder`
4. 参数 3: `OK`

**生成查询**: `iff(quantity < minStock, 'Reorder', 'OK')`

### 示例 4：字符串包含
**需求**: 如果 email 包含 "gmail.com"，返回 "Gmail User"

**步骤**:
1. 选择函数：`iff`
2. 参数 1（条件）:
   - 类型：`ExpressionBuilder`
   - Field: `email`
   - Operator: `Contains`
   - Value: `gmail.com` (Static)
3. 参数 2: `Gmail User`
4. 参数 3: `Other`

**生成查询**: `iff(contains(email, 'gmail.com'), 'Gmail User', 'Other')`

## 优势

? **无需手动创建中间查询** - 直接在 Expression Builder 中一步完成条件构建  
? **智能字段提示** - 自动从当前 JSON 提取字段列表  
? **可视化操作** - 通过下拉选择和输入框，无需记忆语法  
? **灵活组合** - 支持静态值和字段引用的混合使用  
? **实时预览** - Result Preview 区域实时显示生成的查询表达式

## 参数类型说明

| 类型 | 说明 | 示例 |
|------|------|------|
| SavedQuery | 使用保存的查询 | `UserDefinedFields` |
| StaticString | 静态字符串（自动加引号） | `'Hello'` |
| Number | 数字（自动加反引号） | `` `42` `` |
| CurrentTabField | 当前 Tab 的字段路径 | `user.name` |
| ExpressionBuilder | 表达式构建器（条件判断） | `eq(Id, `5`)` |
| TreePath | 自定义路径表达式 | `items[*].price` |
| Separator | 分隔符（用于 join 等） | `', '` |
| NewLine | 换行符 | `newline()` |
| Space | 空格 | `' '` |

## 注意事项

- Expression Builder 自动识别数字和字符串，无需手动添加引号或反引号
- 切换 Static/Field 按钮可以在静态值和字段引用之间切换
- iff 函数会自动将第一个参数设置为 ExpressionBuilder 类型
- 字段列表最多提取 3 层深度，避免过于复杂
