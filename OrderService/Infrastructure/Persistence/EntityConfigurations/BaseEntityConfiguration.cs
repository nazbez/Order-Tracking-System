using System.Diagnostics.CodeAnalysis;
using Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations;

[ExcludeFromCodeCoverage]
public abstract class BaseEntityConfiguration<TEntity, TId> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity<TId>
    where TId : struct
{
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(e => e.Id);
        
        if (typeof(TId) == typeof(int))
        {
            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();
        }

        builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false);
        
        ConfigureEntity(builder);
    }

    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
}
