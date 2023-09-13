using CDT.Helpers;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Colorspace;
using iText.Kernel.Pdf.Extgstate;
using iText.Kernel.Utils;
using iText.Layout;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using OcrLine = Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models.Line;

namespace CDT.PDF.IText {
	public class PdfClient {
		/// <summary>
		/// configure how the merged OCR text will look
		/// </summary>
		struct OcrOptions {
			/// <summary>
			/// text render mode - normal/default is Fill
			/// HACK: making text invisible as workaround for scans with transparent backgrounds
			/// </summary>
			public int TextRenderingMode { get; set; }

			/// <summary>
			/// True, to hide merged text layer under all other content
			/// else it will be placed on top
			/// </summary>
			public bool SendOcrToBack { get; set; }

			/// <summary>
			/// text transparency level
			/// </summary>
			public float? FillOpacity { get; set; }

			/// <summary>
			/// the color to use for the merged OCR text
			/// </summary>
			public Color TextColor { get; set; }
		}


		private OcrOptions ocrOptions { get; set; }
		private OcrOptions ocrOptionsTextOnly { get; set; }


		#region #DEBUG
		[Conditional("DEBUG")]
		private void SetDebug() {
			//show text overlay to make debugging easier
			ocrOptions = new OcrOptions() {
				FillOpacity = 0.0f, //  0.7f,  // LAYERING WILL NOT WORK WIHT THIS SET TO > 0
				TextColor = ColorConstants.GRAY,
				TextRenderingMode = PdfCanvasConstants.TextRenderingMode.FILL,
				SendOcrToBack = false
			};
		}

		[Conditional("DEBUG")]
		private void TracePageLayout(PdfPage pdfPage, TextRecognitionResult ocrPage, string sourceDocumentPath) {
			GetPageLayout(pdfPage, out int pageRotation, out double pageWidth, out double pageHeight, out double mediaX, out double mediaY);
			GetTextLayoutPdf(ocrPage, out int ocrRotation, out double ocrWidth, out double ocrHeight);

			Console.WriteLine($"{pageRotation}    {ocrRotation}    {mediaX}    {mediaY}    {pageWidth}    {pageHeight}    {ocrPage.Page.Value}    {sourceDocumentPath}");
		}
		#endregion #DEBUG



		private void Initialize() {
			ocrOptions = new OcrOptions() {
				FillOpacity = 0.0f,
				TextColor = ColorConstants.GRAY,
				TextRenderingMode = PdfCanvasConstants.TextRenderingMode.FILL,
				SendOcrToBack = false
			};

			ocrOptionsTextOnly = new OcrOptions() {
				FillOpacity = 0.0f,
				TextColor = ColorConstants.BLACK,
				TextRenderingMode = PdfCanvasConstants.TextRenderingMode.FILL,
				SendOcrToBack = false
			};
		}


		//will run with no license
		public PdfClient() {
			Initialize();
			SetDebug();
		}
		public PdfClient(Stream license) : this() {
			if (license != null) {
				iText.License.LicenseKey.LoadLicenseFile(license);
			}
		}

		public static string LibraryInfo() {
			var version = iText.Kernel.Version.GetInstance();
			return version.GetVersion();
		}

		//ToDo: decouple Azure entities
		//ToDo: hoist all the font and layout options into configuration
		public void MergePdfOcr(string sourceDocumentPath, 
				IList<Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models.TextRecognitionResult> results, 
				string outputDocumentPath, CancellationToken cancelToken, out int mixedPageCount, bool skipMixedPages, bool textOnlyMode)
		{
			PdfDocument pdf = null;
            mixedPageCount = 0;
			try {
				if (results == null) {
					throw new ArgumentException("TextRecognition results not set.");
				}

				cancelToken.ThrowIfCancellationRequested();

				//create copy of original document for editing
				using (pdf = CopyPdf(sourceDocumentPath, outputDocumentPath)) {
					//use default font (e.g., Helvetica, which is fairly similar to Windows Arial)
					var font = pdf.GetDefaultFont();

					foreach (var ocrPage in results) {
						cancelToken.ThrowIfCancellationRequested();
						var pdfPage = pdf.GetPage(ocrPage.Page.Value);

						if (skipMixedPages &&  ReaderExtensions.HasText(pdfPage)){
                            mixedPageCount++;
							continue;
						}


						pdfPage.SetIgnorePageRotationForContent(true);
						TracePageLayout(pdfPage, ocrPage, sourceDocumentPath);

						var ocrOptionsToUse  = ocrOptions;
						if (textOnlyMode) {
							ocrOptionsToUse = ocrOptionsTextOnly;
						}
						var canvas = GetCanvas(pdfPage, ocrOptionsToUse);
						SyncCanvasRotation(canvas, ocrPage);
						var textBoxes = TranslateOcr(pdfPage, ocrPage, font);

						foreach (var textBox in textBoxes) {
							canvas.BeginText()
								.MoveText(textBox.X, textBox.Y)
								.SetFontAndSize(font, textBox.FontSize)
								.ShowText(textBox.Text)
								.EndText();
						}

						canvas.RestoreState(); //done editing; restore state so edits don't cause unintended side-effects to existing content
					}

					pdf.Close();
				}
			}
			catch (Exception ex) {
				//release file locks first
				if (pdf != null && !pdf.IsClosed()) { pdf.Close(); }

				//delete unfinished output file
				if (File.Exists(outputDocumentPath)) {
					File.Delete(outputDocumentPath);
				}

				//then bubble up all exceptions
				throw;
			}
		}


