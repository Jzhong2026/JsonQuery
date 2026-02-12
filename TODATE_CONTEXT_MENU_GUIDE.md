# ToDate Context Menu - Quick Guide

## How to Use

### From Tree View Context Menu

1. **Right-click** on any node in the Tree View (especially date fields)
2. Select **"Generate todate Query..."**
3. Configure the date format conversion in the dialog
4. Click **OK** to generate the query

## Dialog Options

### Source Format
- **Leave empty** to auto-detect the date format
- **Enter format** if you know the exact format (e.g., `MM/dd/yyyy`)

### Target Format
- **Leave empty** to use default format (`yyyy-MM-dd`)
- **Enter format** for custom output (e.g., `MMMM dd, yyyy`)

### Quick Format Buttons
Click any button to quickly set the target format:
- `yyyy-MM-dd` - ISO standard (2024-01-15)
- `MM/dd/yyyy` - US format (01/15/2024)
- `dd/MM/yyyy` - European format (15/01/2024)
- `yyyyMMdd` - Compact format (20240115)
- `MMMM dd, yyyy` - Long date (January 15, 2024)
- `yyyy/MM/dd` - Slash format (2024/01/15)

## Examples

### Example 1: Auto-detect and convert to ISO format
**Node Value**: "01/15/2024"
- Source Format: *(leave empty)*
- Target Format: `yyyy-MM-dd`
- **Generated Query**: `todate(fieldName, '', 'yyyy-MM-dd')`
- **Result**: "2024-01-15"

### Example 2: Specific source format to long date
**Node Value**: "2024-01-15"
- Source Format: `yyyy-MM-dd`
- Target Format: `MMMM dd, yyyy`
- **Generated Query**: `todate(fieldName, 'yyyy-MM-dd', 'MMMM dd, yyyy')`
- **Result**: "January 15, 2024"

### Example 3: Array of dates
**Node**: Array of date strings
- Source Format: `MM/dd/yyyy`
- Target Format: `yyyy-MM-dd`
- **Generated Query**: `todate(dates[*], 'MM/dd/yyyy', 'yyyy-MM-dd')`
- **Result**: Array of converted dates

## Tips

?? **Auto-detect works best** when your dates are in common formats  
?? **Specify source format** for unusual or ambiguous date strings  
?? **Use empty target** for consistent ISO 8601 output  
?? **Click "Clear Both"** to use auto-detect for both source and target

## Common Use Cases

1. **Standardize mixed date formats** - Convert all dates to ISO format
2. **Localize dates** - Convert ISO dates to user-friendly formats
3. **Database export** - Convert from display format to database format
4. **Report generation** - Convert to long date format for reports
5. **API integration** - Match required date format for APIs
