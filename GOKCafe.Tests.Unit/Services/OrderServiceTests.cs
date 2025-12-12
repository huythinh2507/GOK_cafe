using FluentAssertions;
using GOKCafe.Application.DTOs.Order;
using GOKCafe.Application.Services;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using GOKCafe.Tests.Unit.Helpers;
using Moq;

namespace GOKCafe.Tests.Unit.Services;

public class OrderServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<Order>> _orderRepositoryMock;
    private readonly Mock<IRepository<Product>> _productRepositoryMock;
    private readonly Mock<IRepository<OrderItem>> _orderItemRepositoryMock;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orderRepositoryMock = new Mock<IRepository<Order>>();
        _productRepositoryMock = new Mock<IRepository<Product>>();
        _orderItemRepositoryMock = new Mock<IRepository<OrderItem>>();

        _unitOfWorkMock.Setup(u => u.Orders).Returns(_orderRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.OrderItems).Returns(_orderItemRepositoryMock.Object);

        _orderService = new OrderService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetOrdersAsync_ShouldReturnPaginatedOrders()
    {
        // Arrange
        var orders = new List<Order>
        {
            TestDataBuilder.CreateOrder(status: OrderStatus.Pending),
            TestDataBuilder.CreateOrder(status: OrderStatus.Confirmed),
            TestDataBuilder.CreateOrder(status: OrderStatus.Delivered)
        };

        _orderRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(orders);

        // Act
        var result = await _orderService.GetOrdersAsync(1, 10);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetOrdersAsync_ShouldFilterByStatus()
    {
        // Arrange
        var orders = new List<Order>
        {
            TestDataBuilder.CreateOrder(status: OrderStatus.Pending),
            TestDataBuilder.CreateOrder(status: OrderStatus.Pending),
            TestDataBuilder.CreateOrder(status: OrderStatus.Delivered)
        };

        _orderRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(orders);

        // Act
        var result = await _orderService.GetOrdersAsync(1, 10, "Pending");

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.Items.Should().AllSatisfy(o => o.Status.Should().Be("Pending"));
    }

    [Fact]
    public async Task GetOrderByIdAsync_ShouldReturnOrder_WhenOrderExists()
    {
        // Arrange
        var order = TestDataBuilder.CreateOrder();
        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.GetOrderByIdAsync(order.Id);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOrderByIdAsync_ShouldReturnFailure_WhenOrderNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _orderService.GetOrderByIdAsync(orderId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order not found");
    }

    [Fact]
    public async Task GetOrderByOrderNumberAsync_ShouldReturnOrder_WhenOrderExists()
    {
        // Arrange
        var order = TestDataBuilder.CreateOrder(orderNumber: "ORD-12345");
        _orderRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.GetOrderByOrderNumberAsync("ORD-12345");

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_ShouldUpdateStatus_WhenOrderExists()
    {
        // Arrange
        var order = TestDataBuilder.CreateOrder(status: OrderStatus.Pending);
        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _orderService.UpdateOrderStatusAsync(order.Id, "Confirmed");

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Order status updated successfully");
        _orderRepositoryMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_ShouldReturnFailure_WhenOrderNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _orderService.UpdateOrderStatusAsync(orderId, "Confirmed");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order not found");
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_ShouldReturnFailure_WhenStatusIsInvalid()
    {
        // Arrange
        var order = TestDataBuilder.CreateOrder();
        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.UpdateOrderStatusAsync(order.Id, "InvalidStatus");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid order status");
    }

    [Fact]
    public async Task CancelOrderAsync_ShouldCancelOrder_WhenOrderCanBeCancelled()
    {
        // Arrange
        var product = TestDataBuilder.CreateProduct(name: "Test Product");
        product.StockQuantity = 10;

        var orderItem = TestDataBuilder.CreateOrderItem(productId: product.Id, quantity: 2);
        var order = TestDataBuilder.CreateOrder(status: OrderStatus.Pending);
        order.OrderItems = new List<OrderItem> { orderItem };

        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id))
            .ReturnsAsync(product);

        // Act
        var result = await _orderService.CancelOrderAsync(order.Id);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Order cancelled successfully");
        product.StockQuantity.Should().Be(12);
    }

    [Fact]
    public async Task CancelOrderAsync_ShouldReturnFailure_WhenOrderIsDelivered()
    {
        // Arrange
        var order = TestDataBuilder.CreateOrder(status: OrderStatus.Delivered);
        order.OrderItems = new List<OrderItem>();

        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.CancelOrderAsync(order.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Cannot cancel this order");
    }
}
