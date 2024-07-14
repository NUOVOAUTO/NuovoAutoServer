using NuovoAutoServer.Model.Constants;
using NuovoAutoServer.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore;

namespace NuovoAutoServer.Repository.DBContext
{
    public class DbContextHelper
    {
        public static void ConfigureIdProperty(ModelBuilder modelBuilder, IMutableEntityType entityType)
        {
            var idProperty = entityType.FindProperty("Id");
            if (idProperty != null && idProperty.ClrType == typeof(Guid))
            {
                modelBuilder.Entity(entityType.ClrType)
                            .HasKey("Id");
                modelBuilder.Entity(entityType.ClrType)
                            .Property("Id")
                            .HasValueGenerator(typeof(SequentialGuidValueGenerator));
            }
        }
    }
}
