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
        [DataRow("�O�P�QabcABC-")]
        [DataRow("012������ABC-")]
        [DataRow("012abc�`�a�b-")]
        [DataRow("012abcABC�[")]
        [DataRow("012abcABC�[")]
        [DataRow("012abcABC-*")]
        [DataRow("012abcABC-��")]
        [DataRow("012abcABC-�")]
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