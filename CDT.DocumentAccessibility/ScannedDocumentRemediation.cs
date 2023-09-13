using CDT.OCR.Microsoft.Azure;
using CDT.PDF.IText;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using CDT.Log;
using System;
using System.IO;
using System.Linq;
using CDT.Helpers;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using CDT.Acrobat;
using iText.Kernel.Pdf;
using CDT.Data;

namespace CDT.Accessibility.Documents {
	public class ScannedDocumentRemediation {
		private static readonly string TempFileSuffix = ".OCRBotTemp.pdf";

		//disabling image formats for now until support added for them
		public static string[] SupportedFileTypesExtensions => new string[] { ".pdf"/*, ".jpg", ".jpeg", ".bmp", ".png", ".tiff"*/ };
		public static string SupportedFileTypesFilter => "*" + String.Join(";*", SupportedFileTypesExtensions);
		public static string SupportedFileTypesFilterDescription => "*" + String.Join(", *", SupportedFileTypesExtensions);


		private static string[] FatalExceptions = new string[] {
			"invalid request URI",
			"PermissionDenied",
			"Permission Denied",
			"An error occurred while sending the request.",
			"The remote name could not be resolved"
		};

		private ComputerVisionClient cvClient;
		private PdfClient pdfClient;
		private AcrobatClient acrobatClient;
		private readonly Action<LogEntry> LogAction;
		private Stats stats;

		//workflow gates
		private bool EnableOcrMerge = true;
		//only if debug
		private bool EnableSaveOcrMetadata = false;
		private bool EnableSaveOcrText = false;

		private int MaxAzurePages = 200;
		private int MaxAzureFileSize = 20 * 1024 * 1024;


		[Conditional("DEBUG")]
		private void SetDebug() {
			EnableSaveOcrMetadata = true;
			EnableSaveOcrText = true;
		}


		public ScannedDocumentRemediation(
			CognitiveServicesConfiguration.ClientConfiguration config,
			Stats stats,
			AcrobatClient acrobatClient,
			Action<LogEntry> logAction = null,
			Stream pdfClientLicense = null) {

			try {
				SetDebug();

				this.LogAction = logAction ?? DefaultLogger;
				//ToDo: need to inject these values
				this.cvClient = new ComputerVisionClient(config);
				this.pdfClient = new PdfClient(pdfClientLicense);
				this.acrobatClient = acrobatClient;
				this.stats = stats;
			}
			catch (Exception ex) {
				Log(ex);
			}
		}

		public static string LibraryInfo() {
			var version = PdfClient.LibraryInfo();
			return version;
		}


		/// <summary>
		/// batch process all documents files found in a folder
		/// </summary>
		/// <param name="sessionConfig">
		/// </param>
		public void ProcessFolder(SessionConfig sessionConfig) {
			var sourceFolder = sessionConfig?.InputDirectory;
			var destinationFolder = sessionConfig?.OutputDirectory;

			try {
				//progress feedback
				Log($"Begin Folder Processing: {sessionConfig.ToString()}", "ProcessFolder");


				if (sessionConfig == null) {
					throw new ArgumentNullException(nameof(sessionConfig));
				}


				ValidateFolderPaths(sessionConfig.InputDirectory, sessionConfig.OutputDirectory);

				//get list of all matching files (recursive)
				//(enumerable so more performant if directory structure contains a lot of files)
				var files = FileHelper.GetFiles(sourceFolder, true, ScannedDocumentRemediation.SupportedFileTypesExtensions);

				foreach (var sourceFile in files) {
					sessionConfig.CancelToken.ThrowIfCancellationRequested();

					//get relative path from input folder root
					//so processed file can be placed at the same relative path in the output folder
					var relativePath = FileHelper.MakeRelativePath(sourceFolder, sourceFile);
					var destinationFile = Path.GetFullPath(Path.Combine(destinationFolder, relativePath));

					ProcessFile(sourceFile, destinationFile, sessionConfig);
				}
			}
			catch (Exception ex) {
				var source = $"{sourceFolder}\r\n-> {destinationFolder}";

				if (!HandleBatchProcessingException(ex, source)) {
					throw;
				}
			}
		}

