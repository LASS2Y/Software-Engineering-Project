using System.Collections.Generic;
using KitBox.Models;

namespace KitBox.Services.Interfaces;

/// <summary>
/// Validates locker and cabinet configurations according to the KitBox catalog rules.
/// </summary>
public interface ILockerValidationService
{
    /// <summary>
    /// Returns true if the given dimensions are available in the catalog.
    /// </summary>
    bool AreDimensionsValid(double height, double width, double depth, bool hasDoors);

    /// <summary>
    /// Returns true if the locker count does not exceed the maximum of 7.
    /// </summary>
    bool IsLockerCountValid(int lockerCount);

    /// <summary>
    /// Validates a full cabinet configuration before placing an order.
    /// Returns a list of error messages (empty if valid).
    /// </summary>
    List<string> ValidateCabinet(List<Locker> lockers);
}
