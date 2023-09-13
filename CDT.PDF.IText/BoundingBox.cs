using iText.Kernel.Geom;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;

namespace CDT.PDF.IText {
	public struct Orientation {
		public static int Portrait => 0;
		public static int Landscape => 90;
		public static int InversePortrait => 180;
		public static int Seascape => 270;
	}
	
	public enum BoxUnits {
		Unknown = 0,
		Inches,
		Pixels,
		Points
	}

	public enum CoordinatesOrigin {
		NotSet = 0,

		/// <summary>
		/// Origin is in the Lower Left corner of the page, abbreviated as (llx, lly)
		/// <para>X, Y values increase to the RIGHT and UP, towards the upper right corner of the page</para>
		/// </summary>
		Cartesian,

		/// <summary>
		/// Origin is in the Upper Left corner of the page, abbreviated as (ulx, uly)
		/// <para>X, Y values increase to the RIGHT and DOWN, towards the bottom right corner of the page</para>
		/// </summary>
		Screen
	}


	/// <summary>
	/// rectangle is anchored by lower left corner and drawn in a clockwise direction 
	/// </summary>
	public struct BoxPoints {
		public static int LowerLeftX => 0;
		public static int LowerLeftY => 1;

		public static int LowerRightX => 2;
		public static int LowerRightY => 3;

		public static int UpperRightX => 4;
		public static int UpperRightY => 5;

		public static int UpperLeftX => 6;
		public static int UpperLeftY => 7;

		/// <summary>
		/// always return the first X value listed; the origin
		/// </summary>
		public static int OriginX => LowerLeftX;
		/// <summary>
		/// always return the first Y value listed; the origin
		/// </summary>
		public static int OriginY => LowerLeftY;
	}


	public class BoundingBox {
		private static int OrientationIncrementAngle => 90;
		private static int OrientationFullRotation => 360;
		private static int OrientationAngleCount => OrientationFullRotation / OrientationIncrementAngle;
		private static int BoxNumberOfEdges => 4;

		public static double PointsPerInch => 72d;
		//A.K.A: DPI, e.g Dots per Inch (pixels)
		public static double DefaultResolution => 96d;/*72d*/

		public double[] Box { get; private set; }
		public CoordinatesOrigin Origin { get; private set; }

		public int TextOrientation { get; private set; }
		public BoxUnits Units { get; private set; }
		public double Resolution { get; private set; }
		public string Text { get; private set; }
		public double PageWidth { get; private set; }
		public double PageHeight { get; private set; }

		/// <summary>
		/// X value of box origin (llx); e.g. the lower left corner
		/// <para>note, PDF uses Cartesian coordinates; origin is in lower left corner (llx, lly)\n
		/// Scanner/OCR use Screen coordinates are vertically inverted; origin is upper left corner of document (ulx, uly)
		/// </summary>
		public double X () {
			double x = this.Box[BoxPoints.LowerLeftX];
			return x;
		}

		/// <summary>
		/// Y value of box origin (lly); e.g. the lower left corner
		/// <para>note, PDF uses Cartesian coordinates; origin is in lower left corner (llx, lly)\n
		/// Scanner/OCR use Screen coordinates are vertically inverted; origin is upper left corner of document (ulx, uly)
		/// </summary>
		public double Y () {
			double y = this.Box[BoxPoints.LowerLeftY];
			return y;
		}

		/// <summary>
		/// get the width of the text, adjusting for rotation
		/// <para>negative values indicate text is mirrored, e.g. rotated</para>
		/// </summary>
		public double Width() {
			double width;

			//portrait/inverted orientation
			var x1 = this.Box[BoxPoints.LowerLeftX];
			var y1 = this.Box[BoxPoints.LowerLeftY];
			var x2 = this.Box[BoxPoints.LowerRightX];
			var y2 = this.Box[BoxPoints.LowerRightY];

			width = Distance(x1, y1, x2, y2);
			return width;
		}

		/// <summary>
		/// get the width of the text, adjusting for rotation
		/// <para>negative values indicate text is mirrored, e.g. rotated</para>
		/// </summary>
		public double Height() {
			double height;

			var x1 = this.Box[BoxPoints.LowerLeftX];
			var y1 = this.Box[BoxPoints.LowerLeftY];
			var x2 = this.Box[BoxPoints.UpperLeftX];
			var y2 = this.Box[BoxPoints.UpperLeftY];

			height = Distance(x1, y1, x2, y2);
			return height;
		}


		public BoundingBox(double[] box, CoordinatesOrigin origin, BoxUnits units, double rotation, double resolution, double pageWidth, double pageHeight, string text) {
			Init(box, origin, units, rotation, resolution, pageWidth, pageHeight, text);
		}


