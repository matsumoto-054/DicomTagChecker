using System.Text.RegularExpressions;

namespace DicomTagChecker.Temp
{
    public class ValidateContents
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

        private void ValidatePatientId(string patientId)
        {
            if (string.IsNullOrWhiteSpace(patientId) || !Regex.IsMatch(patientId, "^[0-9a-zA-Z-]$"))
            {
                throw new InvalidPatientIdException("DICOMタグエラー：PatientID");
            }
        }
    }
}