		/// <summary>
		/// batch process a list of files
		/// </summary>
		/// <param name="sourceFiles">list of documents to be processed</param>
		/// <param name="destinationFolder">path where all files will be saved, regardless of source folder structure</param>
		public void ProcessFiles(SessionConfig sessionConfig) {
			try {
				//ToDo: log total number of files, trim list of existing files, log number of skipped files
				Log($"Begin Files Processing: {sessionConfig.ToString()}", "ProcessFiles");

				foreach (var file in sessionConfig.InputFiles) {
					sessionConfig.CancelToken.ThrowIfCancellationRequested();

					var destinationFile = Path.Combine(sessionConfig.OutputDirectory, Path.GetFileName(file));
					ProcessFile(file, destinationFile, sessionConfig);
				}
			}
			catch (Exception ex) {
				var source = $"-> {sessionConfig.OutputDirectory}";

				if (!HandleBatchProcessingException(ex, source)) {
					throw;
				}
			}
		}

		/// <summary>
		/// process an individual document
		/// </summary>
		/// <param name="sourceFile">path to the document to be processed</param>
		/// <param name="destinationFile">full path and filename where the processed file will be saved; existing files will be overwritten</param>
		/// <param name="sessionConfig">sessionConfig object populated accordingly</param>
		public void ProcessFile(string sourceFile, string destinationFile, SessionConfig sessionConfig) {
			try {
				Log($"Processing:  {sourceFile}\r\n-> {destinationFile}", sourceFile);

				if (!ValidateFilePaths(sourceFile, destinationFile, sessionConfig.OverwriteDestination)) {
					return;
				}

				var numPages = 0;
				if (acrobatClient.IsReOCRAvailable) {
					numPages = DoReOcrWorkflow(sourceFile, destinationFile, sessionConfig);
				}
				else {
					numPages = DoBasicOcrWorkflow(sourceFile, destinationFile, sessionConfig);
				}

				//preserve creation date from original file
				FileHelper.CopyCreationDate(sourceFile, destinationFile);
				stats.FileProcessedCount++;
				stats.PagesProcessedCount += numPages; 
			}
			catch (Exception ex) {
				var source = $"{sourceFile}\r\n-> {destinationFile}";

				if (!HandleBatchProcessingException(ex, source)) {
					throw;
				}
			}
			finally {
				pdfClient.DeleteTempSplitFolder(destinationFile);
			}
		}

