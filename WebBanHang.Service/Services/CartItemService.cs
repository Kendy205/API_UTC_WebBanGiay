using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using WebBanHang.BLL.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.Services
{
    public class CartItemService : ICartItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICartService _cartService;

        public CartItemService(IUnitOfWork unitOfWork, IMapper mapper, ICartService cartService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cartService = cartService;
        }

        public async Task<bool> ClearCartAsync(long cartId)
        {
            var items = await _unitOfWork.CartItem.GetAllAsync(x => x.CartId == cartId);
            if (!items.Any()) return true;

            foreach (var item in items)
            {
                _unitOfWork.CartItem.Remove(item);
            }

            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}