using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFReader {

    /// <summary>
    /// Defines a contract for classes that support collecting and reporting error messages.
    /// </summary>
    /// <remarks>
    /// Implementing classes typically maintain an internal collection
    /// where errors can be added via the <see cref="AddError"/> method.
    /// </remarks>
    public interface IErrorReporter {

        /// <summary>
        /// Adds an error message to the implementing object's error collection.
        /// </summary>
        /// <param name="error">The error message string to record.</param>
        void AddError(string error);
    }
}
