﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GitCommander
{
	public class BranchInfo
	{
		public string name, fullname;
	}

	public class Branch
	{
		public string name, fullname, remoteName;
		public bool isActive, isRemote, isTracking, isHead;
		public BranchInfo head, tracking;
	}

	public static partial class Repository
	{
		public static bool DeleteBranch(string branch)
		{
			string error;
			lastResult = Tools.RunExe("git", "branch -d " + branch, null, out error);
			lastError = error;

			return string.IsNullOrEmpty(lastError);
		}

		public static bool PruneRemoteBranches()
		{
			string error;
			lastResult = Tools.RunExe("git", "remote prune origin", null, out error);
			lastError = error;

			return string.IsNullOrEmpty(lastError);
		}

		public static bool GetRemotePrunableBrancheNames(out string[] branchName)
		{
			var branchNameList = new List<string>();
			void stdCallback(string line)
			{
				branchNameList.Add(line);
			}

			string error;
			lastResult = Tools.RunExe("git", "remote prune origin --dry-run", null, out error, stdCallback);
			lastError = error;

			if (!string.IsNullOrEmpty(lastError))
			{
				branchName = null;
				return false;
			}
			
			branchName = branchNameList.ToArray();
			return true;
		}

		public static bool GetRemotes(out string[] remotes)
		{
			var remotesList = new List<string>();
			void stdCallback(string line)
			{
				remotesList.Add(line);
			}

			string error;
			lastResult = Tools.RunExe("git", "remote show", null, out error, stdCallback);
			lastError = error;

			if (!string.IsNullOrEmpty(lastError))
			{
				remotes = null;
				return false;
			}
			
			remotes = remotesList.ToArray();
			return true;
		}

		// TODO: use "git rev-parse --abbrev-ref --symbolic-full-name @{u} awitte" or "git branch -vv" to get tracking info
		public static bool GetAllBranches(out Branch[] branches)
		{
			var branchList = new List<Branch>();
			void stdCallback(string line)
			{
				line = line.TrimStart();

				// check if remote
				bool isActive = false;
				if (line[0] == '*')
				{
					isActive = true;
					line = line.Replace("* ", "");
				}

				// state vars
				string fullname = line;
				string name = line, remoteName = null, headPtrName = null, headPtrFullName = null;
				bool isRemote = false, isHead = false;

				// check if branch is remote head
				var match = Regex.Match(line, @"remotes/(.*)/HEAD -> (.*)");
				if (match.Success)
				{
					isRemote = true;
					isHead = true;
					name = "HEAD";
					fullname = match.Groups[1].Value + '/' + name;
					headPtrFullName = match.Groups[2].Value;

					var values = headPtrFullName.Split('/');
					if (values.Length == 2) headPtrName = values[1];
				}

				// check if branch is remote
				else
				{
					match = Regex.Match(line, @"remotes/(.*)/(.*)");
					if (match.Success)
					{
						isRemote = true;
						remoteName = match.Groups[1].Value;
						name = match.Groups[2].Value;
						fullname = remoteName + '/' + name;
					}
				}

				var branch = new Branch()
				{
					name = name,
					fullname = fullname,
					isActive = isActive,
					isRemote = isRemote,
					remoteName = remoteName,
					isHead = isHead
				};

				if (isHead)
				{
					branch.head = new BranchInfo()
					{
						name = headPtrName,
						fullname = headPtrFullName
					};
				}

				branchList.Add(branch);
			}

			string error;
			lastResult = Tools.RunExe("git", "branch -a", null, out error, stdCallback);
			lastError = error;

			if (!string.IsNullOrEmpty(lastError))
			{
				branches = null;
				return false;
			}

			branches = branchList.ToArray();
			return true;
		}
	}
}