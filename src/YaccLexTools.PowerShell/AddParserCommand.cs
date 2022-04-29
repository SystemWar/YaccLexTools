﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using YaccLexTools.PowerShell.Extensions;
using YaccLexTools.PowerShell.Utilities;


namespace YaccLexTools.PowerShell
{

	internal class AddParserCommand : YaccLexToolsCommand
	{

		private string _projectDir;
		private string _projectRootNamespace;


		public AddParserCommand(string parserName, string @namespace, string projectDir, string projectRootNamespace)
		{
			// Using check because this is effecitively public surface since
			// it is called by a PowerShell command.
			Check.NotEmpty(parserName, "parserName");

			_projectDir = projectDir;
			_projectRootNamespace = projectRootNamespace;
			
			Execute(() => Execute(parserName, @namespace));
		}


		public void Execute(string parserName, string @namespace)
		{
			DebugCheck.NotEmpty(parserName);

			WriteLine("\nAdding parser \"" + parserName + "\"...\n");

			//string rootNamespace = Project.GetRootNamespace();
            if (String.IsNullOrEmpty(@namespace))
                @namespace = _projectRootNamespace;

			Dictionary<string, string> tokens = new Dictionary<string, string>();
			tokens.Add("parserName", parserName);
			tokens.Add("namespace", @namespace);

			string path = @namespace.Replace(".", "\\");

			if (_projectRootNamespace != null && @namespace.StartsWith(_projectRootNamespace))
			{
				path = path.Substring(_projectRootNamespace.Length);
				if (path.StartsWith("\\")) path = path.Substring(1);
			}

			if (path.Length > 0)
			{
				parserName = path + "\\" + parserName;
			}

			parserName += "\\" + parserName;

			AddFile(parserName + ".Language.analyzer.lex", "___.Language.analyzer.lex", tokens);
			AddFile(parserName + ".Language.grammar.y", "___.Language.grammar.y", tokens);
			AddFile(parserName + ".Parser.cs", "___.Parser.cs", tokens);
			//AddFile(parserName + ".Parser.Generated.cs", "___.Parser.Generated.cs", tokens);
			AddFile(parserName + ".Scanner.cs", "___.Scanner.cs", tokens);
			//AddFile(parserName + ".Scanner.Generated.cs", "___.Scanner.Generated.cs", tokens);
		}


		private void AddFile(string path, string templateName, Dictionary<string, string> tokens)
		{
			string template = LoadTemplate(templateName);
			AddProjectFile(_projectDir, path, new TemplateProcessor().Process(template, tokens));
		}


		private string LoadTemplate(string name)
		{
			DebugCheck.NotEmpty(name);

			var stream = GetType().Assembly.GetManifestResourceStream("YaccLexTools.PowerShell.Templates." + name);
			Debug.Assert(stream != null);

			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				return reader.ReadToEnd();
			}
		}
	}
}
