using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FactonExtensionPackage.Test
{
	using System.IO;
	using FactonExtensionPackage.FormatingCommands.SubCommands;

	[TestClass]
	public class FactonizeModuleCommandTest
	{
		[TestMethod]
		public void ExecuteTest()
		{
			string configFile = File.ReadAllText("configFile.txt");
			string csFile = File.ReadAllText("csFile.txt");
			string actual = FactonizeModuleCommand.DetermineNewConfig(configFile, csFile);

			Assert.AreNotEqual(configFile, actual);
		}
	}
}
