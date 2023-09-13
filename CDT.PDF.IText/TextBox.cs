using System;
using System.Collections.Generic;
using System.Text;

namespace CDT.PDF.IText {
	class TextBox {
		public double X { get; set; }
		public double Y { get; set; }
		public double Width { get; set; }
		public double Height { get; set; }
		public string Text { get; set; }
		public float FontSize { get; set; }

		public TextBox() {
		}
	}
}
