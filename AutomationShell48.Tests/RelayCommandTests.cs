using AutomationShell48.Core.MVVM;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutomationShell48.Tests
{
    [TestClass]
    public class RelayCommandTests
    {
        [TestMethod]
        public void RelayCommand_Executes_Action()
        {
            var executed = false;
            var command = new RelayCommand(() => executed = true);

            command.Execute(null);

            Assert.IsTrue(executed);
        }
    }
}
