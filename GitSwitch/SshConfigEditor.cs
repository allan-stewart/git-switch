﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GitSwitch
{
    public interface ISshConfigEditor
    {
        void SetGitHubKeyFile(string sshKeyPath);
    }

    public class SshConfigEditor : ISshConfigEditor
    {
        readonly IFileHandler fileHandler;

        public SshConfigEditor(IFileHandler fileHandler)
        {
            this.fileHandler = fileHandler;
        }

        internal string SshConfigFilePath
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + AppConstants.SshConfigFile;
            }
        }

        public void SetGitHubKeyFile(string sshKeyPath)
        {
            string unixSshKeyPath = WindowsToUnixPath(sshKeyPath);
            string defaultSshConfig = "Host *\n\tIdentityFile " + unixSshKeyPath + "\n";

            try
            {
                IEnumerable<string> configFileLines = fileHandler.ReadLines(SshConfigFilePath);
                var newSsshConfig = ProcessSshFile(configFileLines, unixSshKeyPath, defaultSshConfig);
                fileHandler.WriteFile(SshConfigFilePath, newSsshConfig);
            }
            catch (DirectoryNotFoundException)
            {
                fileHandler.WriteFile(SshConfigFilePath, defaultSshConfig);
            }
            catch (FileNotFoundException)
            {
                fileHandler.WriteFile(SshConfigFilePath, defaultSshConfig);
            }
        }

        static string ProcessSshFile(IEnumerable<string> configFileLines, string unixSshKeyPath, string defaultSshConfig)
        {
            List<string> output = new List<string>();
            bool didFindGitHub = false;
            bool inGitHubSection = false;

            foreach (var line in configFileLines)
            {
                output.Add(line);
                if (Regex.IsMatch(line, @"^\s*Host\s+"))
                {
                    inGitHubSection = line.Contains("*");
                    if (inGitHubSection)
                    {
                        didFindGitHub = true;
                        output.Add("\tIdentityFile " + unixSshKeyPath);
                    }
                }

                if (inGitHubSection && Regex.IsMatch(line, @"^\s*IdentityFile\s+"))
                    output.RemoveAt(output.Count - 1);
            }

            if (output.Count > 0 && output.Last() != "")
                output.Add("");

            if (!didFindGitHub)
                output.Add(defaultSshConfig);

            return string.Join("\n", output);
        }

        string WindowsToUnixPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "\"\"";

            return "\"" + Regex.Replace(path.Replace("\\", "/"), "^([A-Za-z]):", "/$1") + "\"";
        }
    }
}
