using System;
using System.ComponentModel.DataAnnotations;

namespace C3D.IISOwinApp;

public class AppOptions
{
    [Required]
    public string? TestResponse { get; set; }

    public bool UseTestTimeservice => TestTime.HasValue;

    public DateTimeOffset? TestTime { get; set; }
}
