using DicomTagChecker.Temp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DicomTagUnitTest
{
    [TestClass]
    public class ValidateDicomTagContents_ValidatePatientId_Should
    {
        /// <summary>
        /// 1~32•¶š‚Ì”¼Šp‰p”š(+ƒnƒCƒtƒ“)‚Í—áŠO‚Æ‚È‚ç‚È‚¢
        /// </summary>
        [TestMethod]
        public void NotThrowIfPatientIdIsValid()
        {
            var validate = new ValidateDicomTagContents();
            validate.ValidatePatientId("123456789-123456789-123456789-aA");
        }

        /// <summary>
        /// —áŠO‚ª”­¶‚·‚éƒpƒ^[ƒ“
        /// </summary>
        /// <param name="value"></param>
        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("123456789012345678901234567890123")]
        [DataRow("‚O‚P‚QabcABC-")]
        [DataRow("012‚‚‚‚ƒABC-")]
        [DataRow("012abc‚`‚a‚b-")]
        [DataRow("012abcABC[")]
        [DataRow("012abcABC- ")]
        [DataRow("012abcABC-*")]
        [DataRow("012abcABC-‚ ")]
        [DataRow("012abcABC-±")]
        public void ThrowIfPatientIdIsInvalid(string value)
        {
            var validate = new ValidateDicomTagContents();
            Assert.ThrowsException<InvalidPatientIdException>
            (
                () => { validate.ValidatePatientId(value); }
            );
        }
    }
}