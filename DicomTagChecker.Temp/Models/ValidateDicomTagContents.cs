using System.Text.RegularExpressions;

namespace DicomTagChecker.Temp
{
    public class ValidateDicomTagContents
    {
        public bool HasErrorTag(DicomTagContents dicomTagContents)
        {
            try
            {
                this.ValidatePatientId(dicomTagContents.PatientId);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 患者IDを仮に1~32文字とする
        /// </summary>
        /// <param name="patientId"></param>
        public void ValidatePatientId(string patientId)
        {
            if (string.IsNullOrWhiteSpace(patientId) || !Regex.IsMatch(patientId, "^[0-9a-zA-Z-]{1,32}$"))
            {
                throw new InvalidPatientIdException("DICOMタグエラー：PatientID");
            }
        }
    }
}