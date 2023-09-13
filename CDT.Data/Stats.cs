using System;
using System.Collections.Generic;
using System.Text;

namespace CDT.Data {
	public class Stats {
		private TimeSpan SessionTime { get; set; }
		private DateTime? StartTime { get; set; }
		private DateTime? EndTime { get; set; }
		private bool IsRunning { get; set; }


		public int FileProcessedCount { get; set; }

		public int PagesProcessedCount { get; set; }

		public int FileErrorCount { get; set; }

		public string StatusMessage { get; set; }


		public TimeSpan RunTime() {
			if (IsRunning) {
				//on-the-fly calc of current elapsed time
				var runTime = DateTime.Now - StartTime.Value;
				return runTime;
			}
			else {
				//use final finish time
				return SessionTime;
			}
		}

		public void Start() {
			if (!IsRunning) {
				Initialize();
				IsRunning = true;
			}
		}

		public void Stop() {
			if (IsRunning) {
				EndTime = DateTime.Now;
				SessionTime = EndTime.Value - StartTime.Value;
				IsRunning = false;

				if (FileErrorCount > 0) {
					StatusMessage = "Task Completed with errors";
				}
				else {
					StatusMessage = "Task Completed";
				}
			}
		}


		public override string ToString() {
			var delimiter = $"{Environment.NewLine}    ";

			var formatted = $"{StatusMessage}:{delimiter}" +
				$"Time Taken: {ToTimeString()}{delimiter}" + 
				$"Files Processed: {FileProcessedCount}{delimiter}" + 
				$"Pages Processed: {PagesProcessedCount}{delimiter}" + 
				$"File Errors: {FileErrorCount}";

			return formatted;
		}

		/// <summary>
		/// Current/Final elapsed time formatted as h:mm:ss
		/// </summary>
		/// <returns></returns>
		public string ToTimeString() {
			var ts = RunTime();
			var formatted =$"{Math.Truncate(ts.TotalHours)}:{ts.ToString(@"mm\:ss")}";

			return formatted;
		}



		// private to protect state
		// should only be called as part of a start
		private void Initialize() {
			IsRunning = false;

			FileErrorCount = 0;
			PagesProcessedCount = 0;
			FileProcessedCount = 0;

			SessionTime = TimeSpan.Zero;
			StartTime = DateTime.Now;
			EndTime = null;
			StatusMessage = String.Empty;
		}
	}
}
