using StockifyPlus.Exceptions;
using StockifyPlus.Hubs;
using StockifyPlus.Models;
using StockifyPlus.Models.Enums;
using StockifyPlus.Repositories.Interfaces;
using StockifyPlus.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace StockifyPlus.Services.Implementations
{
    public class StockMovementService : IStockMovementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductService _productService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<StockMovementService> _logger;

        public StockMovementService(
            IUnitOfWork unitOfWork,
            IProductService productService,
            IHubContext<NotificationHub> hubContext,
            ILogger<StockMovementService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<StockMovement>> GetAllMovementsAsync()
        {
            var movements = await _unitOfWork.StockMovementRepository.IncludeProperties(m => m.Product)
                .OrderByDescending(m => m.MovementDate).ToListAsync();
            return movements;
        }

        public async Task<IEnumerable<StockMovement>> GetMovementsByProductAsync(int productId)
        {
            if (productId <= 0)
                throw new ValidationException("Ürün ID geçerli olmalıdır.");

            await _productService.GetProductByIdAsync(productId);

            var movements = await _unitOfWork.StockMovementRepository.IncludeProperties(m => m.Product)
                .Where(m => m.ProductId == productId)
                .OrderByDescending(m => m.MovementDate).ToListAsync();

            return movements;
        }

        public async Task<IEnumerable<StockMovement>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ValidationException("Başlangıç tarihi bitiş tarihinden sonra olamaz.");

            var movements = await _unitOfWork.StockMovementRepository.IncludeProperties(m => m.Product)
                .Where(m => m.MovementDate >= startDate && m.MovementDate <= endDate)
                .OrderByDescending(m => m.MovementDate).ToListAsync();

            return movements;
        }

        public async Task<StockMovement> RecordStockInAsync(int productId, int quantity, string? description)
        {
            if (quantity <= 0)
                throw new ValidationException("Giriş miktarı 0'dan büyük olmalıdır.");

            var product = await _productService.GetProductByIdAsync(productId);

            product.StockQuantity += quantity;

            var movement = new StockMovement
            {
                ProductId = productId,
                MovementType = MovementType.Giriş,
                Quantity = quantity,
                MovementDate = DateTime.Now,
                Description = description?.Trim() ?? string.Empty
            };

            await _unitOfWork.StockMovementRepository.AddAsync(movement);
            _unitOfWork.ProductRepository.Update(product);
            await _unitOfWork.SaveChangesAsync();
            await NotifyCriticalStockAsync(product);

            return movement;
        }

        public async Task<StockMovement> RecordStockOutAsync(int productId, int quantity, string? description)
        {
            if (quantity <= 0)
                throw new ValidationException("Çıkış miktarı 0'dan büyük olmalıdır.");

            var product = await _productService.GetProductByIdAsync(productId);

            if (product.StockQuantity < quantity)
                throw new BusinessException($"Ürün için yeterli stok yok. Mevcut: {product.StockQuantity}, İstenen: {quantity}");

            product.StockQuantity -= quantity;

            var movement = new StockMovement
            {
                ProductId = productId,
                MovementType = MovementType.Çıkış,
                Quantity = quantity,
                MovementDate = DateTime.Now,
                Description = description?.Trim() ?? string.Empty
            };

            await _unitOfWork.StockMovementRepository.AddAsync(movement);
            _unitOfWork.ProductRepository.Update(product);
            await _unitOfWork.SaveChangesAsync();

            return movement;
        }

        public async Task<StockMovement> RecordStockTransferAsync(int productId, int quantity, string? description)
        {
            if (quantity <= 0)
                throw new ValidationException("Transfer miktarı 0'dan büyük olmalıdır.");

            var product = await _productService.GetProductByIdAsync(productId);

            if (product.StockQuantity < quantity)
                throw new BusinessException($"Transfer için yeterli stok yok.");

            var movement = new StockMovement
            {
                ProductId = productId,
                MovementType = MovementType.Transfer,
                Quantity = quantity,
                MovementDate = DateTime.Now,
                Description = description?.Trim() ?? string.Empty
            };

            await _unitOfWork.StockMovementRepository.AddAsync(movement);
            await _unitOfWork.SaveChangesAsync();

            return movement;
        }

        public async Task<StockMovement> RecordStockAdjustmentAsync(int productId, int quantity, string? description)
        {
            var product = await _productService.GetProductByIdAsync(productId);

            if (product.StockQuantity + quantity < 0)
                throw new BusinessException("Ayarlama sonrası stok miktarı negatif olamaz.");

            product.StockQuantity += quantity;
            var stockReduced = quantity < 0;

            var movement = new StockMovement
            {
                ProductId = productId,
                MovementType = MovementType.Ayarlama,
                Quantity = quantity,
                MovementDate = DateTime.Now,
                Description = description?.Trim() ?? string.Empty
            };

            await _unitOfWork.StockMovementRepository.AddAsync(movement);
            _unitOfWork.ProductRepository.Update(product);
            await _unitOfWork.SaveChangesAsync();

            if (stockReduced)
            {
                await NotifyCriticalStockAsync(product);
            }

            return movement;
        }

        public async Task ReverseMovementAsync(int movementId)
        {
            if (movementId <= 0)
                throw new ValidationException("Hareket ID geçerli olmalıdır.");

            var movement = await _unitOfWork.StockMovementRepository.GetByIdAsync(movementId);
            if (movement == null)
                throw new NotFoundException(nameof(StockMovement), movementId);

            var product = await _productService.GetProductByIdAsync(movement.ProductId);

            switch (movement.MovementType)
            {
                case MovementType.Giriş:
                    if (product.StockQuantity < movement.Quantity)
                        throw new BusinessException("Stok hareketi geri alınamaz.");
                    product.StockQuantity -= movement.Quantity;
                    break;

                case MovementType.Çıkış:
                    product.StockQuantity += movement.Quantity;
                    break;

                case MovementType.Transfer:
                    product.StockQuantity += movement.Quantity;
                    break;

                case MovementType.Ayarlama:
                    if (product.StockQuantity - movement.Quantity < 0)
                        throw new BusinessException("Hareket geri alınamaz.");
                    product.StockQuantity -= movement.Quantity;
                    break;
            }

            _unitOfWork.StockMovementRepository.Delete(movement);
            _unitOfWork.ProductRepository.Update(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<int> GetCurrentStockAsync(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            return product.StockQuantity;
        }

        private async Task NotifyCriticalStockAsync(Product product)
        {
            if (product.StockQuantity > product.CriticalStockLevel)
            {
                return;
            }

            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveStockAlert", new
                {
                    ProductName = product.Name,
                    RemainingStock = product.StockQuantity,
                    CriticalLevel = product.CriticalStockLevel,
                    AlertTime = DateTime.Now.ToString("HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Kritik stok bildirimi yayinlanirken hata olustu. ProductId: {ProductId}", product.Id);
            }
        }
    }
}
