# Array Operations Guide

## New Features Implemented

### 1. **merge_arrays** Function
Combines multiple arrays into a single array.

**Syntax:**
```
merge_arrays(array1, array2, array3, ...)
```

**Example with sample.json:**
```
merge_arrays(UserDefinedFields[0].Attributes, UserDefinedFields[1].Attributes)
```

**Result:**
All 6 attributes (Attr1-Attr6) combined into a single array.

---

### 2. **flatten** Function
Recursively flattens nested arrays into a single level array.

**Syntax:**
```
flatten(array)
```

**Example:**
```
flatten([[1, 2], [3, [4, 5]]])
```

**Result:**
```
[1, 2, 3, 4, 5]
```

---

### 3. **join** Function (Enhanced)
Now available in the Complex Query Builder with saved query pipeline support.

**Dialog Features:**
- Custom separator input
- Quick-add buttons for common separators
- Property selection for object arrays
- **NEW:** Saved query pipeline checkbox
- **NEW:** Saved query selector

**Pipeline Example:**
If you have a saved query named "GetAttrNames":
```
UserDefinedFields[*].Attributes[*].Name
```

And you enable "Use saved query as pipeline", the dialog will generate:
```
(UserDefinedFields[*].Attributes[*].Name) | join(', ', @)
```

**Result:**
```
"Attr1, Attr2, Attr3, Attr4, Attr5, Attr6"
```

---

### 4. **concat_ws** Function
Concatenate with separator (added to Complex Query Builder).

**Syntax:**
```
concat_ws(separator, value1, value2, ...)
```

**Example:**
```
concat_ws(' | ', UserDefinedFields[*].DisplayName)
```

**Result:**
```
"DD-text2 | DD-List"
```

---

## Complex Query Builder Enhancements

### New Parameter Types:

1. **TreePath** - Reference JSON tree paths
   - Enter raw JMESPath expressions
   - Examples: `UserDefinedFields[*].Attributes`, `@`, `Name`

2. **ArrayExpression** - Direct array expressions
   - Enter array literals or expressions
   - Examples: `[]`, `[1, 2, 3]`, `[*].Name`

### New Functions Available:
- `merge_arrays` (variadic)
- `flatten` (1 parameter)
- `join` (2 parameters: separator, array)
- `concat_ws` (variadic: separator, values...)

---

## Usage Examples with sample.json

### Example 1: Combine All Attributes
```
merge_arrays(UserDefinedFields[0].Attributes, UserDefinedFields[1].Attributes)
```

### Example 2: Get All Attribute Names as Comma-Separated String
```
join(', ', UserDefinedFields[*].Attributes[*].Name)
```

### Example 3: Complex Nested Query
```
merge_arrays(UserDefinedFields[*].Attributes) | [*].{ID: ID, Name: Name, Date: CreateDate}
```

### Example 4: Using Complex Query Builder
1. Select function: `concat_ws`
2. Add parameters:
   - Parameter 1 (String): ` | `
   - Parameter 2 (Saved Query): Select your saved query
   - Parameter 3 (TreePath): `CallerName`
3. Result: `concat_ws(' | ', <saved_query_result>, CallerName)`

### Example 5: Join with Pipeline
1. Right-click on array node ¡ú "Generate Join Query"
2. Set separator: `, `
3. Check "Use saved query as pipeline"
4. Select saved query that returns an array
5. Click OK

---

## Tips

- **Save frequently used expressions** as Saved Queries for reuse in pipelines
- **Use TreePath parameter type** when you need to reference JSON paths dynamically
- **Combine merge_arrays with projections** to reshape data efficiently
- **Pipeline syntax** allows chaining: `query1 | query2 | query3`

---

## Testing with sample.json

Try these queries:

1. **Get all attribute IDs:**
   ```
   merge_arrays(UserDefinedFields[*].Attributes) | [*].ID
   ```

2. **Create summary string:**
   ```
   concat_ws(' - ', Name, Priority, CallerName)
   ```

3. **Get sorted unique attribute names:**
   ```
   merge_arrays(UserDefinedFields[*].Attributes) | [*].Name | sort(@)
   ```

4. **Complex filtering and joining:**
   ```
   UserDefinedFields[?Id == `1`].Attributes[*].Name | join(' + ', @)
   ```
