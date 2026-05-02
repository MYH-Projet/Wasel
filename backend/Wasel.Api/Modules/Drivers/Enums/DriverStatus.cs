namespace Wasel.Api.Modules.Drivers.Enums;

    public enum DriverStatus
    {
        PendingVerification,
        Approved,
        Rejected,
        Suspended
    }

    /*PendingVerification : livreur en attente de validation
    Approved            : livreur validé par l’admin
    Rejected            : livreur refusé
    Suspended           : livreur suspendu après validation*/