		/// <summary>
		/// counter rotate canvas because OCR results are already rotated
		/// </summary>
		private void SyncCanvasRotation(PdfCanvas canvas, TextRecognitionResult ocrPage) {
			GetTextLayoutPdf(ocrPage, out int ocrRotation, out double ocrWidth, out double ocrHeight);

			var angle = BoundingBox.MirrorRotation(ocrRotation);
			double cx = ocrWidth / 2;
			double cy = ocrHeight / 2;

			Rotate(canvas, angle, cx, cy);
		}

		private void Rotate(PdfCanvas canvas, double angle, double x, double y) {
			var radians = BoundingBox.DegreesToRadians(angle);
			var transform = AffineTransform.GetRotateInstance(radians, x, y);
			canvas.ConcatMatrix(transform);
		}

		/// <summary>
		/// translate OCR to PDF scale and coordinates
		/// </summary>
		private IEnumerable<TextBox> TranslateOcr(PdfPage pdfPage, TextRecognitionResult ocrPage, PdfFont font) {
			//get page and text layouts for calculating rotations and translations
			GetPageLayout(pdfPage, out var pageRotation, out var pageWidth, out var pageHeight, out var mediaX, out var mediaY);

			//add line as a block
			//per word would be more accurrate, but also inflates document size and complexity
			//assumes uniform line font for now for simplicity
			var textBoxes = new List<TextBox>(ocrPage.Lines.Count);

			foreach (var ocrLine in ocrPage.Lines) {
				var box = BoundingBoxFactory(ocrPage, ocrLine);

				//translate OCR to PDF scale and coordinates
				//undo text rotation to simplify syncing with canvas orientation
				box.ConvertUnits(BoxUnits.Points)
					.ResetRotation()
					.TranslateCoordinates(CoordinatesOrigin.Cartesian)
					;

				//calculate and scale font size to fit bounding box
				float fontSize = FitText(font, box.Text, box.Width(), box.Height());

				var textBox = new TextBox() {
					X = box.X() + mediaX,
					Y = box.Y() + mediaY,
					Height = box.Height(),
					Width = box.Width(),
					FontSize = fontSize,
					Text = box.Text
				};

				textBoxes.Add(textBox);
			}

			return textBoxes;
		}

		private BoundingBox BoundingBoxFactory(TextRecognitionResult ocrPage, OcrLine ocrLine) {
			var box = ocrLine.BoundingBox.ToArray();
			var units = BoundingBox.GetBoxUnits(ocrPage.Unit.ToString());
			var resolution = BoundingBox.DefaultResolution;  //HACK: figure out how to get real DPI (only needed for Pixel units)
			var text = ocrLine.Text.Trim() + " ";
			var origin = CoordinatesOrigin.Screen;

			var ocrRotation = ocrPage.ClockwiseOrientation.GetValueOrDefault();
			var ocrWidth = ocrPage.Width.Value;
			var ocrHeight = ocrPage.Height.Value;

			var bbox = new BoundingBox(box, origin, units, ocrRotation, resolution, ocrWidth, ocrHeight, text);

			return bbox;
		}

		private void GetPageLayout(PdfPage page, out int pageRotation, out double pageWidth, out double pageHeight, out double mediaX, out double mediaY) {
			pageRotation = page.GetRotation();

			var pageSize = page.GetPageSizeWithRotation();
			pageWidth = pageSize.GetWidth();
			pageHeight = pageSize.GetHeight();

			//adjust text position to match mediabox offsets
			//(page origin adjusted away from 0, 0)
			var media = page.GetMediaBox();
			double mx = media.GetX();
			double my = media.GetY();

			switch (pageRotation) {
				case 90:
					mediaX = +my;
					mediaY = -mx;
					break;

				case 270:
					mediaX = -my;
					mediaY = +mx;
					break;

				default:
					mediaX = +mx;
					mediaY = +my;
					break;
			}
		}

		private void GetTextLayoutPdf(TextRecognitionResult ocrPage, out int ocrRotation, out double ocrWidthPoints, out double ocrHeightPoints) {
			GetTextLayoutRaw(ocrPage, out double rotation, out double width, out double height);
			ocrRotation = BoundingBox.RoundToNearestOrientation(rotation);
			ocrWidthPoints = BoundingBox.ConvertUnitsInchesToPoints(width);
			ocrHeightPoints = BoundingBox.ConvertUnitsInchesToPoints(height);
		}

