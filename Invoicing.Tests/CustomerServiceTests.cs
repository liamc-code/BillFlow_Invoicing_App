/* CustomerServiceTests.cs
* Invoicing App
* Liam Conn
* Unit tests for verifying functionality of some
* customer service methods.
*
*/

using Invoicing.Entities;
using Invoicing.Interfaces;
using Moq;
using Xunit;

namespace Invoicing.Tests
{
    public class CustomerServiceTests
    {
        [Fact]
        public async Task GetCustomersByGroupAsync_ShouldReturnCorrectCustomers()
        {
            // Arrange
            var mockCustomerService = new Mock<ICustomerService>();
            var group = "A-E";
            var expectedCustomers = new List<Customer>
            {
                new Customer { CustomerId = 1, Name = "Alice" },
                new Customer { CustomerId = 2, Name = "Andrew" }
            };

            mockCustomerService.Setup(service => service.GetCustomersByGroupAsync(group))
                               .ReturnsAsync(expectedCustomers);

            // Act
            var result = await mockCustomerService.Object.GetCustomersByGroupAsync(group);

            // Assert
            Assert.NotNull(result); // Ensure the result is not null
            Assert.Equal(expectedCustomers.Count, result.Count()); // Check count matches
            Assert.All(result, customer => // Verify each customer matches expected values
            {
                var matchingCustomer = expectedCustomers.FirstOrDefault(c => c.CustomerId == customer.CustomerId);
                Assert.NotNull(matchingCustomer);
                Assert.Equal(matchingCustomer!.Name, customer.Name);
            });
        }

        [Fact]
        public async Task AddCustomerAsync_ShouldCallServiceWithCorrectCustomer()
        {
            // Arrange
            var mockCustomerService = new Mock<ICustomerService>();
            var newCustomer = new Customer { Name = "Bob", City = "Toronto", ProvinceOrState = "ON" };

            mockCustomerService.Setup(service => service.AddCustomerAsync(newCustomer))
                               .Returns(Task.CompletedTask);

            // Act
            await mockCustomerService.Object.AddCustomerAsync(newCustomer);

            // Assert
            mockCustomerService.Verify(service => service.AddCustomerAsync(newCustomer), Times.Once);
        }

        [Fact]
        public async Task SoftDeleteCustomerAsync_ShouldMarkCustomerAsDeleted()
        {
            // Arrange
            var mockCustomerService = new Mock<ICustomerService>();
            var customerId = 3;

            mockCustomerService.Setup(service => service.SoftDeleteCustomerAsync(customerId))
                               .Returns(Task.CompletedTask);

            // Act
            await mockCustomerService.Object.SoftDeleteCustomerAsync(customerId);

            // Assert
            mockCustomerService.Verify(service => service.SoftDeleteCustomerAsync(customerId), Times.Once);
        }
    }
}