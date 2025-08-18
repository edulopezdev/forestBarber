using System.Threading.Tasks;

namespace backend.Services.Interfaces
{
    public interface IStockService
    {
        Task<bool> UpdateStockAsync(int productoServicioId, int cantidad);
        Task DevolverStockAsync(int productoServicioId, int cantidad);
        Task<int> GetStockAsync(int productoServicioId);
        Task SendLowStockAlertAsync(int productoServicioId);
    }
}
