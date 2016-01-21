namespace Telemetry.Core.UnitTests.Steps
{
	using System.Linq;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using TechTalk.SpecFlow;

	[Binding]
    public class ConfigSteps
    {
        [Given(@"key ""(.*)""")]
        public void GivenKey(string key)
        {
            ScenarioContext.Current.Set(key, "key");
        }
        
        [When(@"I get value")]
        public void WhenIGetValue()
        {
	        var key = ScenarioContext.Current.Get<string>("key");
	        var value = Config.Get(key);
			ScenarioContext.Current.Set(value, "value");
        }
        
        [Then(@"the result should be ""(.*)""")]
        public void ThenTheResultShouldBe(string expected)
        {
	        expected = expected.Equals("<null>") ? null : expected;
	        if (expected == null)
	        {
		        if (ScenarioContext.Current.Keys.Any(k => k.Equals("value")))
		        {
			        var actual = ScenarioContext.Current.First(kv => kv.Key == "value").Value;
					Assert.IsNull(actual);
		        }
	        }
	        else
	        {
				var value = ScenarioContext.Current.Get<string>("value");
				Assert.AreEqual(expected, value);
			}
        }
    }
}
