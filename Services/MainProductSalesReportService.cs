using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Drawing;
using System.Linq;

public class MainProductSalesReportService
{
    private readonly ApplicationDbContext _context;

    public MainProductSalesReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public class PivotReportResult
    {
        public List<Dictionary<string, object>> Data { get; set; } = [];
        public Dictionary<int, string> MainProductDict { get; set; } = [];
    }

    public Dictionary<int, string> mainProductDict;

    //public async Task<List<Dictionary<string, object>>> GetOrderPivotReport(
    public async Task<PivotReportResult> GetOrderPivotReport(
    DateTime start,
    DateTime end,
    int? OfficeId)
    {
        var mainProducts = await _context.Products
            .Where(p => p.ProductClass == "Main"
                     //&& p.ProductType != "Sachet"
                     && p.ProductCategory != "Package"
                     && p.IsActive)
            .OrderBy(p => p.ProductName)
            .ToListAsync();

        //var 
        mainProductDict = mainProducts
        .ToDictionary(p => p.Id, p => p.ProductName.Trim());

        var combos = await _context.ProductCombos
            .Where(c => c.IsActive)
            .ToListAsync();

        var comboLookup = combos
            .GroupBy(c => c.ProductId)
            .ToDictionary(g => g.Key, g => g.First());

        var orders = await _context.Orders
            .Where(o => o.OrderDate >= start &&
                        o.OrderDate <= end &&
                        //!o.IsVoided && --> should get all but do not display voided
                        o.OfficeId == OfficeId)
            .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Products)
            .Include(o => o.Customers)
            .Include(o => o.Office)
            .ToListAsync();

        var result = new List<Dictionary<string, object>>();

