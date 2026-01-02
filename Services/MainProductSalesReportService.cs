using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using OfficeOpenXml;
using System.ComponentModel;

public class MainProductSalesReportService
{
    private readonly ApplicationDbContext _context;

    public MainProductSalesReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Dictionary<string, object>>> GetOrderPivotReport(
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

        var mainProductDict = mainProducts
            .ToDictionary(p => p.Id, p => p.ProductName);

        var combos = await _context.ProductCombos
            .Where(c => c.IsActive)
            .ToListAsync();

        var comboLookup = combos
            .GroupBy(c => c.ProductId)
            .ToDictionary(g => g.Key, g => g.First());

        var orders = await _context.Orders
            .Where(o => o.OrderDate >= start &&
                        o.OrderDate <= end &&
                        !o.IsVoided &&
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
                //row["Office"] = order.Office?.Name ?? "";
                row["Unit Price"] = d.Price.ToString("N2");
                row["Total Amount"] = d.TotalPrice.ToString("N2");

                // Itemized product row
                row["Product"] = d.Products?.ProductName ?? "";
                row["Item Qty"] = d.Quantity;

                // Init pivot columns
                foreach (var mp in mainProducts)
                    row[mp.ProductName] = 0;

                // 1️⃣ REGULAR MAIN PRODUCT
                if (mainProductDict.TryGetValue(d.ProductId.Value, out var col))
                {
                    row[col] = d.Quantity;
                }

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

        return result;
    }

    public byte[] ExportOrderPivotToExcel(List<Dictionary<string, object>> data)
    {
        //ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        using (var package = new ExcelPackage())
        {
            var ws = package.Workbook.Worksheets.Add("Order Report");

            if (data == null || data.Count == 0)
                return package.GetAsByteArray();

            var columns = data.First().Keys.ToList();

            for (int i = 0; i < columns.Count; i++)
                ws.Cells[1, i + 1].Value = columns[i];

            int row = 2;
            foreach (var item in data)
            {
                for (int col = 0; col < columns.Count; col++)
                {
                    ws.Cells[row, col + 1].Value = item[columns[col]];
                }
                row++;
            }

            ws.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }
    }
}
