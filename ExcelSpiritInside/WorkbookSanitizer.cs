using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ExcelSpiritInside
{
    /// <summary>
    /// Repairs workbooks that ClosedXML cannot load: broken relationship ids
    /// (images, comments, drawings) and out-of-range style indices.
    /// Only cell values matter for diffing, so visual parts are dropped.
    /// </summary>
    internal static class WorkbookSanitizer
    {
        private const string RelationshipNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

        public static void Sanitize(string path)
        {
            using var doc = SpreadsheetDocument.Open(path, true);
            var wbPart = doc.WorkbookPart ?? throw new InvalidDataException("Missing workbook part.");

            int xfCount = wbPart.WorkbookStylesPart?.Stylesheet?.CellFormats?.Elements<CellFormat>().Count() ?? 0;

            foreach (var wsPart in wbPart.WorksheetParts.ToList())
            {
                var ws = wsPart.Worksheet;
                if (ws == null)
                {
                    continue;
                }

                RemoveVisualParts(wsPart, ws);
                RemoveDanglingReferences(wsPart, ws);
                ClampStyleIndices(ws, xfCount);

                ws.Save();
            }

            FixStylesheet(wbPart.WorkbookStylesPart?.Stylesheet);
            wbPart.Workbook.Save();
        }

        private static void RemoveVisualParts(WorksheetPart wsPart, Worksheet ws)
        {
            foreach (var element in ws.Elements<Drawing>().Cast<OpenXmlElement>()
                         .Concat(ws.Elements<LegacyDrawing>())
                         .Concat(ws.Elements<LegacyDrawingHeaderFooter>())
                         .Concat(ws.Elements<Picture>())
                         .ToList())
            {
                element.Remove();
            }

            if (wsPart.DrawingsPart != null)
            {
                wsPart.DeletePart(wsPart.DrawingsPart);
            }
            wsPart.DeleteParts(wsPart.VmlDrawingParts.ToList());
            wsPart.DeleteParts(wsPart.ImageParts.ToList());
            wsPart.DeleteParts(wsPart.EmbeddedControlPersistenceParts.ToList());
            wsPart.DeleteParts(wsPart.EmbeddedObjectParts.ToList());
        }

        private static void RemoveDanglingReferences(WorksheetPart wsPart, Worksheet ws)
        {
            var validIds = new HashSet<string>(
                wsPart.Parts.Select(p => p.RelationshipId)
                    .Concat(wsPart.ExternalRelationships.Select(r => r.Id))
                    .Concat(wsPart.HyperlinkRelationships.Select(r => r.Id))
                    .Concat(wsPart.DataPartReferenceRelationships.Select(r => r.Id)),
                StringComparer.Ordinal);

            foreach (var element in ws.Descendants().ToList())
            {
                if (element.Parent == null)
                {
                    continue;
                }

                var hasBrokenRef = element.GetAttributes()
                    .Any(a => a.NamespaceUri == RelationshipNamespace && a.Value != null && !validIds.Contains(a.Value));
                if (hasBrokenRef)
                {
                    element.Remove();
                }
            }

            var tableParts = ws.Elements<TableParts>().FirstOrDefault();
            if (tableParts != null)
            {
                tableParts.Count = (uint)tableParts.Elements<TablePart>().Count();
            }
        }

        private static void ClampStyleIndices(Worksheet ws, int xfCount)
        {
            if (xfCount <= 0)
            {
                return;
            }

            foreach (var col in ws.Descendants<Column>())
            {
                if (col.Style != null && col.Style.Value >= xfCount)
                {
                    col.Style = 0u;
                }
            }

            foreach (var row in ws.Descendants<Row>())
            {
                if (row.StyleIndex != null && row.StyleIndex.Value >= xfCount)
                {
                    row.StyleIndex = 0u;
                }

                foreach (var cell in row.Elements<Cell>())
                {
                    if (cell.StyleIndex != null && cell.StyleIndex.Value >= xfCount)
                    {
                        cell.StyleIndex = 0u;
                    }
                }
            }
        }

        private static void FixStylesheet(Stylesheet? stylesheet)
        {
            if (stylesheet == null)
            {
                return;
            }

            int fontCount = stylesheet.Fonts?.Elements<DocumentFormat.OpenXml.Spreadsheet.Font>().Count() ?? 0;
            int fillCount = stylesheet.Fills?.Elements<Fill>().Count() ?? 0;
            int borderCount = stylesheet.Borders?.Elements<Border>().Count() ?? 0;
            var numFmtIds = new HashSet<uint>(
                stylesheet.NumberingFormats?.Elements<NumberingFormat>()
                    .Where(n => n.NumberFormatId != null)
                    .Select(n => n.NumberFormatId!.Value) ?? Enumerable.Empty<uint>());

            var xfs = (stylesheet.CellFormats?.Elements<CellFormat>() ?? Enumerable.Empty<CellFormat>())
                .Concat(stylesheet.CellStyleFormats?.Elements<CellFormat>() ?? Enumerable.Empty<CellFormat>());

            foreach (var xf in xfs)
            {
                if (xf.FontId != null && xf.FontId.Value >= fontCount) xf.FontId = 0u;
                if (xf.FillId != null && xf.FillId.Value >= fillCount) xf.FillId = 0u;
                if (xf.BorderId != null && xf.BorderId.Value >= borderCount) xf.BorderId = 0u;
                if (xf.NumberFormatId != null && xf.NumberFormatId.Value >= 164 && !numFmtIds.Contains(xf.NumberFormatId.Value))
                {
                    xf.NumberFormatId = 0u;
                    xf.ApplyNumberFormat = null;
                }
            }

            stylesheet.Save();
        }
    }
}
