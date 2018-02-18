#if FALSE
using NUnit.Framework;

namespace Skaillz.SanityChecker.Editor
{
    public class SanityCheckerTests
    {
        [Test]
        public void SanityCheck_BuildScenes()
        {
            SanityChecker.RunChecksInBuildScenes();
        }

        [Test]
        public void SanityCheck_Prefabs()
        {
            SanityChecker.RunChecksInPrefabs();
        }
    }
}
#endif