		// returns num pages in document
		private int DoBasicOcrWorkflow(string sourceFile, string destinationFile, SessionConfig sessionConfig) {
			var result = GetOCR(sourceFile, destinationFile, sessionConfig.CancelToken, sessionConfig.UseOCRCache);
			SaveArtifacts(result, sourceFile, destinationFile);
			Log($"OCR fetched from Azure: {sourceFile}", sourceFile);

			MergeOcr(sourceFile, destinationFile, result, sessionConfig.CancelToken, true, sessionConfig.ShowTextOnly);
			Log($"Created Merged PDF: {destinationFile}", sourceFile);

			if (sessionConfig.DoAutoTag) {
				AutotagDocument(destinationFile, sessionConfig.CancelToken);
				Log($"Acrobat AutoTagged PDF: {destinationFile}", destinationFile);
			}
			return result.RecognitionResults.Count;
		}
		// returns the number of pages in document
		private int DoReOcrWorkflow(string sourceFile, string destinationFile, SessionConfig sessionConfig) {
			string tempFile =  $"{destinationFile}{TempFileSuffix}";
			var numPages = 0;
			string tempSourceFile = sourceFile;
			try {
				if (sessionConfig.DoConvertCMYK)
				{
					ConvertCMYKDocument(sourceFile, tempFile, sessionConfig.CancelToken);
                    Log($"Converted to image: {sourceFile}", tempFile);
                    RemoveAllText(tempFile, tempFile, sessionConfig.CancelToken);
					tempSourceFile = tempFile;
                }
                else
				{
					//removeAlltext
					RemoveAllText(sourceFile, tempFile, sessionConfig.CancelToken);
				}

				//do ocr
				var result = GetOCR(tempFile, tempFile, sessionConfig.CancelToken, sessionConfig.UseOCRCache);
				numPages = result.RecognitionResults.Count;
				SaveArtifacts(result, sourceFile, destinationFile);
				Log($"Image Only OCR fetched from Azure: {sourceFile}", sourceFile);

				// Opacity must be 0... (???This wont work as expecgted if Debug opacity set)
				RemoveInvisibleText(tempSourceFile, tempFile, sessionConfig.CancelToken);

				//merge OCR text into pdf
				if (EnableOcrMerge) {
					MergeOcr(tempFile, destinationFile, result, sessionConfig.CancelToken, false, sessionConfig.ShowTextOnly);
					Log($"Created Merged PDF: {destinationFile}", sourceFile);
				}

				if (sessionConfig.DoAutoTag) {
					ForceAutotagDocument(destinationFile, sessionConfig.CancelToken);
					Log($"Acrobat AutoTagged PDF: {destinationFile}", destinationFile);
				}

				if (!sessionConfig.DontLayerPDF) {
					AddLayer(destinationFile, sessionConfig.CancelToken);
					Log($"Acrobat added layers PDF: {destinationFile}", destinationFile);
				}

				if (sessionConfig.ShowTextOnly) {
					ShowTextOnly(destinationFile, sessionConfig.CancelToken);
					Log($"ShowTextOnly PDF: {destinationFile}", destinationFile);
				}
			}
			finally {
				FileHelper.DeleteFile(tempFile);
			}
			return numPages;
		}


		/// <summary>
		/// process an individual document using offline cached OCR request result
		/// </summary>
		/// <param name="sourceFile">path to the document to be processed</param>
		/// <param name="destinationFile">full path and filename where the processed file will be saved; existing files will be overwritten</param>
		/// <param name="ocrResultCacheFile">file path to cached OCR results</param>
		/// <param name="overwriteDestination">allow overwriting the output file if it already exists (cannot be used to overwrite the sourceFile)</param>
		public void ProcessFile(string sourceFile, string destinationFile, string ocrResultCacheFile,
				CancellationToken cancelToken, bool overwriteDestination = false, bool autoTag = true, bool textOnlyMode = false) {
			try {
				Log($"Processing:  {sourceFile}\r\n-> {destinationFile}", sourceFile);

				if (!ValidateFilePaths(sourceFile, destinationFile, overwriteDestination, ocrResultCacheFile)) {
					return;
				}


				//load OCR metadata from local file
				var result = ComputerVisionClient.LoadOCR(ocrResultCacheFile);
				Log($"OCR loaded from cache: {ocrResultCacheFile}", sourceFile);

				MergeOcr(sourceFile, destinationFile, result, cancelToken, true, textOnlyMode);
				Log($"Merged PDF saved to: {destinationFile}", sourceFile);

				//do autotag
				if (autoTag) {
					AutotagDocument(destinationFile, cancelToken);
					Log($"AutoTagged PDF: {destinationFile}", destinationFile);
				}

			}
			catch (Exception ex) {
				//from Azure or Cache?
				var ocrSource = ocrResultCacheFile == null ? "Azure" : $"Cache: {ocrResultCacheFile}";
				//process parameters
				var source = $"{sourceFile}\r\n-> {destinationFile}\r\nFrom {ocrSource}";

				if (!HandleBatchProcessingException(ex, source)) {
					throw;
				}
			}
		}

