using NEA.math;
using NUnit.Framework;
using ui.math;
using math_parser;

namespace NEA.Tests.math
{
    [TestFixture]
    [TestOf(typeof(SimplexInterationRunner))]
    public class SimplexTest
    {

        [Test]
        public void LpModelToTableauTest()
        {
            using (Assert.EnterMultipleScope())
            {
                // 2022 A Level OCR MEI B Further Math Modelling with ALgorithms Q5b/c
                string q1 = @"MAX 2x + 3y + z
ST
3x + y + 4z <= 48
5x + 4y <= 32
END";
                Assert.That(
                    ToSimplexRunner.Translate(q1).Expressions,
                    Is.EquivalentTo(
                        new Fraction[,]
                        {
                            {1, 0, 0},
                            {-2, 3, 5},
                            {-3, 1, 4},
                            {-1, 4, 0},
                            {0, 1, 0},
                            {0, 0, 1},
                            {0, 48, 32}
                        }
                    )
                );
            }
        }

        [Test]
        public void StructuredToTableauTest()
        {
            // using (Assert.EnterMultipleScope())
            // {
            //     Assert.That();
            // }
        }
    }
}