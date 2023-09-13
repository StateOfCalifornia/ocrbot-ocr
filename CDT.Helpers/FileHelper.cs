using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CDT.Helpers {
	public class FileHelper {
		public static string DefaultSearchPattern => "*.*";

		/// <summary>
		///Move file with overwrite
		/// <summary>
		public static void MoveFileForce(string source, string target) {
			DeleteFile(target);
			File.Move(source, target);
		}


		/// <summary>
		///Returns true if file exists
		/// <summary>
		public static bool Exists(string file) {
			bool isFound = File.Exists(file);
			return isFound;
		}



		/// <summary>
		///Delete file with checking for null or whitespae
		/// <summary>
		public static void DeleteFile(string file) {
			if (string.IsNullOrWhiteSpace(file)) {
				return;
			}
			File.Delete(file);
		}

		/// <summary>
		/// Determine if two  paths are in the same folder
		/// </summary>
		public static bool AreSameFolder(string path1, string path2) {
			if (String.IsNullOrWhiteSpace(path1) || String.IsNullOrWhiteSpace(path2)) {
				throw new ArgumentException("Path cannot be empty.");
			}

			var normalized1 = Path.GetDirectoryName(NormalizePath(path1));
			var normalized2 = Path.GetDirectoryName(NormalizePath(path2));

			var areEqual = normalized1.Equals(normalized2, StringComparison.InvariantCultureIgnoreCase);

			return areEqual;
		}

		/// <summary>
		/// peform case insensitive comparison of fully resolved path strings
		/// </summary>
		public static bool AreEqual(string path1, string path2) {
			if (String.IsNullOrWhiteSpace(path1) || String.IsNullOrWhiteSpace(path2)) {
				throw new ArgumentException("Path cannot be empty.");
			}

			var normalized1 = NormalizePath(path1).TrimEnd(Path.DirectorySeparatorChar);
			var normalized2 = NormalizePath(path2).TrimEnd(Path.DirectorySeparatorChar);

			var areEqual = normalized1.Equals(normalized2, StringComparison.InvariantCultureIgnoreCase);

			return areEqual;
		}

		/// <summary>
		/// resolve path and normalize format
		/// </summary>
		/// <param name="path">folder or file path</param>
		public static string NormalizePath(string path) {
			var uri = new Uri(path);
			var local = uri.LocalPath;
			var fullPath = Path.GetFullPath(local);

			var normalized = fullPath
				.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

			return normalized;
		}


		/// <summary>
		/// Creates a relative path from one file or folder to another.
		/// </summary>
		/// <param name="fromPath">fully qualified directory path to use as the anchor of the relative path</param>
		/// <param name="toPath">fully qualified  or partial file or directory path to be made relative to <paramref name="fromPath"/></param>
		/// <returns>
		/// The relative path from <paramref name="fromPath"/> to <paramref name="toPath"/>
		/// <para></para>
		/// </returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="UriFormatException"></exception>
		/// <exception cref="InvalidOperationException">if </exception>
		public static String MakeRelativePath(String fromPath, String toPath) {
			if (String.IsNullOrEmpty(fromPath)) { throw new ArgumentNullException("fromPath"); }
			if (String.IsNullOrEmpty(toPath)) { throw new ArgumentNullException("toPath"); }

			//must end with slash to be treated as a directory by Uri
			fromPath = TerminateDirectoryPath(fromPath);

			Uri fromUri = new Uri(fromPath);
			Uri toUri = new Uri(toPath);

			if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

			Uri relativeUri = fromUri.MakeRelativeUri(toUri);
			String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

			if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase)) {
				relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			}

			return relativePath;
		}

		/// <summary>
		/// get a list of all files in <paramref name="folderPath"/>
		/// </summary>
		/// <param name="folderPath">the root path to search</param>
		/// <param name="includeSubfolders">true to recurse all subfolders</param>
		/// <param name="fileTypes">optional whitelist of file extensions to filter on, e.g. { ".bmp", ".gif", ".jpg", ".png" }</param>
		/// <returns>enumerable list of fully qualified file paths</returns>
		public static IEnumerable<string> GetFiles(string folderPath, bool includeSubfolders = false, string[] fileTypes = null) {
			if (String.IsNullOrWhiteSpace(folderPath)) { throw new ArgumentException("String cannot be null or empty", "folderPath"); }

			var searchOption = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			var files = Directory.EnumerateFiles(folderPath, DefaultSearchPattern, searchOption);

			if (fileTypes?.Length > 0) {
				//apply file type filter
				//(enumerable so more performant if directory structure contains a lot of files)
				var filtered = files
					.Where(f => fileTypes.Contains(Path.GetExtension(f).ToLower()));

				return filtered;
			}
			else {
				//unfiltered
				return files;
			}
		}

		/// <summary>
		/// creates each part of a directory path if they do not already exist
		/// </summary>
		/// <param name="directoryPath">
		/// path without filename for the directory to create
		/// <para>unterminated file paths (without trailing slash) are assumed to be a directory (not a file name)</para>
		/// </param>
		/// <seealso cref="EnsureFileDirectoryExists"/>
		public static void EnsureDirectoryExists(string path) {
			Directory.CreateDirectory(path);
		}

		/// <summary>
		/// creates each part of a directory path if they do not already exist
		/// <para>
		/// path accomodates directory or filenames,
		/// any unterminated paths (without trailing slash) are assumed to be the file name, not a directory
		/// </para>
		/// </summary>
		/// <param name="fileOrFolderPath">
		/// path of the directory to be created
		/// <para>NOTE: unterminated paths (without trailing slash) will be treated as file names</para>
		/// </param>
		/// <seealso cref="EnsureDirectoryExists"/>
		public static void EnsureFileDirectoryExists(string fileOrFolderPath) {
			var directory = Path.GetDirectoryName(fileOrFolderPath);

			EnsureDirectoryExists(directory);
		}

		/// <summary>
		/// determines if <paramref name="potentialBase"/> is the parent folder of <paramref name="potentialChild"/>
		/// </summary>
		/// <param name="potentialBase"></param>
		/// <param name="potentialChild"></param>
		/// <returns>boolean indicating if <paramref name="potentialBase"/> is the parent folder of <paramref name="potentialChild"/></returns>
		/// <example>
		/// string potentialBase = @"c:\dir1\";
		/// string regular = @"c:\dir1\dir2";
		/// string confusing = @"c:\temp\..\dir1\dir2";
		/// string malicious = @"c:\dir1\..\windows\system32\";
		/// 
		/// Console.WriteLine(FileHelper.IsBaseOf(potentialBase, regular));   // True
		/// Console.WriteLine(FileHelper.IsBaseOf(potentialBase, confusing)); // True
		/// Console.WriteLine(FileHelper.IsBaseOf(potentialBase, malicious)); // False
		/// </example>
		public static bool IsBaseOf(string potentialBase, string potentialChild) {
			potentialBase = TerminateDirectoryPath(potentialBase);

			var baseUri = new Uri(potentialBase);
			var childUri = new Uri(potentialChild);

			var isBase = baseUri.IsBaseOf(childUri);

			return isBase;
		}

		public static string TerminateDirectoryPath(string path) {
			if (String.IsNullOrWhiteSpace(path)
				|| path.EndsWith(Path.DirectorySeparatorChar.ToString())) {
				return path;
			}
			else {
				var terminated = $"{path}{Path.DirectorySeparatorChar}";
				return terminated;
			}
		}

		public static void ToTextFile(string file, string text) {
			File.WriteAllText(file, text);
		}


		public static void ToJsonFile<T>(string file, T item) {
			var json = StreamHelper.SerializeToJson(item);
			ToTextFile(file, json);
		}

		public static T FromJsonFile<T>(string file) {
			var json = File.ReadAllText(file);
			var item  = StreamHelper.DeserializeFromJson<T>(json);
			return item;
		}

		public static void CopyCreationDate(string sourceFile, string destinationFile) {
			var creationDate = File.GetCreationTime(sourceFile);
			File.SetCreationTime(destinationFile, creationDate);
		}
	}
}