		private ReadOperationResult GetOCRCacheFile(string destinationFile) {
			// destinationFile is either .pdf or .pdf{TempFileSuffix}
			// but want to find .ocr.json
			var cacheFile = destinationFile.ToLower().Replace($".pdf{TempFileSuffix.ToLower()}", ".ocr.json");
			cacheFile = cacheFile.ToLower().Replace($".pdf".ToLower(), ".ocr.json");
			Console.WriteLine($"GetOCRCacheFile = {destinationFile}   cacheFile={cacheFile}");
			var readOperationResult = ComputerVisionClient.LoadOCR(cacheFile);
			return readOperationResult;
		}

		private ReadOperationResult GetOCR(string sourceFile, string destinationFile, CancellationToken cancelToken, bool useOCRCache) {
			if (useOCRCache) {
				try {
					var readOperationResult = GetOCRCacheFile(destinationFile);
					if (readOperationResult != null) {
						return readOperationResult;
					}
				}
				catch (Exception) { }
			}
			//split
			IList<string> documentList = new List<string>();

			using (var reader = new PdfReader(sourceFile)) {
				using (var document = new PdfDocument(reader)) {
					var documentFileInfo = new FileInfo(sourceFile);

					// Test upfront to avoid writing to disc if we don't need to.
					if (document.GetNumberOfPages() > MaxAzurePages || documentFileInfo.Length > MaxAzureFileSize) {
						documentList = pdfClient.SplitPdf(sourceFile, destinationFile);
					}
					else {
						documentList.Add(sourceFile);
					}
				}
			}

			//iterate and ocr each step
			IList<ReadOperationResult> results = new List<ReadOperationResult>();

			foreach (var file in documentList) {
				//avoid direct file access in lower libraries
				using (var fs = File.OpenRead(file)) {
					//submit document to Azure and wait for the response
					results.Add(cvClient.DocumentOCR(fs, cancelToken));
				}
			}

			//Merge OCR results back together
			var result = cvClient.MergOCRResults(results);

			return result;
		}

		private void SaveArtifacts(ReadOperationResult result, string sourceFile, string destinationFile) {
			//save raw ocr results to disk
			if (EnableSaveOcrMetadata) {
				var ocrMetadataPath = Path.ChangeExtension(destinationFile, ".ocr.json");
				SaveOcrMetadata(ocrMetadataPath, result);
				Log($"Saved OCR metadata: {ocrMetadataPath}", sourceFile);
			}

			//save as plain-text
			if (EnableSaveOcrText) {
				var ocrTextPath = Path.ChangeExtension(destinationFile, ".ocr.txt");
				SaveOcrText(ocrTextPath, result);
				Log($"Saved OCR as plain-text: {ocrTextPath}", sourceFile);
			}
		}

		/// <summary>
		/// validate rules for source and destination files
		/// </summary>
		/// <param name="sourceFile">document to be processed</param>
		/// <param name="destinationFile">path the output will be written to</param>
		/// <param name="overwriteDestination">allow or deny overwriting an existing output file</param>
		/// <exception cref="ArgumentException">source and destination are the same</exception>
		/// <remarks>Logs a warning and gracefully terminates if destination file already exists and is not marked for overwrite</remarks>
		/// <returns>true if all checks passed, else false</returns>
		private bool ValidateFolderPaths(string sourceFolder, string destinationFolder) {
			if (String.IsNullOrWhiteSpace(sourceFolder)) {
				throw new ArgumentException("Input folder cannot be blank");
			}

			if (String.IsNullOrWhiteSpace(destinationFolder)) {
				throw new ArgumentException("Output folder cannot be blank");
			}

			if (!Directory.Exists(sourceFolder)) {
				throw new ArgumentException("Input folder cannot be found");
			}

			//prevent unintended processing of output folder
			if (FileHelper.IsBaseOf(sourceFolder, destinationFolder)) {
				throw new ArgumentException("Output folder cannot be the same as or a subfolder of the Input folder");
			}

			//ensure the output directory exists
			FileHelper.EnsureFileDirectoryExists(destinationFolder);

			return true;
		}

