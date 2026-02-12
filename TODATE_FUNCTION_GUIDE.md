# ToDate Function - 使用说明

## 函数签名
```
todate(value, source_format, target_format)
```

## 参数说明

| 参数 | 类型 | 必需 | 说明 |
|------|------|------|------|
| `value` | string/array | ? | 日期字符串或日期字符串数组 |
| `source_format` | string | ? | 源日期格式（为空则自动检测） |
| `target_format` | string | ? | 目标日期格式（为空则使用默认：`yyyy-MM-dd`） |

## 功能特性

? **支持单个值和数组** - 可以处理单个日期字符串或日期字符串数组  
? **源格式可选** - 可以指定源格式，也可以留空让函数自动检测  
? **目标格式可选** - 可以指定目标格式，也可以留空使用默认的 ISO 8601 日期格式  
? **返回字符串** - 总是返回格式化后的字符串值  
? **不涉及时区** - 纯粹的日期格式转换，不进行时区转换

## 使用示例

### 示例 1：基本格式转换
```json
// Input
{
  "date": "01/15/2024"
}

// Query
todate(date, 'MM/dd/yyyy', 'yyyy-MM-dd')

// Output
"2024-01-15"
```

### 示例 2：自动检测源格式
```json
// Input
{
  "date": "2024-01-15 14:30:00"
}

// Query
todate(date, '', 'yyyy-MM-dd')

// Output
"2024-01-15"
```

### 示例 3：使用默认目标格式
```json
// Input
{
  "date": "01/15/2024"
}

// Query
todate(date, 'MM/dd/yyyy', '')

// Output
"2024-01-15"  // 默认格式
```

### 示例 4：完全自动（两个格式都为空）
```json
// Input
{
  "date": "2024-01-15 14:30:00"
}

// Query
todate(date, '', '')

// Output
"2024-01-15"  // 自动解析并输出默认格式
```

### 示例 5：处理数组
```json
// Input
{
  "events": [
    {"name": "Meeting", "date": "01/15/2024"},
    {"name": "Call", "date": "02/20/2024"},
    {"name": "Review", "date": "03/10/2024"}
  ]
}

// Query
todate(events[*].date, 'MM/dd/yyyy', 'yyyy-MM-dd')

// Output
[
  "2024-01-15",
  "2024-02-20",
  "2024-03-10"
]
```

### 示例 6：自定义输出格式（长日期）
```json
// Input
{
  "date": "2024-01-15"
}

// Query
todate(date, 'yyyy-MM-dd', 'MMMM dd, yyyy')

// Output
"January 15, 2024"
```

### 示例 7：只保留年月
```json
// Input
{
  "date": "2024-01-15 14:30:00"
}

// Query
todate(date, 'yyyy-MM-dd HH:mm:ss', 'yyyy-MM')

// Output
"2024-01"
```

### 示例 8：转换为中文格式
```json
// Input
{
  "date": "2024-01-15"
}

// Query
todate(date, 'yyyy-MM-dd', 'yyyy年MM月dd日')

// Output
"2024年01月15日"
```

## 常用日期格式

| 格式 | 示例 | 说明 |
|------|------|------|
| `yyyy-MM-dd` | 2024-01-15 | ISO 8601 标准日期格式 |
| `MM/dd/yyyy` | 01/15/2024 | 美国常用格式 |
| `dd/MM/yyyy` | 15/01/2024 | 欧洲常用格式 |
| `yyyy-MM-dd HH:mm:ss` | 2024-01-15 14:30:00 | 日期和时间 |
| `yyyyMMdd` | 20240115 | 紧凑格式 |
| `MMMM dd, yyyy` | January 15, 2024 | 长日期格式 |
| `MMM dd, yyyy` | Jan 15, 2024 | 短月份格式 |
| `yyyy年MM月dd日` | 2024年01月15日 | 中文格式 |
| `yyyy/MM/dd` | 2024/01/15 | 斜杠分隔 |
| `dd-MMM-yyyy` | 15-Jan-2024 | 短月份名称 |

## 与 todatetime 的区别

| 功能 | todate | todatetime |
|------|--------|------------|
| 格式转换 | ? | ? |
| 时区转换 | ? | ? |
| 参数数量 | 3个 | 4个 |
| 使用场景 | 纯日期格式转换 | 需要时区转换的日期时间 |

## 错误处理

如果输入无法解析，函数将返回 `null`：
```json
// Input
{
  "date": "invalid-date"
}

// Query
todate(date, 'yyyy-MM-dd', 'MM/dd/yyyy')

// Output
null
```

## 实际应用场景

1. **数据导入导出** - 转换不同系统之间的日期格式
2. **报表生成** - 将数据库日期格式转换为友好的显示格式
3. **数据清洗** - 统一混合格式的日期数据
4. **API 对接** - 适配不同 API 的日期格式要求
5. **用户界面显示** - 将内部日期格式转换为用户友好的格式

## 提示

?? 如果不确定源格式，可以将 `source_format` 留空让函数自动检测  
?? 默认输出格式 `yyyy-MM-dd` 是 ISO 8601 标准，适合大多数场景  
?? 对于数组处理，每个元素都会应用相同的格式转换规则  
?? 函数使用 `InvariantCulture`，确保跨文化环境的一致性
