using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

using NDjango.Admin.MongoDB.Authentication.Entities;
using NDjango.Admin.MongoDB.Tests.Fixtures;
using NDjango.Admin.Services;

namespace NDjango.Admin.MongoDB.Tests.IntegrationTests
{
    public class MongoAuthUserFieldVisibilityTests : IClassFixture<MongoAuthEnabledFixture>
    {
        private readonly MongoAuthEnabledFixture _fixture;

        public MongoAuthUserFieldVisibilityTests(MongoAuthEnabledFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData("DateJoined")]
        [InlineData("LastLogin")]
        public async Task LoadModel_MongoAuthUser_SystemManagedField_IsNotEditableAndHiddenOnCreateAsync(string fieldName)
        {
            // Arrange
            var host = _fixture.GetTestHost();
            var ndjangoAdminOptions = host.Services.GetRequiredService<NDjangoAdminOptions>();
            var manager = ndjangoAdminOptions.ManagerResolver(host.Services, ndjangoAdminOptions);

            // Act
            var model = await manager.GetModelAsync("__admin");
            var entity = model.EntityRoot.SubEntities
                .FirstOrDefault(e => e.Id == nameof(MongoAuthUser));
            Assert.NotNull(entity);
            var attr = entity.Attributes
                .FirstOrDefault(a => a.PropName == fieldName);

            // Assert
            Assert.NotNull(attr);
            Assert.False(attr.IsEditable, $"{fieldName} should not be editable");
            Assert.False(attr.ShowOnCreate, $"{fieldName} should not appear on the create form");
            Assert.True(attr.ShowOnEdit, $"{fieldName} should be visible on the edit form");
            Assert.True(attr.ShowOnView, $"{fieldName} should be visible on the view");
        }

        [Theory]
        [InlineData("Username")]
        [InlineData("Password")]
        [InlineData("IsSuperuser")]
        [InlineData("IsActive")]
        public async Task LoadModel_MongoAuthUser_EditableField_IsEditableAndShownOnCreateAsync(string fieldName)
        {
            // Arrange
            var host = _fixture.GetTestHost();
            var ndjangoAdminOptions = host.Services.GetRequiredService<NDjangoAdminOptions>();
            var manager = ndjangoAdminOptions.ManagerResolver(host.Services, ndjangoAdminOptions);

            // Act
            var model = await manager.GetModelAsync("__admin");
            var entity = model.EntityRoot.SubEntities
                .FirstOrDefault(e => e.Id == nameof(MongoAuthUser));
            Assert.NotNull(entity);
            var attr = entity.Attributes
                .FirstOrDefault(a => a.PropName == fieldName);

            // Assert
            Assert.NotNull(attr);
            Assert.True(attr.IsEditable, $"{fieldName} should be editable");
            Assert.True(attr.ShowOnCreate, $"{fieldName} should appear on the create form");
            Assert.True(attr.ShowOnEdit, $"{fieldName} should be visible on the edit form");
            Assert.True(attr.ShowOnView, $"{fieldName} should be visible on the view");
        }
    }
}