		/// <summary>
		/// validate rules for source and destination files
		/// </summary>
		/// <param name="sourceFile">document to be processed</param>
		/// <param name="destinationFile">path the output will be written to</param>
		/// <param name="overwriteDestination">allow or deny overwriting an existing output file</param>
		/// <exception cref="ArgumentException">source and destination are the same</exception>
		/// <remarks>Logs a warning and gracefully terminates if destination file already exists and is not marked for overwrite</remarks>
		/// <returns>true if all checks passed, else false</returns>
		private bool ValidateFilePaths(string sourceFile, string destinationFile, bool overwriteDestination, string ocrCacheFile = null) {
			if (FileHelper.AreEqual(sourceFile, destinationFile)) {
				//error
				throw new ArgumentException("Cannot overwrite Source file.  Source and Destination paths must be different.");
			}


			//cached OCR results
			if (ocrCacheFile != null) {
				if (!File.Exists(ocrCacheFile)) {
					//error
					throw new ArgumentException("Invalid ocr cache file path");
				}
			}


			//PDF docs
			if (!File.Exists(sourceFile)) {
				//error
				throw new ArgumentException("Invalid source file path");
			}
			else if (File.Exists(destinationFile)) {
				if (overwriteDestination) {
					File.Delete(destinationFile);
				}
				else {
					//non-critical warning
					Log($"Output file already exists:  {destinationFile}", sourceFile, LogLevel.Warning);
					return false;
				}
			}

			//ensure the output directory exists
			FileHelper.EnsureFileDirectoryExists(destinationFile);

			return true;
		}

		/// <summary>
		/// log and continue for non-fatal exceptions
		/// </summary>
		/// <returns>
		/// flag indicating if exception was handled or not
		/// <para>true: non-fatal - continue batch processing</para>
		/// <para>false: fatal - rethrow exception from calling catch to preserve call stack and to halt batch processing</para>
		/// </returns>
		private bool HandleBatchProcessingException(Exception exception, string source) {
			if (exception == null) { return false; }


			var handled = false;

			//aggregate exception thrown from Tasks
			if (exception is AggregateException) {
				var ae = exception as AggregateException;

				ae.Handle(ex => {
					//pass flag back for external handling so this and non-ae exceptions behave the same
					handled = HandleBatchProcessingException(ex, source);
					//we're not in a catch block, so always return true to prevent automatic rethrow from ae.Handle()
					//rethrowing outside of a catch block will reset the call stack and cancellations will be mishandled as exceptions
					return true;
				});
			}
			//non-aggregated exceptions
			else {
				if (IsFatalException(exception)) {
					//halt batch; bubble up
					handled = false;
				}
				//recurse nested exceptions
				else if (HandleBatchProcessingException(exception.InnerException, source)) {
					//halt batch; bubble up
					handled = false;
				}
				else {
					//continue batch
					handled = true;
					stats.FileErrorCount++;
					Log(exception, source);
				}
			}

			return handled;
		}

		/// <summary>
		/// determine if error is transient or likely to occur for every file in batch
		/// </summary>
		/// <returns>true if batch should be halted</returns>
		private bool IsFatalException(Exception exception) {
			bool isFatal;

			if (exception is OperationCanceledException) {
				//job canceled warning - bubble up
				//rethrow from calling catch to preserve Cancellation
				isFatal = true;
			}
			else if (FatalExceptions.Any(s => exception.Message.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0)) {
				isFatal = true;
			}
			else {
				//anything else - continue batch
				isFatal = false;
			}

			return isFatal;
		}

		/// <summary>
		/// cache OCR results to disk; can be loaded later for processing
		/// <see cref="GetDocumentOcrFromCache(string)"/>
		/// </summary>
		/// <param name="path">location to save the results</param>
		/// <param name="result">the OCR result object to be serialized</param>
		private void SaveOcrMetadata(string path, ReadOperationResult result) {
			FileHelper.ToJsonFile(path, result);
		}

