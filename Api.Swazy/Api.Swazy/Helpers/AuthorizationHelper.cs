using Api.Swazy.Types;

namespace Api.Swazy.Helpers;

public static class AuthorizationHelper
{
    public static bool CanRemoveEmployee(BusinessRole currentUserRole, BusinessRole targetEmployeeRole)
    {
        return currentUserRole switch
        {
            BusinessRole.Owner => true, // Owner can remove anyone
            BusinessRole.Manager => false, // Manager cannot remove anyone
            BusinessRole.Employee => false, // Employee cannot remove anyone
            _ => false
        };
    }

    public static bool CanUpdateEmployeeRole(BusinessRole currentUserRole, BusinessRole targetCurrentRole, BusinessRole targetNewRole)
    {
        // Only Owners can change roles
        if (currentUserRole != BusinessRole.Owner)
        {
            return false;
        }

        // Owners can change any role to any role
        return true;
    }

    public static bool CanInviteEmployee(BusinessRole currentUserRole)
    {
        return currentUserRole == BusinessRole.Manager || currentUserRole == BusinessRole.Owner;
    }

    public static bool CanUpdateBusinessSettings(BusinessRole currentUserRole)
    {
        return currentUserRole == BusinessRole.Owner;
    }

    public static bool CanManageServices(BusinessRole currentUserRole)
    {
        return currentUserRole == BusinessRole.Manager || currentUserRole == BusinessRole.Owner;
    }

    public static bool CanManageSchedules(BusinessRole currentUserRole)
    {
        return currentUserRole == BusinessRole.Manager || currentUserRole == BusinessRole.Owner;
    }

    public static bool CanViewAllBookings(BusinessRole currentUserRole)
    {
        return currentUserRole == BusinessRole.Manager || currentUserRole == BusinessRole.Owner;
    }

    public static BusinessRole ParseBusinessRole(string? roleString)
    {
        if (string.IsNullOrEmpty(roleString))
        {
            return BusinessRole.Employee; // Default to lowest privilege
        }

        return Enum.TryParse<BusinessRole>(roleString, true, out var role)
            ? role
            : BusinessRole.Employee;
    }
}
