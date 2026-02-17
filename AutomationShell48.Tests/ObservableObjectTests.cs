using AutomationShell48.Core.MVVM;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutomationShell48.Tests
{
    [TestClass]
    public class ObservableObjectTests
    {
        [TestMethod]
        public void SetProperty_Raises_PropertyChanged()
        {
            var vm = new TestViewModel();
            string changedName = null;
            vm.PropertyChanged += (s, e) => changedName = e.PropertyName;

            vm.Name = "Updated";

            Assert.AreEqual("Name", changedName);
        }

        private class TestViewModel : ObservableObject
        {
            private string _name;

            public string Name
            {
                get => _name;
                set => SetProperty(ref _name, value);
            }
        }
    }
}
