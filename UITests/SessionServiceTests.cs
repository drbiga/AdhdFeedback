using Core.Services;


namespace CoreTests
{
    [TestClass]
    public sealed class SessionServiceTests
    {
        [TestMethod]
        public void TestCreateNotNull()
        {
            SessionExecutionService service = SessionExecutionService.GetOrCreate();
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void TestSingleton()
        {
            SessionExecutionService service1 = SessionExecutionService.GetOrCreate();
            SessionExecutionService service2 = SessionExecutionService.GetOrCreate();
            Assert.AreSame(service1, service2);
        }
    }
}   
