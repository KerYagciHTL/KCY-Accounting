using KCY_Accounting.Core.Models;

namespace KCY_Accounting.Core.Interfaces;

public interface IDocumentRepository : IRepository<OrderDocument>
{
    Task<IEnumerable<OrderDocument>> GetByOrderIdAsync(int transportOrderId);

    /// <summary>
    /// Copies the source file into the app's document store and persists the record.
    /// Returns the created OrderDocument entity.
    /// </summary>
    Task<OrderDocument> AddFileAsync(int transportOrderId, string sourceFilePath, DocumentType type);

    /// <summary>Resolves the relative path to an absolute file system path.</summary>
    string GetAbsolutePath(string relativeFilePath);
}

