namespace SkyRoute.Domain.Models;

// Enum representing the cabin class of a flight (Economy / Business / First).
// Lives in Domain because it is a business rule: the possible values do not depend on any framework.
public enum CabinClass
{
    Economy,

    Business,

    First
}