		private void GetTextLayoutRaw(TextRecognitionResult ocrPage, out double rotation, out double width, out double height) {
			rotation = ocrPage.ClockwiseOrientation.GetValueOrDefault();
			width = ocrPage.Width.Value;
			height = ocrPage.Height.Value;
		}

		/// <summary>
		/// initialize the canvas stream and set selected render options
		/// </summary>
		private PdfCanvas GetCanvas(PdfPage page, OcrOptions ocrOptions) {
			var canvas = new PdfCanvas(page);

			ApplyCanvasOptions(page, canvas, ocrOptions);
			
			return canvas;
		}

		private void ApplyCanvasOptions(PdfPage page, PdfCanvas canvas, OcrOptions ocrOptions) {
			//overlay / underlay
			var stream = ocrOptions.SendOcrToBack ? page.NewContentStreamBefore() : page.NewContentStreamAfter();
			canvas.AttachContentStream(stream);

			//set restore point to rollback to when done editing to avoid side-effects to existing content
			canvas.SaveState(); 

			//text render mode
			canvas.SetTextRenderingMode(ocrOptions.TextRenderingMode);

			//text opacity
			if (ocrOptions.FillOpacity.HasValue) { 
				canvas.SetExtGState(new PdfExtGState().SetFillOpacity(ocrOptions.FillOpacity.Value));
			}

			//text color
			if (ocrOptions.TextColor != null) {
				canvas.SetFillColor(ocrOptions.TextColor);
			}
		}

		/// <summary>
		/// scale the font size to fit the text inside the box
		/// </summary>
		/// <param name="font">what font to render the text with</param>
		/// <param name="text">string to be rendered</param>
		/// <param name="width">box width to contrain the text by</param>
		/// <param name="height">box height to constrain the text by</param>
		/// <returns>font size, rounded to the nearest half point</returns>
		private float FitText(PdfFont font, string text, double width, double height) {
			float fontSize = Convert.ToSingle(height);// = RoundToNearestHalf(height);

			//scale the width to overcome font differences and ensure line doesn't overrun width of box
			var glyphWidth = font.GetWidth(text) * .001f; //convert 'normalized 1000 units' per iso pdf spec
			float textWidth = glyphWidth * fontSize; //actual width at current font size
			var hScale = Convert.ToSingle(width) / textWidth;

			if (hScale < 1) {
				fontSize = fontSize * hScale;
			}

			//truncate to 1 decimal place (never round up)
			fontSize = Convert.ToSingle(Math.Floor(fontSize * 10f) / 10f);

			return fontSize;
		}

		/// <summary>
		/// create a working copy of a document
		/// </summary>
		/// <param name="sourceDocumentPath">read-only document to clone</param>
		/// <param name="outputDocumentPath">writable copy to create</param>
		/// <returns>unmanaged document open for editing</returns>
		private PdfDocument CopyPdf(string sourceDocumentPath, string outputDocumentPath) {
			//create copy of source doc and work with the copy
			var reader = new PdfReader(sourceDocumentPath); //read from source
			var writer = new PdfWriter(outputDocumentPath); //write to copy
			var pdf = new PdfDocument(reader, writer);

			return pdf;
		}

		/// <summary>
		/// Splits a PDF based on the limitations of the Azure Endpoint
		/// </summary>
		/// <param name="sourceFile">Original File</param>
		/// <param name="targetFile">Target File</param>
		/// <returns>unmanaged document open for editing</returns>
		public IList<string> SplitPdf(string sourceFile, string targetFile) {
			// HACK: Custom splitter is used to write to disc since the split strips the reader needed for the second split
			// so far we've only figured out how to get the reader back by closing and reopening the file...
			var pageSplitter = new CustomSplitter(sourceFile, targetFile);
			var pdfsByPages = pageSplitter.SplitByPageCount(200);
			var pdfsByPagesFileNames = pageSplitter.GetSplitFiles();

			IList<string> results = new List<string>();

			var pageSplitCount = 0;

			foreach(var pdfByPages in pdfsByPages) {
				// Must close the PDF, the next splitter will not work if it is being written. 
				pdfByPages.Close();

				// SplitByPageCount() strips out the required reader
				// HACK: Save and reopen file with a reader
				var sizeSplitter = new CustomSplitter(pdfsByPagesFileNames[pageSplitCount++], targetFile);
				var pdfsBySize = sizeSplitter.SplitBySize(20 * 1024 * 1024);
				var pdfsBySizeFileNames = sizeSplitter.GetSplitFiles();
			
				var sizeSplitCount = 0;

				foreach(var pdfBySize in pdfsBySize) {
					pdfBySize.Close();
					results.Add(pdfsBySizeFileNames[sizeSplitCount++]);
				}
			}

			return results;
		}


		public void DeleteTempSplitFolder(string destinationFile) {
			if (string.IsNullOrEmpty(destinationFile)) {
				return;
			}
			var targetDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(destinationFile), "~TEMPSPLITDIR~");
			try {
				if (Directory.Exists(targetDirectory)) {
					Directory.Delete(targetDirectory, true);
				}
			}
			catch (Exception) { }
		}
	}
}

