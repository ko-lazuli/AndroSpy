using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server.Views
{
    /// <summary>
    /// About the application
    /// </summary>
    public partial class About : Form
    {

        string alphabet;

        /// <summary>
        /// Creates a new <see cref="About"/> instance
        /// </summary>
        public About()
        {
            InitializeComponent();
            LoadAlphabet();
        }

        /// <summary>
        /// Loads the Latin alphabet
        /// </summary>
        private void LoadAlphabet()
        {
            var letters = Enumerable.Range(33, 126 - 33).Select(l => (char)l);
            alphabet = string.Join("", letters);
        }

        /// <summary>
        /// Handles the about form loading event
        /// </summary>
        /// <param name="sender"><see cref="About"/> from</param>
        /// <param name="e"><see cref="EventArgs"/></param>
        private async void OnAboutLoad(object sender, EventArgs e)
        {
            string result = "simpsons";
            StringBuilder sb = LoadsStringBuilder(result);
            await PrintsLetterByLetter(result, sb);
        }

        /// <summary>
        /// Prints letter by letter of result on the label
        /// </summary>
        /// <param name="result">Result to be printed on the label</param>
        /// <param name="sb"><see cref="StringBuilder"/> that builds the result</param>
        /// <returns>A task that returns no value</returns>
        private async Task PrintsLetterByLetter(string result, StringBuilder sb)
        {
            for (int i = 0; i < result.Length; i++)
            {
                for (int j = 0; j < alphabet.Length; j++)
                {
                    var isTheSameLetter = await FillLabelWithResultAnAnimate(sb, result, outerIndex: i, innerIndex: j);
                    if (isTheSameLetter)
                    {
                        break;
                    }
                }
                await Task.Delay(15);
            }
        }


        /// <summary>
        /// Display the result to the user after a cool animation
        /// </summary>
        /// <param name="builder">String Builder will build the output</param>
        /// <param name="outerIndex">Inner index to compare the result with a letter</param>
        /// <param name="innerIndex">OOuter index to compare the letter with a result</param>
        /// <returns>A task of whether the result letter is the same as the alphabet one</returns>
        private async Task<bool> FillLabelWithResultAnAnimate(StringBuilder builder, string result, int outerIndex, int innerIndex)
        {
            Application.DoEvents();
            await Task.Delay(15);
            builder[outerIndex] = (outerIndex == 0) ? alphabet[innerIndex].ToString().ToUpper().ToCharArray()[0] : alphabet[innerIndex];
            label2.Text = builder.ToString();
            var resultLetter = result[outerIndex].ToString().ToLower();
            var alphabetLetter = alphabet[innerIndex].ToString().ToLower();
            return IsTheSameLetter(resultLetter, alphabetLetter);
        }


        /// <summary>
        /// Verifies if 2 strings are equal
        /// </summary>
        /// <param name="resultLetter">Letter of the printed result</param>
        /// <param name="alphabetLetter">Letter of alphabet</param>
        /// <returns></returns>
        private bool IsTheSameLetter(string resultLetter, string alphabetLetter)
        {
            return resultLetter == alphabetLetter;
        }

        /// <summary>
        /// Loads <see cref="StringBuilder"/> and fill it in with white spaces
        /// </summary>
        /// <param name="result">Text to be stored in string builder</param>
        /// <returns>A new string builder white spaced</returns>
        private StringBuilder LoadsStringBuilder(string result)
        {
            StringBuilder sb = new StringBuilder(result.Length);

            for (int i = 0; i < sb.Capacity; i++)
            {
                sb.Append(" ");
            }

            return sb;
        }
    }
}
