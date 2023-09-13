using iText.Kernel.Events;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.IO;

namespace CDT.WARS {
	public class PageRotation {
		public static readonly PdfNumber INVERTEDPORTRAIT = new PdfNumber(180);
		public static readonly PdfNumber LANDSCAPE = new PdfNumber(90);
		public static readonly PdfNumber PORTRAIT = new PdfNumber(0);
		public static readonly PdfNumber SEASCAPE = new PdfNumber(270);

		public void ManipulatePdf(string dest) {
			var eventHandler = new PageRotationEventHandler();

			using (PdfDocument pdfDoc = new PdfDocument(new PdfWriter(dest))) {
				pdfDoc.AddEventHandler(PdfDocumentEvent.START_PAGE, eventHandler);

				using (var doc = new Document(pdfDoc)) {
					//Portrait (0)
					eventHandler.SetRotation(PORTRAIT);
					doc.Add(new Paragraph($"Hello World!\nRotation: {PORTRAIT}"));

					//landscape (90)
					eventHandler.SetRotation(LANDSCAPE);
					doc.Add(new AreaBreak());
					doc.Add(new Paragraph($"Hello World!\nRotation: {LANDSCAPE}"));

					//inverted portrait (180)
					eventHandler.SetRotation(INVERTEDPORTRAIT);
					doc.Add(new AreaBreak());
					doc.Add(new Paragraph($"Hello World!\nRotation: {INVERTEDPORTRAIT}"));

					//Seascape (270)
					eventHandler.SetRotation(SEASCAPE);
					doc.Add(new AreaBreak());
					doc.Add(new Paragraph($"Hello World!\nRotation: {SEASCAPE}"));

					doc.Close();
				}
			}
		}
	}


	internal class PageRotationEventHandler : IEventHandler {
		protected PdfNumber rotation = PageRotation.PORTRAIT;

		public void SetRotation(PdfNumber orientation) {
			this.rotation = orientation;
		}

		public void HandleEvent(Event @event) {
			PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
			docEvent.GetPage().Put(PdfName.Rotate, rotation);
		}
	}
}