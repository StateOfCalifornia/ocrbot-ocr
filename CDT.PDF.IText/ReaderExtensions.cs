using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CDT.PDF.IText {
	public static class ReaderExtensions {

		//static FieldInfo LocationalResultField => typeof(LocationTextExtractionStrategy).GetField("locationalResult", BindingFlags.NonPublic | BindingFlags.Instance);


		public static Boolean HasText(PdfPage page) {
			var textEventListener = new LocationTextExtractionStrategy();
			var pageText = PdfTextExtractor.GetTextFromPage(page, textEventListener);
			//var locationalResult = LocationalResultField.GetValue(textEventListener) as IList<TextChunk>;
			var result = !String.IsNullOrWhiteSpace(pageText);
			return result;
		}
	}
}
