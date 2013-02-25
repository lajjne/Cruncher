﻿#region Licence
// -----------------------------------------------------------------------
// <copyright file="ResourcePreprocessor.cs" company="James South">
//     Copyright (c) James South.
//     Dual licensed under the MIT or GPL Version 2 licenses.
// </copyright>
// -----------------------------------------------------------------------
#endregion

namespace Cruncher.PreProcessors
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Hosting;
    #endregion

    /// <summary>
    /// Provides methods to replace relative resource paths within a stylesheet with absolute paths.
    /// </summary>
    public class ResourcePreprocessor : IPreProcessor
    {
        #region Fields
        /// <summary>
        /// The regular expression for matching resources within a css file.
        /// </summary>
        private static readonly Regex ResourceRegex = new Regex(@"url\(\s*(?:[""']?)(.*?)(?:[""']?)\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //private static readonly Regex ResourceRegex = new Regex(@"url\(\s*(?:[""']?)(.*?)(?:[""']?)\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        #endregion


        #region Methods
        /// <summary>
        /// Transforms the content of the given string by replacing relative paths. 
        /// </summary>
        /// <param name="input">The input string to transform.</param>
        /// <param name="path">The path to the given input string to transform.</param>
        /// <returns>The transformed string.</returns>
        public string Transform(string input, string path)
        {
            //try
            //{
                return RewritePaths(input, path);
            //}
            //catch
            //{

            //    return input;
            //}
        }

        /// <summary>
        /// Rewrites the relative path as relative to the application root.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="path"></param>
        /// <returns>The css with the relative paths replaced.</returns>
        private string RewritePaths(string input, string path)
        {
            string sourceDirectory;
            bool isExternal = path.StartsWith("http");

            if (!isExternal)
            {
                sourceDirectory = string.Format("{0}/", Path.GetDirectoryName(path));
            }
            else
            {
                int directoryIndex = path.LastIndexOf('/');
                sourceDirectory = path.Substring(0, directoryIndex + 1);
            }

            string rootDirectory = string.Format("{0}/", Path.GetDirectoryName(HostingEnvironment.ApplicationPhysicalPath));
            Uri rootUri = new Uri(rootDirectory, UriKind.Absolute);
            IEnumerable<string> relativePaths = this.GetRelativePaths(input);

            foreach (string relativePath in relativePaths)
            {
                int hashQueryIndex = relativePath.IndexOfAny(new[] { '?', '#' });

                string hashQuery = hashQueryIndex >= 0
                    ? relativePath.Substring(hashQueryIndex)
                    : string.Empty;

                string capturedRelativePath = hashQuery != string.Empty
                    ? relativePath.Substring(0, hashQueryIndex)
                    : relativePath;

                Uri resolvedSourcePath = !isExternal
                    ? new Uri(Path.Combine(sourceDirectory, capturedRelativePath))
                    : new Uri(new Uri(sourceDirectory), new Uri(capturedRelativePath));

                string resolvedOutput = rootUri.MakeRelativeUri(resolvedSourcePath).OriginalString;

                string newRelativePath = string.Format("{0,1}", resolvedOutput, hashQuery);

                input = this.ReplaceRelativePathsIn(input, relativePath, newRelativePath);
            }

            return input;
        }

        /// <summary>
        /// Replaces the relative paths in the given css content.
        /// </summary>
        /// <param name="css"></param>
        /// <param name="oldPath">The path to replace.</param>
        /// <param name="newPath">The path to replace the old one with.</param>
        /// <returns>The css content wit the paths replaced.</returns>
        private string ReplaceRelativePathsIn(string css, string oldPath, string newPath)
        {
            Regex regex = new Regex(@"url\(\s*[""']{0,1}" + Regex.Escape(oldPath) + @"[""']{0,1}\s*\)", RegexOptions.IgnoreCase);

            return regex.Replace(css, match =>
            {
                return match.Value.Replace(oldPath, newPath);
            });
        }

        /// <summary>
        /// Returns a distinct enumerable collection of relative paths from the css file.
        /// </summary>
        /// <param name="input">The css file to parse.</param>
        /// <returns>A distinct enumerable collection of relative paths from the css file.</returns>
        private IEnumerable<string> GetRelativePaths(string input)
        {
            MatchCollection matches = ResourceRegex.Matches(input);

            // Filter the matches and return distinct ones.
            return matches.Cast<Match>()
             .Select(m => m.Groups[1].Captures[0].Value)
             .Where(p => p != "\"\""
                    && p != "''"
                    && !string.IsNullOrWhiteSpace(p)
                    && !p.StartsWith("http://")
                    && !p.StartsWith("https://")
                    && !p.StartsWith("data:"))
             //.Where(p => !p.StartsWith("/")
             //       && p != "\"\""
             //       && p != "''"
             //       && !string.IsNullOrWhiteSpace(p)
             //       && !p.StartsWith("http://")
             //       && !p.StartsWith("https://")
             //       && !p.StartsWith("data:"))
             .Distinct();
        }
        #endregion
    }
}