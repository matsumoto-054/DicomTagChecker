using DicomTagChecker.Temp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DicomTagUnitTest
{
    [TestClass]
    public class ValidateContents_ValidatePatientId_Should
    {
        [TestMethod]
        public void NotThrowIfPatientIdIsValid()
        {
            var validate = new ValidateContents();
            validate.ValidatePatientId("012abcABC-");
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("ÇOÇPÇQabcABC-")]
        [DataRow("012ÇÅÇÇÇÉABC-")]
        [DataRow("012abcÇ`ÇaÇb-")]
        [DataRow("012abcABCÅ[")]
        [DataRow("012abcABCÅ[")]
        [DataRow("012abcABC-*")]
        [DataRow("012abcABC-Ç†")]
        [DataRow("012abcABC-±")]
        public void ThrowIfPatientIdIsInvalid(string value)
        {
            var validate = new ValidateContents();
            Assert.ThrowsException<InvalidPatientIdException>
            (
                () => { validate.ValidatePatientId(value); }
            );
        }
    }
}