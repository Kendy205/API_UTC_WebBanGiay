using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Service.IServices
{
    public interface IAddressService
    {
        Task<IEnumerable<AddressDto>> GetAllAsync();
        Task<AddressDto?> GetByIdAsync(long id);
        Task AddAsync(AddressDto dto);
        Task UpdateAsync(long id, AddressDto dto);
        Task DeleteAsync(long id, long userId);

        Task<IEnumerable<AddressDto>> GetByUserIdAsync(long userId);
    }
}
