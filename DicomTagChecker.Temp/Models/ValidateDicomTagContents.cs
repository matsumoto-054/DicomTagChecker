using NLog;
using System.Text.RegularExpressions;

namespace DicomTagChecker.Temp
{
    public class ValidateDicomTagContents
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // DicomタグのValidationチェック
        public bool HasErrorTag(DicomTagContents dicomTagContents)
        {
            try
            {
                // Validateするタグを順次追加
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
                logger.Info($"不正なPatientId：{patientId}");
                throw new InvalidPatientIdException("DICOMタグエラー：PatientId");
            }
        }
    }
}