		private void Init(double[] box, CoordinatesOrigin origin, BoxUnits units, double rotation, double resolution, double pageWidth, double pageHeight, string text) {
			this.Box = box;
			this.Origin = origin;
			this.Units = units;
			this.TextOrientation = RoundToNearestOrientation(rotation);
			this.Resolution = resolution;
			this.PageWidth = pageWidth;
			this.PageHeight = pageHeight;
			this.Text = text;
		}

		/// <summary>
		/// rotate to 0 degrees (portrait)
		/// </summary>
		public BoundingBox ResetRotation() {
			//counter rotate back to 0
			int angle = MirrorRotation(this.TextOrientation);
			Rotate(angle);

			return this;
		}

		private void Rotate(int angle) {
			double cx = PageWidth / 2;
			double cy = PageHeight / 2;

			Rotate(angle, cx, cy);
		}


		private void Rotate(int angle, double cx, double cy) {
			var rotated = new double[Box.Length];
			var radians = DegreesToRadians(angle);
			var sinR = Math.Sin(radians);
			var cosR = Math.Cos(radians);

			//transform coordinates
			for (var i = 0; i < Box.Length; i += 2) {
				var j = i + 1;
				//current point x,y
				var x = Box[i];
				var y = Box[j];
				//temporarily center box about the origin
				var xOffset = (x - cx);
				var yOffset = (y - cy);

				//transform x,y, then move back (from origin) to position
				rotated[i] = (xOffset * cosR) - (yOffset * sinR) + cx;
				rotated[j] = (xOffset * sinR) + (yOffset * cosR) + cy;
			}

			rotated.CopyTo(Box, 0);
		}

		public BoundingBox TranslateCoordinates(CoordinatesOrigin coordinatesOrigin) {
			switch (coordinatesOrigin) {
				case CoordinatesOrigin.Cartesian:
					TranslateCoordinatesToCartesian();
					break;

				case CoordinatesOrigin.Screen:
					TranslateCoordinatesToScreen();
					break;
			}

			return this;
		}

		/// <summary>
		/// translate between grid coordinate systems
		/// </summary>
		private void TranslateCoordinatesToCartesian() {
			AffineTransform transform;
			double[] cartesianBox;

			//from origin
			switch (Origin) {
				//mirror Y axis
				case CoordinatesOrigin.Screen:
					//invert vertical position on page
					//use (screen) bottom of box because that is the new Y origin
					var top = PageHeight - Box[BoxPoints.UpperLeftY] - Box[BoxPoints.LowerLeftY];
					transform = new AffineTransform();
					transform.Translate(0, top);

					break;
				default:
					throw new NotImplementedException();
			}

			cartesianBox = Transform(transform, Box);
			cartesianBox.CopyTo(Box, 0);
		}

		/// <summary>
		/// translate between grid coordinate systems
		/// </summary>
		/// <param name="box"></param>
		private void TranslateCoordinatesToScreen() {
			switch (Origin) {
				case CoordinatesOrigin.Cartesian:
				default:
					throw new NotImplementedException();
			}
		}

		private double[] Transform(AffineTransform transform, double[] source) {
			double[] target = new double[Box.Length];
			var edgeCount = source.Length / 2;

			transform.Transform(source, 0, target, 0, edgeCount);

			return target;
		}

		#region Units
		public static BoxUnits GetBoxUnits(string unitName) {
			unitName = unitName.ToLower();
			BoxUnits units;

			if (unitName.StartsWith("pixel")) {
				units = BoxUnits.Pixels;
			}
			else if (unitName.StartsWith("inch")) {
				units = BoxUnits.Inches;
			}
			else if (unitName.StartsWith("point")) {
				units = BoxUnits.Points;
			}
			else {
				units = BoxUnits.Unknown;
			}

			return units;
		}

		/// <summary>
		/// convert box values to the specified <paramref name="units"/>
		/// </summary>
		/// <see cref="Units"/>
		/// <see cref="BoxUnits"/>
		/// <returns>this instance with internal rectangle updated to <see cref="BoxUnits.Points"/> coordinates</returns>
		public BoundingBox ConvertUnits(BoxUnits units) {
			switch (units) {
				case BoxUnits.Points:
					ConvertUnitsToPoints();
					break;

				default:
					throw new NotImplementedException($"Conversion to {units} has not been implimented");
			}

			return this;
		}

