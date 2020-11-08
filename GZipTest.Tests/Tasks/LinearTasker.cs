using GZipTest.Tasks;

namespace GZipTest.Tests.Tasks
{
    class LinearTasker : Tasker
    {
        public override ITasker StartAsync()
        {
            return base.StartSequential();
        }

    }
}
