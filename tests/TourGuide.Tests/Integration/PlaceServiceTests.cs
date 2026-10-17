using TourGuide.Api.Repositories;
using TourGuide.Api.Services;

namespace TourGuide.Tests.Integration;

public class PlaceServiceTests
{
    [Fact]
    public async Task Create_Update_Delete_RoundTripsThroughSqlite()
    {
        await using var db = new TestDb();
        var service = new PlaceService(new PlaceRepository(db.Context));

        var place = await service.CreateAsync("Ben Thanh", "Market", "HCMC", CancellationToken.None);
        Assert.NotEqual(0, place.Id);
        Assert.Single(await service.ListAsync("Ben", CancellationToken.None));

        Assert.True(await service.UpdateAsync(place.Id, "Ben Thanh Market", "Updated", "HCMC", CancellationToken.None));
        Assert.Equal("Ben Thanh Market", (await service.GetAsync(place.Id, CancellationToken.None))!.Name);
        Assert.True(await service.DeleteAsync(place.Id, CancellationToken.None));
        Assert.Null(await service.GetAsync(place.Id, CancellationToken.None));
    }
}