		private void SaveOcrText(string path, ReadOperationResult result) {
			var txt = ComputerVisionClient.GetOcrPlainText(result, true);
			FileHelper.ToTextFile(path, txt);
		}

		private void MergeOcr(string sourceFile, string destinationFile, ReadOperationResult ocrResult, CancellationToken cancelToken, bool skipMixedPages, bool textOnlyMode) {
			int mixedPageCount = 0;

			pdfClient.MergePdfOcr(sourceFile, ocrResult.RecognitionResults, destinationFile, cancelToken, out mixedPageCount, skipMixedPages, textOnlyMode);

			// log warning if mixedPageCount > 0
			if (mixedPageCount > 0) {
				Log($"Text container already exist on {mixedPageCount} pages for {destinationFile}", "MergeOcr", LogLevel.Warning);
			}
        }
        private void ConvertCMYKDocument(string documentSource, string documentTarget, CancellationToken cancelToken)
        {
            this.acrobatClient.ConvertCMYK(documentSource, documentTarget, cancelToken);
        }


        private void AutotagDocument(string documentPath, CancellationToken cancelToken)
        {
            this.acrobatClient.AutotagDocument(documentPath, cancelToken);
        }

        private void ForceAutotagDocument(string documentPath, CancellationToken cancelToken) {
			acrobatClient.ForceAutotagDocument(documentPath, cancelToken);
		}

		private void AddLayer(string documentPath, CancellationToken cancelToken) {
			acrobatClient.AddLayers(documentPath, cancelToken);
		}

		private void RemoveAllText(string documentSource, string documentTarget, CancellationToken cancelToken) {
			acrobatClient.RemoveAllText(documentSource, documentTarget, cancelToken);
		}

		private void RemoveInvisibleText(string documentSource, string documentTarget, CancellationToken cancelToken) {
			acrobatClient.RemoveInvisibleText(documentSource, documentTarget, cancelToken);
		}

		private void ShowTextOnly(string documentSource, CancellationToken cancelToken) {
			acrobatClient.ShowTextOnly(documentSource, cancelToken);
		}


		private void Log(string message, string source, LogLevel logLevel = LogLevel.Informational) {
			try {
				if (LogAction != null) {
					source = source ?? this.ToString();
					var logEntry = new LogEntry(logLevel, source, message);

					LogAction.Invoke(logEntry);
				}
			}
			catch (Exception ex) {
				//avoid unhandled exception
				//but nowhere to go with it
				//will be lost silently
			}
		}

		private void Log(Exception exception, string source = null) {
			try {
				if (LogAction != null) {
					source = source ?? this.ToString();
					
					var logEntry = new LogEntry(exception: exception, source: source);

					LogAction.Invoke(logEntry);

					//search exception message contains could not find file and ~ocrbot-success~, indicating wrong Adobe Acrobat settings
					if (exception.Message.Contains("Could not find file") && exception.Message.Contains("~ocrbot-success~"))
					{						
						Log("Make sure to set in Adobe Acrobat DC under Edit -> Preferences -> Security: Enhanced -> Enable Protected Mode at Startup (Preview) is disabled.", source, LogLevel.Error);
					}

					

				}
			}
			catch(Exception ex) {
				//avoid unhandled exception
				//but nowhere to go with it
				//will be lost silently
			}
		}

		private void DefaultLogger(LogEntry message) {
			try {
				var formattedDate = DateTime.Now.ToString("s"); //sortable, e.g. 2009-06-15T13:45:30
				var formattedMessage = $"{formattedDate}: {message}";
				var logPath = Path.GetFullPath(@".\PdfToDocs.log");

				//Console.WriteLine(formattedMessage);
				File.AppendAllText(logPath, formattedMessage + Environment.NewLine);
			}
			catch(Exception ex) {
				//avoid unhandled exception
				//but nowhere to go with it
				//will be lost silently
			}
		}
	}
}
