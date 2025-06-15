using System.Diagnostics.CodeAnalysis;
using Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Persistence.ValueConvertors;

[ExcludeFromCodeCoverage]
public sealed class StatusConvertor()
    : ValueConverter<Status, string>(
        d => d.ToString(), 
        d => Enum.Parse<Status>(d));
