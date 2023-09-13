using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CDT.PDF.IText {
    class CustomSplitter : PdfSplitter {
        private int _order;
        private readonly string _baseName;
		private readonly string _targetDirectory;
        private static IList<string> FileNames;

        public CustomSplitter(string pdfDocument, string destinationFile) : base(new PdfDocument(new PdfReader(pdfDocument))) {
			_targetDirectory = Path.Combine(Path.GetDirectoryName(destinationFile), "~TEMPSPLITDIR~");
			 Directory.CreateDirectory(_targetDirectory);
            _baseName = Path.GetFileNameWithoutExtension(pdfDocument);
            _order = 0;
            FileNames = new List<string>();
        }

        protected override PdfWriter GetNextPdfWriter(PageRange documentPageRange) {
            var fileName = Path.Combine(_targetDirectory, $"{_baseName}_{_order++}.pdf");
            FileNames.Add(fileName);
            return new PdfWriter(fileName);
        }

		public IList<string> GetSplitFiles() {
			return FileNames;
		}
    }
}