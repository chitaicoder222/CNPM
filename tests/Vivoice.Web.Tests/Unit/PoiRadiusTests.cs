// Purpose: Verifies invalid POI radii are rejected before create/update can access the database.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Vivoice.Web.Controllers;
using Vivoice.Web.Data;
using Vivoice.Web.Models;
using Vivoice.Web.Services;

namespace Vivoice.Web.Tests.Unit;

public sealed class PoiRadiusTests
{
    [Theory]
    [InlineData(-100d)]
    [InlineData(0d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public async Task InvalidRadius_IsRejectedByBothCreateAndUpdateBeforeDatabaseAccess(double radius)
    {
        await using var db = new VivoiceDbContext(new DbContextOptionsBuilder<VivoiceDbContext>()
            .UseSqlServer("Server=localhost;Database=RadiusValidationOnly;Integrated Security=True;Connect Timeout=1").Options);
        var controller = new AdminController(db, new AnalyticsService(db, new ConfigurationBuilder().Build()));
        var request = new PoiWriteRequest("test", 21m, 105m, radius, null, 1, true, "Địa điểm", "Mô tả");

        Assert.IsType<BadRequestObjectResult>(await controller.CreatePoi(request, CancellationToken.None));
        Assert.IsType<BadRequestObjectResult>(await controller.UpdatePoi(1, request, CancellationToken.None));
    }

    [Theory]
    [InlineData(0.01d)]
    [InlineData(100d)]
    [InlineData(123.45d)]
    public void PositiveFiniteRadius_IsAccepted(double radius) => Assert.True(PoiRadiusPolicy.IsValid(radius));
}