		/// <summary>
		/// convert coordinates from the current units to <see cref="BoxUnits.Points"/>
		/// </summary>
		/// <returns>this instance with internal rectangle updated to <see cref="BoxUnits.Points"/> coordinates</returns>
		private BoundingBox ConvertUnitsToPoints() {
			this.Box = ConvertUnitsToPoints(this.Box, this.Units, this.Resolution);
			this.PageWidth = ConvertUnitsToPoints(this.PageWidth, this.Units, this.Resolution);
			this.PageHeight = ConvertUnitsToPoints(this.PageHeight, this.Units, this.Resolution);
			this.Units = BoxUnits.Points;

			return this;
		}

		/// <summary>
		/// convert coordinates from the current units to <see cref="BoxUnits.Points"/>
		/// </summary>
		/// <param name="box">the rectangle to be converted</param>
		/// <param name="units">units to convert from</param>
		/// <param name="resolution">image resolution; only needed if source <paramref name="units"/> is <see cref="BoxUnits.Pixels"/></param>
		/// <returns>copy of <paramref name="box"/> converted to <see cref="BoxUnits.Points"/></returns>
		public static double[] ConvertUnitsToPoints(double[] box, BoxUnits units, double resolution) {
			var points = new double[box.Length];

			switch (units) {
				case BoxUnits.Inches:
					points = ConvertUnitsInchesToPoints(box);
					break;

				case BoxUnits.Pixels:
					points = ConvertUnitsPixelsToPoints(box, resolution);
					break;

				case BoxUnits.Points:
					//nothing to do
					break;

				default:
					throw new ArgumentException("Cannot convert Unknown units into Points");
			}

			return points;
		}

		public static double ConvertUnitsToPoints(double value, BoxUnits units, double resolution) {
			double points;

			switch (units) {
				case BoxUnits.Inches:
					points = ConvertUnitsInchesToPoints(value);
					break;

				case BoxUnits.Pixels:
					points = ConvertUnitsPixelsToPoints(value, resolution);
					break;

				case BoxUnits.Points:
					//nothing to do
					points = value;
					break;

				default:
					throw new ArgumentException("Cannot convert Unknown units into Points");
			}

			return points;
		}

		private static double[] ConvertUnitsInchesToPoints(double[] inches) {
			var points = new double[inches.Length];

			for (var i = 0; i < inches.Length; i++) {
				points[i] = ConvertUnitsInchesToPoints(inches[i]);
			}

			return points;
		}

		public static double ConvertUnitsInchesToPoints(double inches) {
			var points = (inches * BoundingBox.PointsPerInch);

			return points;
		}

		private static double[] ConvertUnitsPixelsToPoints(double[] pixels, double resolution) {
			var points = new double[pixels.Length];

			for (var i = 0; i < pixels.Length; i++) {
				points[i] = ConvertUnitsPixelsToPoints(pixels[i], resolution);
			}

			return points;
		}

		private static double ConvertUnitsPixelsToPoints(double pixels, double resolution) {
			var inches = pixels / resolution;
			var points = ConvertUnitsInchesToPoints(inches);

			return points;
		}
		#endregion Units

		#region Rotation
		public static double DegreesToRadians(double degrees) {
			var radians = Math.PI * degrees / 180.0;

			return radians;
		}

		public static double RadiansToDegrees(double radians) {
			var degrees = radians * (180.0 / Math.PI);

			return degrees;
		}

		public static int MirrorRotation(int rotation) {
			var mirror = NormalizeRotation(-rotation);

			return mirror;
		}

		public static int NormalizeRotation(int rotation) {
			var turns = (rotation / OrientationIncrementAngle) % OrientationAngleCount;
			var normalizedRotation = turns * OrientationIncrementAngle;

			if (normalizedRotation < 0) {
				normalizedRotation += OrientationFullRotation;
			}

			return normalizedRotation;
		}

		/// <summary>
		/// round the page skew to the nearest right angle
		/// </summary>
		/// <param name="angle"></param>
		/// <returns></returns>
		public static int RoundToNearestOrientation(double angle) {
			int orientation;

			//Seascape
			if (angle > 265 && angle < 275) {
				orientation = 270;
			}
			//Inverse Portrait
			else if (angle > 175 && angle < 185) {
			orientation = 180;
			}
			//Landscape
			else if (angle > 85 && angle < 95) {
			orientation = 90;
			}
			//Portrait
			else {//if (angle > 360 || angle < 5) {
				orientation = 0;
			}

			return orientation;
		}
		#endregion Rotation

		/// <summary>
		/// get the distance between two points
		/// </summary>
		public static double Distance(double x1, double y1, double x2, double y2) {
			//pythagorean theorem c^2 = a^2 + b^2
			var a = x2 - x1;
			var b = y2 - y1;
			var distance = Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));

			return distance;
		}
	}
}
