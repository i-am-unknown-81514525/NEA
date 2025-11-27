using NEA.math;
using NUnit.Framework;
using ui.math;
using math_parser;
using System.Collections.Generic;

namespace NEA.Tests.math
{
    [TestFixture]
    [TestOf(typeof(SimplexInterationRunner))]
    public class SimplexTest
    {

        [Test]
        // 2022 A Level OCR MEI B Further Math Modelling with Algorithms Q5b/c
        public void LpModelToTableauTest1()
        {
            string question = @"MAX 2x + 3y + z
            ST
            3x + y + 4z <= 48
            5x + 4y <= 32
            END";
            SimplexInterationRunner runner = ToSimplexRunner.Translate(question);
            using (Assert.EnterMultipleScope())
            {

                Assert.That(
                    runner.Expressions,
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

                Assert.That(
                    runner.Vars,
                    Is.EquivalentTo(new [] {"P", "x", "y", "z", "s_1", "s_2"})
                );
            }
        }

        [Test]
        // 2022 A Level OCR MEI B Further Math Modelling with Algorithms Q5c/d
        public void TableauExecution1()
        {
            using (Assert.EnterMultipleScope())
            {
                string question = @"MAX 2x + 3y + z
                ST
                3x + y + 4z <= 48
                5x + 4y <= 32
                END";
                SimplexInterationRunner runner = ToSimplexRunner.Translate(question);
                while (runner.It < 2)
                {
                    SimplexRunnerOutput output = runner.Next();
                    runner = output.Next;
                }

                Assert.That(
                    runner.Expressions,
                    Is.EquivalentTo(
                        new Fraction[,]
                        {
                            {1, 0, 0},
                            {new Fraction(35, 16), new Fraction(7, 16), new Fraction(5, 4)},
                            {0, 0, 1},
                            {0, 1, 0},
                            {new Fraction(1, 4), new Fraction(1, 4), 0},
                            {new Fraction(11, 16), new Fraction(-1, 16), new Fraction(1, 4)},
                            {34, 10, 8}
                        }
                    )
                );
            }
        }

        [Test]
        // 2022 A Level OCR MEI B Further Math Modelling with Algorithms Q5d
        public void TableauExecution2()
        {
            using (Assert.EnterMultipleScope())
            {
                string question = @"MAX 2x + 3y + z
                ST
                3x + y + 4z <= 48
                5x + 4y <= 32
                END";
                SimplexInterationRunner runner = ToSimplexRunner.Translate(question);
                while (runner.It < 1)
                {
                    SimplexRunnerOutput output = runner.Next();
                    runner = output.Next;
                }

                Assert.That(
                    runner.Expressions,
                    Is.EquivalentTo(
                        new Fraction[,]
                        {
                            {1, 0, 0},
                            {new Fraction(7,4), new Fraction(7,4), new Fraction(5, 4)},
                            {0, 0, 1},
                            {-1, 4, 0},
                            {0, 1, 0},
                            {new Fraction(3, 4), new Fraction(-1, 4), new Fraction(1, 4)},
                            {24, 40, 8}
                        }
                    )
                );
            }
        }

        [Test]
        // Sample Assessment Material A Level OCR MEI B Further Math Modelling with Algorithms Q5/Q5(ii)
        public void LpModelToTableauTest2()
        {
            string question = @"MAX 1/3x + 1/2y
ST
x + 2y <= 9
2x + 3y <= 14
2x + y <= 10
END";
            SimplexInterationRunner runner = ToSimplexRunner.Translate(question);
            using (Assert.EnterMultipleScope())
            {

                Assert.That(
                    runner.Expressions,
                    Is.EquivalentTo(
                        new Fraction[,]
                        {
                            {1, 0, 0, 0},
                            {0, 0, 1, 0},
                            {0, 0, 0, 1},
                            {0, 1, 0, 0},
                            {new Fraction(1, 6), new Fraction(-3, 4), new Fraction(-1, 4), new Fraction(1, 2)},
                            {0, new Fraction(1, 4), new Fraction(3, 4), new Fraction(-1, 2)},
                            {new Fraction(7, 3), 1, 4, 2}
                        }
                    )
                );

                Assert.That(
                    runner.Vars,
                    Is.EquivalentTo(new [] {"P", "x", "y", "s_1", "s_2", "s_3"})
                );

                

                Dictionary<string, Fraction> result = runner.Resolve();

                Assert.That(result, Does.ContainKey("P").WithValue(new Fraction(7, 4)));
                Assert.That(result, Does.ContainKey("x").WithValue((Fraction)4));
                Assert.That(result, Does.ContainKey("y").WithValue((Fraction)2));
                Assert.That(result, Does.ContainKey("s_1").WithValue((Fraction)1));
            }
        }
    }
}