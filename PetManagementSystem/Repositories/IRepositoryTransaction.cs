namespace PetManagementSystem.Repositories;

/// <summary>Giao dịch CSDL do repository mở; hủy (Dispose) mà chưa Commit sẽ tự rollback.</summary>
public interface IRepositoryTransaction : IAsyncDisposable
{
    Task CommitAsync();
}
