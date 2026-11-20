using Microsoft.Extensions.Logging;

namespace AtaGreenTech.SharedKernel.Security;

/// <summary>Başlangıçta kayıtlı perm:* bağlamalarını loglar (#70 denetim).</summary>
public static class PermissionPolicyStartupAudit
{
    public static void LogRegisteredPolicies(ILogger logger)
    {
        foreach (var (policyName, permission) in AuthorizationPolicies.Bindings)
        {
            logger.LogInformation(
                "Permission policy registered: {PolicyName} -> {Permission}",
                policyName,
                permission
            );
        }
    }
}
