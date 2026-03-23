using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NDjango.Admin.Export
{
    /// <summary>
    /// Represents a type used to perform exporting of the data stream to some format (like CSV or PDF) 
    /// </summary>
    public interface IDataExporter
    {
        /// <summary>
        /// Default settings of the exporter.
        /// </summary>
        public IDataExportSettings GetDefaultSettings(CultureInfo culture = null);

        /// <summary>
        /// Exports the specified data to the stream.
        /// </summary>
        /// <param name="data">The fetched data.</param>
        /// <param name="stream">The stream.</param>
        public void Export(INDjangoAdminResultSet data, Stream stream);

        /// <summary>
        /// Exports the specified data to the stream with the specified formats.
        /// </summary>
        /// <param name="data">The fetched data.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="settings">Some exporting settings.</param>
        public void Export(INDjangoAdminResultSet data, Stream stream, IDataExportSettings settings);

        /// <summary>
        /// Asynchronical version of <see cref="IDataExporter.Export(INDjangoAdminResultSet, Stream)"/> method.
        /// </summary>
        /// <param name="data">The fetched data.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>Task.</returns>
        public Task ExportAsync(INDjangoAdminResultSet data, Stream stream, CancellationToken ct = default);

        /// <summary>
        /// Asynchronical version of <see cref="IDataExporter.Export(INDjangoAdminResultSet,Stream, IDataExportSettings)" /> method.
        /// </summary>
        /// <param name="data">The fetched data.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="settings">Some exporting settings.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>Task.</returns>
        public Task ExportAsync(INDjangoAdminResultSet data, Stream stream, IDataExportSettings settings, CancellationToken ct = default);

        /// <summary>
        /// Gets the MIME content type of the exporting format.
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetContentType();

        /// <summary>
        /// Gets the preferred file extension by the exporting format.
        /// </summary>
        /// <returns>A string object that represents the file extension (without the dot)</returns>
        public string GetFileExtension();
    }
}
