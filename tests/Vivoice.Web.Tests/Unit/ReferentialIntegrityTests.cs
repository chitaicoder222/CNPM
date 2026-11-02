// Purpose: Protects every relationship from cascading deletes and automatic foreign-key nulling.
// Responsibility: Verifies the SQL Server model and EF's behavior with already-loaded dependents without a database connection.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Vivoice.Web.Data;

namespace Vivoice.Web.Tests.Unit;

public sealed class ReferentialIntegrityTests
{
    private static VivoiceDbContext CreateContext() => new(new DbContextOptionsBuilder<VivoiceDbContext>()
        .UseSqlServer("Server=localhost;Database=IntegrityModelOnly;Integrated Security=True;Connect Timeout=1").Options);

    public static IEnumerable<object[]> Relationships()
    {
        using var db = CreateContext();
        return db.Model.GetEntityTypes().SelectMany(entity => entity.GetForeignKeys())
            .Select(foreignKey => new object[] { foreignKey.DeclaringEntityType.ClrType, foreignKey.Properties.Single().Name })
            .ToArray();
    }

    [Fact]
    public void AllForeignKeysUseDatabaseNoAction()
    {
        using var db = CreateContext();
        var foreignKeys = db.Model.GetEntityTypes().SelectMany(entity => entity.GetForeignKeys()).ToArray();
        Assert.Equal(10, foreignKeys.Length);
        Assert.All(foreignKeys, foreignKey => Assert.Equal(DeleteBehavior.ClientNoAction, foreignKey.DeleteBehavior));
        var constraints = db.Model.GetRelationalModel().Tables.SelectMany(table => table.ForeignKeyConstraints).ToArray();
        Assert.Equal(10, constraints.Length);
        Assert.All(constraints, constraint => Assert.Equal(ReferentialAction.NoAction, constraint.OnDeleteAction));
    }

    [Theory]
    [MemberData(nameof(Relationships))]
    public void DeletingLoadedParentPreservesDependentAndItsForeignKey(Type dependentType, string foreignKeyName)
    {
        using var db = CreateContext();
        var foreignKey = db.Model.FindEntityType(dependentType)!.GetForeignKeys()
            .Single(key => key.Properties.Single().Name == foreignKeyName);
        var principal = Activator.CreateInstance(foreignKey.PrincipalEntityType.ClrType)!;
        var dependent = Activator.CreateInstance(dependentType)!;
        foreignKey.PrincipalKey.Properties.Single().PropertyInfo!.SetValue(principal, 1001L);
        db.Model.FindEntityType(dependentType)!.FindPrimaryKey()!.Properties.Single().PropertyInfo!.SetValue(dependent, 2001L);
        foreignKey.Properties.Single().PropertyInfo!.SetValue(dependent, 1001L);
        foreignKey.DependentToPrincipal!.PropertyInfo!.SetValue(dependent, principal);
        db.Attach(principal);
        db.Attach(dependent);

        db.Remove(principal);
        db.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Deleted, db.Entry(principal).State);
        Assert.Equal(EntityState.Unchanged, db.Entry(dependent).State);
        Assert.Equal(1001L, db.Entry(dependent).Property(foreignKeyName).CurrentValue);
    }
}
