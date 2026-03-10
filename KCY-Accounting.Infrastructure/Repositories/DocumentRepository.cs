using KCY_Accounting.Core.Interfaces;
using KCY_Accounting.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KCY_Accounting.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _db;

    /// <summary>Root folder where all uploaded documents are stored on disk.</summary>
    private readonly string _storageRoot;

    public DocumentRepository(AppDbContext db)
    {
        _db = db;
        // Store documents next to the SQLite database file in a "Documents" subfolder.
        _storageRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "KCY-Accounting", "Documents");
        Directory.CreateDirectory(_storageRoot);
    }

    public async Task<IEnumerable<OrderDocument>> GetAllAsync() =>
        await _db.OrderDocuments.ToListAsync();

    public async Task<OrderDocument?> GetByIdAsync(int id) =>
        await _db.OrderDocuments.FindAsync(id);

    public async Task<IEnumerable<OrderDocument>> GetByOrderIdAsync(int transportOrderId) =>
        await _db.OrderDocuments
            .Where(d => d.TransportOrderId == transportOrderId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();

    public async Task<OrderDocument> AddFileAsync(int transportOrderId, string sourceFilePath, DocumentType type)
    {
        // Create a unique filename to avoid collisions between orders.
        var ext = Path.GetExtension(sourceFilePath);
        var uniqueName = $"{transportOrderId}_{type}_{Guid.NewGuid():N}{ext}";
        var destPath = Path.Combine(_storageRoot, uniqueName);

        File.Copy(sourceFilePath, destPath, overwrite: true);

        var doc = new OrderDocument
        {
            TransportOrderId = transportOrderId,
            Type = type,
            OriginalFileName = Path.GetFileName(sourceFilePath),
            RelativeFilePath = uniqueName,
            UploadedAt = DateTime.UtcNow
        };

        _db.OrderDocuments.Add(doc);
        await _db.SaveChangesAsync();
        return doc;
    }

    public string GetAbsolutePath(string relativeFilePath) =>
        Path.Combine(_storageRoot, relativeFilePath);

    public async Task AddAsync(OrderDocument entity)
    {
        _db.OrderDocuments.Add(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(OrderDocument entity)
    {
        _db.OrderDocuments.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.OrderDocuments.FindAsync(id);
        if (entity != null)
        {
            // Also remove the physical file if it exists.
            var fullPath = GetAbsolutePath(entity.RelativeFilePath);
            if (File.Exists(fullPath)) File.Delete(fullPath);
            _db.OrderDocuments.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }
}

