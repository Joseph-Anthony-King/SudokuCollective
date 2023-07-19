namespace SudokuCollective.Api.Models
{
    /// <summary>
    /// A class to manage the error view.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// The property for the request id.
        /// </summary>
        public string RequestId { get; set; }
        /// <summary>
        /// A property to show the request id if present.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