        foreach (var order in orders)
        {
            foreach (var d in order.OrderDetails.Where(d => d.ProductId != null))
            {
                var row = new Dictionary<string, object>();

                // Order-level info (repeated)
                row["Order No"] = order.OrderNo;
                row["Order Date"] = order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss");
                row["Type"] = order.OrderType;
                row["Customer"] = $"{order.Customers?.FirstName} {order.Customers?.LastName}".Trim();
                row["Product"] = d.Products?.ProductName ?? "";

                // Init pivot columns
                foreach (var mp in mainProducts)
                    row[mp.ProductName] = 0;

                // 1️⃣ REGULAR MAIN PRODUCT
                if (mainProductDict.TryGetValue(d.ProductId.Value, out var col))
                {
                    row[col] = d.Quantity;
                }

                row["Item Qty"] = d.Quantity;
                row["Total Amount"] = d.TotalPrice;

                //For export
                row["Encoded By"] = order.EncodedBy;
                row["Date Encoded"] = order.EncodeDate.HasValue ? order.EncodeDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "";
                row["Voided"] = order.IsVoided;
                row["Voided By"] = order.VoidedBy;
                row["Date Voided"] = order.VoidedDate.HasValue ? order.VoidedDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "";

                // 2️⃣ PACKAGE → explode ProductCombo
                if (comboLookup.TryGetValue(d.ProductId.Value, out var combo))
                {
                    var productIds = combo.ProductIdList
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList();

                    var quantities = combo.QuantityList
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList();

                    if (productIds.Count == quantities.Count)
                    {
                        for (int i = 0; i < productIds.Count; i++)
                        {
                            if (mainProductDict.TryGetValue(productIds[i], out var pivotCol))
                            {
                                row[pivotCol] =
                                    Convert.ToInt32(row[pivotCol]) +
                                    (quantities[i] * d.Quantity);
                            }
                        }
                    }
                }

                result.Add(row);
            }
        }
        //return result;
        return new PivotReportResult
        {
            Data = result,
            MainProductDict = mainProductDict
        };
    }

    public byte[] ExportOrderPivotToExcel(List<Dictionary<string, object>> data,
        DateTime start,
        DateTime end,
        int? officeId,
        Dictionary<int, string> mainProduct)
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Order Report");

        if (data == null || data.Count == 0)
            return package.GetAsByteArray();

        var columns = data.First().Keys.ToList();

        var displayColumns = columns
            .Where(c => !c.Equals("Voided", StringComparison.OrdinalIgnoreCase))
            .ToList();

        for (int i = 0; i < columns.Count; i++)
        {
            if (columns[i].Contains("void", StringComparison.OrdinalIgnoreCase)) continue;
            ws.Cells[1, i + 1].Value = columns[i];
        }

        // Main Header Styling
        ws.Rows[1].Style.Font.Bold = true;
        ws.Rows[1].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Rows[1].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
        ws.Rows[1].Style.Font.Color.SetColor(Color.White);

        int row = 2;

        var filteredData = data
            .Where(item =>
                !item.TryGetValue("Voided", out var v) ||
                v is not bool isVoided ||
                !isVoided)
            .ToList();

        foreach (var item in filteredData)
        {
            for (int col = 0; col < displayColumns.Count; col++)
                ws.Cells[row, col + 1].Value = item[displayColumns[col]];
            row++;
        }

        int footerRow = row;

        // Footer Styling
        ws.Rows[footerRow].Style.Font.Bold = true;
        ws.Rows[footerRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Rows[footerRow].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
        ws.Rows[footerRow].Style.Font.Color.SetColor(Color.Maroon);

        ws.Cells[footerRow, 1].Value = "TOTAL:";
        //ws.Cells[footerRow, 1, footerRow, 4].Merge = true;
        //ws.Cells[footerRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

        ws.Rows[footerRow + 1].Style.Font.Bold = true;
        ws.Rows[footerRow + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Rows[footerRow + 1].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
        ws.Rows[footerRow + 1].Style.Font.Color.SetColor(Color.DarkOrange);

        ws.Cells[footerRow + 1, 1].Value = "ACTUAL INVENTORY:";
        //ws.Cells[footerRow + 1, 1, footerRow, 4].Merge = true;
        //ws.Cells[footerRow + 1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

        var columnToProductId = mainProductDict.ToDictionary(kvp => kvp.Value.Trim(), kvp => kvp.Key); //reverse the dictionary to get the product name instead of id

        // Get all stocks first
        var allStocks = _context.InventoryStocks
    .Where(i => !officeId.HasValue || i.Location.OfficeId == officeId)
    .GroupBy(i => i.ProductId)
    .Select(g => new { ProductId = g.Key, TotalStock = g.Sum(x => x.StockQty) })
    .ToDictionary(x => x.ProductId, x => x.TotalStock);


        foreach (var colName in columns)
        {
            //if (colName == "Total Amount")
            //    continue;

            if (!columnToProductId.TryGetValue(colName.Trim(), out int productId))
                continue; // Not a product column

            int colIndex = columns.IndexOf(colName) + 1;
            string colLetter = ExcelCellAddress.GetColumnLetter(colIndex);

            ws.Cells[footerRow, colIndex].Formula =
                $"SUM({colLetter}2:{colLetter}{footerRow - 1})"; //total it

            //ws.Cells[footerRow + 1, colIndex].Value =
            //    GetStocks(officeId, productId); //get the stocks
            ws.Cells[footerRow + 1, colIndex].Value =
    allStocks.ContainsKey(productId) ? allStocks[productId] : 0;
        }

        int totalAmountColIndex = columns.IndexOf("Total Amount") + 1;

        if (totalAmountColIndex > 0)
        {
            string colLetter = ExcelCellAddress.GetColumnLetter(totalAmountColIndex);
            ws.Cells[footerRow, totalAmountColIndex].Formula =
                $"SUM({colLetter}2:{colLetter}{footerRow - 1})";

            ws.Cells[footerRow, totalAmountColIndex].Style.Numberformat.Format = "#,##0.00";

        }

        int voidedRow = footerRow + 5;

        ws.Cells[voidedRow - 1, 1].Value = "VOIDED LIST";

        // Voided Header Styling
        ws.Rows[voidedRow - 1].Style.Font.Bold = true;
        ws.Rows[voidedRow].Style.Font.Bold = true;
        ws.Rows[voidedRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Rows[voidedRow].Style.Fill.BackgroundColor.SetColor(Color.DarkSlateGray);
        ws.Rows[voidedRow].Style.Font.Color.SetColor(Color.White);

        for (int i = 0; i < displayColumns.Count; i++)
        {

            ws.Cells[voidedRow, i + 1].Value = displayColumns[i];
        }

        var voidedData = data
            .Where(item =>
                item.TryGetValue("Voided", out var v) &&
                v is bool isVoided &&
                isVoided)
            .ToList();

        foreach (var item in voidedData)
        {
            voidedRow++;
            for (int col = 0; col < displayColumns.Count; col++)
                ws.Cells[voidedRow, col + 1].Value = item[displayColumns[col]];
        }

        //INVENTORY SECTION

        int startCol = 1;
        int currentRow = 1;
        int headerRow;
        int headerCount;

        //Stockroom to Office
        var InvTransactions = _context.InventoryTransactions
        .Include(t => t.Product)
        .Include(t => t.SourceLocation)
        .Include(t => t.DestinationLocation)
        .Where(t => t.DestinationLocation.OfficeId == officeId && t.TransactionDate >= start &&
                        t.TransactionDate <= end && t.FromLocationId == 2) //2 for Stockroom
        .OrderByDescending(t => t.TransactionDate)
        .ToList();

        if (InvTransactions.Count > 0)
        {
            int invRow = voidedRow + 4;

            ws.Cells[invRow - 1, 1].Value = "Product IN from Stock Room";
            ws.Cells[invRow - 1, 1].Style.Font.Bold = true;

            var invResult = new List<Dictionary<string, object>>();

            foreach (var item in InvTransactions)
            {
                var rowInv = new Dictionary<string, object>();

                rowInv["Date"] = item.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss");
                rowInv["Office"] = item.SourceLocation.Name;
                rowInv["Product"] = item.Product.ProductName;
                rowInv["Type"] = item.Product.ProductType;
                rowInv["Qty"] = item.Quantity;

                invResult.Add(rowInv);
            }

            //int startCol = 1;
            currentRow = invRow;

            // 1️⃣ Write headers (once)
            if (invResult.Any())
            {
                int col = startCol;
                foreach (var header in invResult.First().Keys)
                {
                    ws.Cells[currentRow, col++].Value = header;
                }
                currentRow++;
            }

            // 2️⃣ Write data rows
            foreach (var irow in invResult)
            {
                int col = startCol;
                foreach (var value in irow.Values)
                {
                    ws.Cells[currentRow, col++].Value = value;
                }
                currentRow++;
            }

            // Main Header Styling
            headerRow = invRow;
            headerCount = invResult.First().Count;
            using (var range = ws.Cells[headerRow, startCol, headerRow, startCol + headerCount - 1])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.GreenYellow);
                range.Style.Font.Color.SetColor(Color.DarkGreen);
            }
        }

        //Other Office to Selcted Office
        int?[] excludedLocations = { 1, 2 };

        var InvTransactionsIn = _context.InventoryTransactions
        .Include(t => t.Product)
        .Include(t => t.SourceLocation)
        .Include(t => t.DestinationLocation)
        .Where(t => t.DestinationLocation.OfficeId == officeId && t.TransactionDate >= start &&
                        t.TransactionDate <= end && !excludedLocations.Contains(t.FromLocationId)) //Do not get Warehouse and Stockroom
        .OrderByDescending(t => t.TransactionDate)
        .ToList();

        if (InvTransactionsIn.Count > 0)
        {
            int invRowIn = currentRow + 4;

            ws.Cells[invRowIn - 1, 1].Value = "Product IN from Other Office";
            ws.Cells[invRowIn - 1, 1].Style.Font.Bold = true;

            var invResultIn = new List<Dictionary<string, object>>();

            foreach (var item in InvTransactionsIn)
            {
                var rowInv = new Dictionary<string, object>();

                rowInv["Date"] = item.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss");
                rowInv["Office"] = item.SourceLocation.Name;
                rowInv["Product"] = item.Product.ProductName;
                rowInv["Type"] = item.Product.ProductType;
                rowInv["Qty"] = item.Quantity;

                invResultIn.Add(rowInv);
            }

            startCol = 1;
            currentRow = invRowIn;

            // 1️⃣ Write headers (once)
            if (invResultIn.Any())
            {
                int col = startCol;
                foreach (var header in invResultIn.First().Keys)
                {
                    ws.Cells[currentRow, col++].Value = header;
                }
                currentRow++;
            }

            // 2️⃣ Write data rows
            foreach (var irow in invResultIn)
            {
                int col = startCol;
                foreach (var value in irow.Values)
                {
                    ws.Cells[currentRow, col++].Value = value;
                }
                currentRow++;
            }

            // Main Header Styling
            headerRow = invRowIn;
            headerCount = invResultIn.First().Count;
            using (var range = ws.Cells[headerRow, startCol, headerRow, startCol + headerCount - 1])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.GreenYellow);
                range.Style.Font.Color.SetColor(Color.DarkGreen);
            }
        }

        ws.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }





}
