using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourierPortal.Core.Domain.Entities.Configuration
{
    public class CourierApplicantConfiguration : IEntityTypeConfiguration<CourierApplicant>
    {
        public void Configure(EntityTypeBuilder<CourierApplicant> builder)
        {
            builder
                .Property(x => x.CourierId).IsConcurrencyToken();

            builder
                .Property(x => x.DeclarationDate).IsConcurrencyToken();
        }
    }
}
