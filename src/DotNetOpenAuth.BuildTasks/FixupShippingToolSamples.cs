﻿//-----------------------------------------------------------------------
// <copyright file="FixupShippingToolSamples.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using Microsoft.Build.BuildEngine;
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	/// <summary>
	/// Removes imports that only apply when a shipping tool sample builds as part of
	/// the entire project, but not when it's part of a source code sample.
	/// </summary>
	public class FixupShippingToolSamples : Task {
		[Required]
		public ITaskItem[] Projects { get; set; }

		public string[] RemoveImportsStartingWith { get; set; }

		/// <summary>
		/// Executes this instance.
		/// </summary>
		/// <returns></returns>
		public override bool Execute() {
			foreach (ITaskItem projectTaskItem in this.Projects) {
				this.Log.LogMessage("Fixing up the {0} sample for shipping as source code.", Path.GetFileNameWithoutExtension(projectTaskItem.ItemSpec));

				var project = new Project();
				Uri projectUri = new Uri(projectTaskItem.GetMetadata("FullPath"));
				project.Load(projectTaskItem.ItemSpec, ProjectLoadSettings.IgnoreMissingImports);

				if (this.RemoveImportsStartingWith != null && this.RemoveImportsStartingWith.Length > 0) {
					project.Imports.Cast<Import>()
						.Where(import => this.RemoveImportsStartingWith.Any(start => import.ProjectPath.StartsWith(start, StringComparison.OrdinalIgnoreCase)))
						.ToList()
						.ForEach(import => project.Imports.RemoveImport(import));
				}

				project.Save(projectTaskItem.ItemSpec);
			}

			return !this.Log.HasLoggedErrors;
		}
	}
}
